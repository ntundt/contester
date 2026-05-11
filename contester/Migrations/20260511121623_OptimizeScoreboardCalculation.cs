using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace contester.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeScoreboardCalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

DROP VIEW ""Scoreboard"";

ALTER TABLE ""ScoreboardBase"" DROP COLUMN ""FirstName"";
ALTER TABLE ""ScoreboardBase"" DROP COLUMN ""LastName"";
ALTER TABLE ""ScoreboardBase"" DROP COLUMN ""Patronymic"";
ALTER TABLE ""ScoreboardBase"" DROP COLUMN ""AdditionalInfo"";
ALTER TABLE ""ScoreboardBase"" DROP COLUMN ""FinalGrade"";
ALTER TABLE ""ScoreboardBase"" DROP COLUMN latest_solution_time;
ALTER TABLE ""ScoreboardBase"" DROP COLUMN ""Problems"";
ALTER TABLE ""ScoreboardBase"" ADD COLUMN ""Problems"" ""ScoreboardProblemEntry""[];

CREATE VIEW ""Scoreboard"" AS
SELECT sb.""ContestId"",
	sb.""UserId"",
	u.""FirstName"",
	u.""LastName"",
	u.""Patronymic"",
	u.""AdditionalInfo"",
	0 AS ""Fee"",
	sum(p.""Grade"") AS ""FinalGrade"",
	to_jsonb(sb.""Problems"") AS ""Problems""
FROM ""ScoreboardBase"" sb
	LEFT JOIN ""Users"" u ON sb.""UserId"" = u.""Id""
	LEFT JOIN LATERAL unnest(sb.""Problems"") p(""ProblemId"", ""Name"", ""AttemptsCount"", ""IsSolved"", ""MaxGrade"", ""Grade"", ""SolvedAt"", ""SolvingAttemptId"") ON true
GROUP BY sb.""ContestId"", sb.""UserId"", u.""FirstName"", u.""LastName"", u.""Patronymic"", u.""AdditionalInfo""
ORDER BY (sum(p.""Grade"")) DESC, (max(p.""SolvedAt""));

CREATE OR REPLACE PROCEDURE public.update_scoreboard_adjustments(
	IN attempt_id uuid)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    attempt ""Attempts"";
    problem ""Problems"";

    commission_members uuid[];
    grade_adjustments_count int;
    grade_adjustments_sum int;

    problem_index int;
    new_problem_grade int;
    old_problem_grade int;
    old_final_grade int;
BEGIN
	SELECT *
	INTO attempt
	FROM ""Attempts""
	WHERE ""Id"" = attempt_id;

	SELECT *
	INTO problem
	FROM ""Problems""
	WHERE ""Id"" = attempt.""ProblemId"";

	WITH problems_numbered AS (
		SELECT ""Id"", row_number() OVER (ORDER BY ""Ordinal"") AS row_index
		FROM ""Problems""
		WHERE ""ContestId"" = problem.""ContestId""
	)
	SELECT row_index
	INTO problem_index
	FROM problems_numbered
	WHERE ""Id"" = problem.""Id"";

	SELECT array_agg(""CommissionMembersId"")
	INTO commission_members
	FROM ""ContestCommissionMembers""
	WHERE ""ContestId"" = problem.""ContestId"";

	SELECT count(*), coalesce(sum(""Grade""),0)
	INTO grade_adjustments_count, grade_adjustments_sum
	FROM ""GradeAdjustments""
	WHERE ""AttemptId"" = attempt.""Id""
		-- Filter out any users later removed from commission members
		AND ""UserId"" = ANY(commission_members);

	IF (SELECT ""Problems""[problem_index].""SolvingAttemptId""
		FROM ""ScoreboardBase""
		WHERE ""ContestId"" = problem.""ContestId"" AND ""UserId"" = attempt.""AuthorId"") != attempt.""Id"" THEN
		RETURN;
	END IF;

	new_problem_grade := ((array_length(commission_members, 1) - grade_adjustments_count) * problem.""MaxGrade"" + grade_adjustments_sum) / array_length(commission_members, 1);

	SELECT ""Problems""[problem_index].""Grade""
	INTO old_problem_grade
	FROM ""ScoreboardBase""
	WHERE ""ContestId"" = problem.""ContestId"" AND ""UserId"" = attempt.""AuthorId"";

	UPDATE ""ScoreboardBase""
	SET ""Problems""[problem_index].""Grade"" = new_problem_grade
	WHERE ""ContestId"" = problem.""ContestId"" AND ""UserId"" = attempt.""AuthorId"";
END;
$BODY$;


CREATE OR REPLACE PROCEDURE update_scoreboard_incrementally(
	IN attempt_id uuid,
	IN re_evaluation boolean DEFAULT false)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    attempt ""Attempts"" := (
        SELECT (a.*)::""Attempts""
        FROM ""Attempts"" a
        WHERE ""Id"" = attempt_id
    );

    problem ""Problems"" := (
        SELECT (p.*)::""Problems""
        FROM ""Problems"" p
        WHERE p.""Id"" = attempt.""ProblemId""
    );

    author ""Users"" := (
        SELECT (u.*)::""Users""
        FROM ""Users"" u
        WHERE u.""Id"" = attempt.""AuthorId""
    );

    default_problems public.""ScoreboardProblemEntry""[] := (
        SELECT array_agg(row(p.""Id"", p.""Name"", 0, false, p.""MaxGrade"", 0, NULL, NULL)
            ::""ScoreboardProblemEntry"" ORDER BY ""Ordinal"")
        FROM ""Problems"" p
        WHERE ""ContestId"" = problem.""ContestId""
    );

    ordinality_to_index_map int[] := (
        WITH src AS (
            SELECT ""Ordinal"" AS value,
                row_number() OVER (ORDER BY ""Ordinal"") AS idx
            FROM ""Problems""
            WHERE ""ContestId"" = problem.""ContestId""
        )
        SELECT array_agg(idx ORDER BY value)
        FROM src
    );

	scoreboard_problem_entry ""ScoreboardProblemEntry"";
	latest_problem_solution_time timestamptz;
BEGIN
    IF (SELECT count(*)
        FROM ""ScoreboardBase""
        WHERE ""ContestId"" = problem.""ContestId"" AND ""UserId"" = attempt.""AuthorId"") = 0
    THEN
        INSERT INTO ""ScoreboardBase""(""ContestId"", ""UserId"", ""Fee"", ""Problems"")
        VALUES (
            problem.""ContestId"", attempt.""AuthorId"", 0, default_problems
        );
    END IF;

	SELECT (""Problems""[ordinality_to_index_map[problem.""Ordinal""]]).*
	INTO scoreboard_problem_entry
	FROM ""ScoreboardBase""
	WHERE ""ContestId"" = problem.""ContestId"" AND ""UserId"" = attempt.""AuthorId"";

	IF attempt.""Status"" = 5 /* Accepted */ THEN
		scoreboard_problem_entry.""IsSolved"" = true;
	END IF;

	IF (scoreboard_problem_entry.""SolvedAt"" IS NULL OR scoreboard_problem_entry.""SolvedAt"" < attempt.""CreatedAt"") AND attempt.""Status"" = 5 /* Accepted */ THEN
		scoreboard_problem_entry.""Grade"" := scoreboard_problem_entry.""MaxGrade"";
		scoreboard_problem_entry.""SolvingAttemptId"" := attempt.""Id"";
	END IF;

	IF attempt.""Status"" = 5 /* Accepted */ THEN
		scoreboard_problem_entry.""SolvedAt"" := greatest(attempt.""CreatedAt"", scoreboard_problem_entry.""SolvedAt"");
	END IF;
	
	IF NOT re_evaluation THEN
		scoreboard_problem_entry.""AttemptsCount"" := scoreboard_problem_entry.""AttemptsCount"" + 1;
	END IF;

    UPDATE ""ScoreboardBase"" sb
    SET ""Problems""[ordinality_to_index_map[problem.""Ordinal""]] = scoreboard_problem_entry
    WHERE
        sb.""ContestId"" = problem.""ContestId"" AND sb.""UserId"" = attempt.""AuthorId"";
END;
$BODY$;

CREATE OR REPLACE PROCEDURE public.refresh_scoreboard(
	IN contest_id uuid)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    attempt ""Attempts"";
    adjustment ""GradeAdjustments"";
	problem_entry ""ScoreboardProblemEntry"";
    i int := 0;
BEGIN
    DELETE FROM ""ScoreboardBase"" WHERE ""ContestId"" = contest_id;

    FOR attempt IN
		SELECT a.*
		FROM ""Attempts"" a
			JOIN ""Problems"" p ON p.""Id"" = a.""ProblemId"" 
		WHERE p.""ContestId"" = contest_id
		ORDER BY a.""CreatedAt""
	LOOP
        CALL update_scoreboard_incrementally(attempt.""Id"");
    END LOOP;

    FOR problem_entry IN
		SELECT p.*
		FROM ""ScoreboardBase"" sb
			LEFT JOIN LATERAL unnest(sb.""Problems"") p ON true
		WHERE p.""IsSolved""
			AND sb.""ContestId"" = contest_id
	LOOP
        CALL update_scoreboard_adjustments(problem_entry.""SolvingAttemptId"");
    END LOOP;
END;
$BODY$;

DO LANGUAGE plpgsql $$
DECLARE
	id uuid;
BEGIN
	FOR id IN SELECT ""Id"" FROM ""Contests"" LOOP
		CALL refresh_scoreboard(id);
	END LOOP;
END;
$$;

");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
