using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace contester.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScoreboardCalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

DROP MATERIALIZED VIEW ""Scoreboard"";

CREATE OR REPLACE FUNCTION scoreboard(contest_id uuid)
RETURNS TABLE (
    ""UserId"" uuid,
    ""FirstName"" ""Users"".""FirstName"" % TYPE,
    ""LastName"" ""Users"".""LastName"" % TYPE,
    ""Patronymic"" ""Users"".""Patronymic"" % TYPE,
    ""AdditionalInfo"" ""Users"".""AdditionalInfo"" % TYPE,
    ""Fee"" int,
    ""FinalGrade"" bigint,
    ""Problems"" jsonb,
    latest_solution_time timestamptz
) LANGUAGE plpgsql
AS $$
DECLARE
    attempts CURSOR FOR
        SELECT a.""Id"", p.""Name"", p.""MaxGrade"", a.""AuthorId"", p.""Ordinal"", p.""Id"" AS ""ProblemId"", ""Status"", a.""CreatedAt""
        FROM ""Attempts"" a
            JOIN ""Problems"" p ON a.""ProblemId"" = p.""Id""
        WHERE p.""ContestId"" = contest_id
        ORDER BY a.""CreatedAt"";

    commission_member_count int := (
        SELECT count(*)
        FROM public.""ContestCommissionMembers""
        WHERE ""ContestId"" = contest_id
    );

    ordinality_to_index_map int[] := (
        WITH src AS (
            SELECT ""Ordinal"" AS value,
                row_number() OVER (ORDER BY ""Ordinal"") AS idx
            FROM ""Problems""
            WHERE ""ContestId"" = contest_id
        )
        SELECT array_agg(idx ORDER BY value)
        FROM src
    );

    default_problems public.""ScoreboardProblemEntry""[] := (
        SELECT array_agg(row(p.""Id"", p.""Name"", 0, false, p.""MaxGrade"", 0, NULL, NULL)
            ::""ScoreboardProblemEntry"" ORDER BY ""Ordinal"")
        FROM ""Problems"" p
        WHERE ""ContestId"" = contest_id
    );

    grade_adjustment record;
BEGIN
    DROP TABLE IF EXISTS result;
    CREATE TEMP TABLE result (
        ""UserId"" uuid,
        ""FirstName"" varchar(50) NOT NULL,
        ""LastName"" varchar(50) NOT NULL,
        ""Patronymic"" varchar(50),
        ""AdditionalInfo"" varchar(150) NOT NULL,
        ""Fee"" int,
        ""FinalGrade"" bigint,
        ""Problems"" ""ScoreboardProblemEntry""[],
        solved_problems_count int,
        latest_solution_time timestamptz
    );

    FOR attempt IN attempts LOOP
        IF (SELECT count(*) FROM result WHERE result.""UserId"" = attempt.""AuthorId"") = 0 THEN
            INSERT INTO result(""UserId"", ""FirstName"", ""LastName"", ""Patronymic"", ""AdditionalInfo"", ""Fee"", ""FinalGrade"",
                    ""Problems"", solved_problems_count, latest_solution_time)
                SELECT u.""Id"", u.""FirstName"", u.""LastName"", u.""Patronymic"", u.""AdditionalInfo"", 0, 0,
                       default_problems, 0, timestamptz '1970-01-01 00:00:00+00'
                FROM ""Users"" u
                WHERE u.""Id"" = attempt.""AuthorId"";
        END IF;

        UPDATE result result_new
        SET ""Problems""[ordinality_to_index_map[attempt.""Ordinal""]] = ROW(
                attempt.""ProblemId"",
                attempt.""Name"",
                result_old.""Problems""[ordinality_to_index_map[attempt.""Ordinal""]].""AttemptsCount"" + 1,
                CASE attempt.""Status""
                    WHEN 5 THEN true
                    ELSE result_old.""Problems""[ordinality_to_index_map[attempt.""Ordinal""]].""IsSolved""
                END,
                attempt.""MaxGrade"",
                CASE attempt.""Status""
                    WHEN 5 THEN attempt.""MaxGrade"" * commission_member_count
                    ELSE result_old.""Problems""[ordinality_to_index_map[attempt.""Ordinal""]].""Grade""
                END,
                CASE attempt.""Status""
                    WHEN 5 THEN attempt.""CreatedAt""
                    ELSE result_old.""Problems""[ordinality_to_index_map[attempt.""Ordinal""]].""SolvedAt""
                END,
                CASE attempt.""Status""
                    WHEN 5 THEN attempt.""Id""
                    ELSE result_old.""Problems""[ordinality_to_index_map[attempt.""Ordinal""]].""SolvingAttemptId""
                END
            )::""ScoreboardProblemEntry"",
            solved_problems_count = CASE
                WHEN NOT result_old.""Problems""[ordinality_to_index_map[attempt.""Ordinal""]].""IsSolved"" AND attempt.""Status"" = 5
                    THEN result_old.solved_problems_count + 1
                ELSE result_old.solved_problems_count
            END,
            latest_solution_time = CASE attempt.""Status""
                WHEN 5 THEN attempt.""CreatedAt""
                ELSE result_old.latest_solution_time
            END
        FROM result result_old
        WHERE result_new.""UserId"" = result_old.""UserId"" AND result_new.""UserId"" = attempt.""AuthorId"";
    END LOOP;

    FOR grade_adjustment IN
        SELECT p.""Ordinal"", a.""AuthorId"" AS ""UserId"", a.""Id"" AS ""AttemptId"", p.""MaxGrade"", ga.""Grade"" AS ""AdjustmentGrade""
        FROM ""GradeAdjustments"" ga
            JOIN ""Attempts"" a ON a.""Id"" = ga.""AttemptId""
            JOIN ""Problems"" p ON p.""Id"" = a.""ProblemId""
        WHERE p.""ContestId"" = contest_id
    LOOP
        UPDATE result result_old
        SET ""Problems""[ordinality_to_index_map[grade_adjustment.""Ordinal""]].""Grade""
            = result_old.""Problems""[ordinality_to_index_map[grade_adjustment.""Ordinal""]].""Grade"" - grade_adjustment.""MaxGrade"" + grade_adjustment.""AdjustmentGrade""
        WHERE result_old.""UserId"" = grade_adjustment.""UserId""
            AND result_old.""Problems""[ordinality_to_index_map[grade_adjustment.""Ordinal""]].""SolvingAttemptId"" = grade_adjustment.""AttemptId"";
    END LOOP;

    FOR i IN 1..coalesce((SELECT array_length(r.""Problems"", 1) FROM result r LIMIT 1), 0) LOOP
        UPDATE result result_old
        SET ""Problems""[i].""Grade"" = result_old.""Problems""[i].""Grade"" / commission_member_count;
    END LOOP;

    MERGE INTO result r
    USING (
        SELECT u.*
        FROM ""ContestParticipants"" cp
            JOIN ""Users"" u ON cp.""ParticipantsId"" = u.""Id""
        WHERE ""ContestsUserParticipatesInId"" = contest_id
    ) u ON u.""Id"" = r.""UserId""
    WHEN MATCHED THEN DO NOTHING
    WHEN NOT MATCHED THEN
        INSERT (""UserId"", ""FirstName"", ""LastName"", ""Patronymic"", ""AdditionalInfo"", ""Fee"", ""FinalGrade"", ""Problems"")
        VALUES (u.""Id"", u.""FirstName"", u.""LastName"", u.""Patronymic"", u.""AdditionalInfo"", 0, 0, default_problems);

    RETURN QUERY
    SELECT r.""UserId"", r.""FirstName"", r.""LastName"", r.""Patronymic"", r.""AdditionalInfo"", r.""Fee"",
        (SELECT sum(p.""Grade"") FROM unnest(r.""Problems"") p) grade, to_jsonb(r.""Problems"") AS ""Problems"",
        r.latest_solution_time
    FROM result r
    ORDER BY grade DESC, r.latest_solution_time ASC;
END;
$$;

CREATE TABLE ""ScoreboardBase""
(
    ""ContestId"" uuid NOT NULL,
    ""UserId"" uuid NOT NULL,
    ""FirstName"" varchar,
    ""LastName"" varchar,
    ""Patronymic"" varchar,
    ""AdditionalInfo"" varchar,
    ""Fee"" integer,
    ""FinalGrade"" bigint,
    ""Problems"" jsonb,
    latest_solution_time timestamp with time zone,
    CONSTRAINT ""Scoreboard_pk""
        PRIMARY KEY (""ContestId"", ""UserId"")
);

CREATE VIEW ""Scoreboard"" AS
SELECT
    ""ContestId"",
    ""UserId"",
    ""FirstName"",
    ""LastName"",
    ""Patronymic"",
    ""AdditionalInfo"",
    ""Fee"",
    ""FinalGrade"",
    ""Problems""
FROM
    ""ScoreboardBase"" sb
ORDER BY ""FinalGrade"" DESC, latest_solution_time;

CREATE PROCEDURE refresh_scoreboard(contest_id uuid)
LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO ""ScoreboardBase""
    SELECT c.""Id"" AS ""ContestId"", a.*
    FROM ""Contests"" c
        JOIN LATERAL scoreboard(c.""Id"") a ON true
    WHERE
        c.""Id"" = contest_id
    ON CONFLICT (""ContestId"", ""UserId"")
        DO UPDATE SET
            ""FirstName"" = excluded.""FirstName"",
            ""LastName"" = excluded.""LastName"",
            ""Patronymic"" = excluded.""Patronymic"",
            ""AdditionalInfo"" = excluded.""AdditionalInfo"",
            ""Fee"" = excluded.""Fee"",
            ""FinalGrade"" = excluded.""FinalGrade"",
            ""Problems"" = excluded.""Problems"",
            latest_solution_time = excluded.latest_solution_time;
END;
$$;

INSERT INTO ""ScoreboardBase""
SELECT c.""Id"" AS ""ContestId"", a.*
FROM ""Contests"" c
    JOIN LATERAL scoreboard(c.""Id"") a ON true
ON CONFLICT (""ContestId"", ""UserId"")
    DO UPDATE SET
        ""FirstName"" = excluded.""FirstName"",
        ""LastName"" = excluded.""LastName"",
        ""Patronymic"" = excluded.""Patronymic"",
        ""AdditionalInfo"" = excluded.""AdditionalInfo"",
        ""Fee"" = excluded.""Fee"",
        ""FinalGrade"" = excluded.""FinalGrade"",
        ""Problems"" = excluded.""Problems"",
        latest_solution_time = excluded.latest_solution_time;

");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

DROP FUNCTION scoreboard(uuid);
DROP PROCEDURE refresh_scoreboard(uuid);

DROP VIEW ""Scoreboard"";

DROP TABLE ""ScoreboardBase"";

CREATE MATERIALIZED VIEW ""Scoreboard"" AS
WITH row_data AS (
	SELECT
		attempt_data.""AuthorId"",
		attempt_data.""Ordinal"" AS ""ProblemOrdinal"",
		attempt_data.""ContestId"",
		ROW(
			attempt_data.""ProblemId"",
			attempt_data.""Name"",
			attempt_data.""AttemptsCount"",
			attempt_data.""IsSolved"",
			attempt_data.""MaxGrade"",
			COALESCE((
				(solved_data.""CommissionMembersCount"" - solved_data.""GradeAdjustmentsCount"") * attempt_data.""MaxGrade"" + solved_data.""GradeAdjustmentsSum""
			) / solved_data.""CommissionMembersCount"", CASE WHEN ""IsSolved"" THEN attempt_data.""MaxGrade"" ELSE 0 END)::int,
			solved_data.""CreatedAt"",
		    solved_data.""Id"" 
		)::""ScoreboardProblemEntry"" AS problem_cell
	FROM (
			SELECT
				p.""ContestId"",
				COALESCE(cp.""ParticipantsId"", a.""AuthorId"") AS ""AuthorId"",
				p.""Id"" AS ""ProblemId"",
				p.""Name"",
				CASE WHEN a.""AuthorId"" IS NOT NULL THEN COUNT(1) ELSE 0 END AS ""AttemptsCount"",
				bool_or(CASE a.""Status"" WHEN 5 THEN true ELSE false END) AS ""IsSolved"",
				p.""MaxGrade"",
				p.""Ordinal""
			FROM
				""Attempts"" a
				FULL OUTER JOIN ""Problems"" p ON a.""ProblemId"" = p.""Id""
				LEFT JOIN ""ContestParticipants"" cp ON cp.""ContestsUserParticipatesInId"" = p.""ContestId""
			GROUP BY a.""AuthorId"", cp.""ParticipantsId"", p.""Id"", p.""Name"", p.""MaxGrade"", p.""Ordinal"", p.""ContestId""
		) attempt_data
		LEFT JOIN LATERAL (
			SELECT
				(SELECT COUNT(1) FROM ""ContestCommissionMembers"" cm WHERE cm.""ContestId"" = p2.""ContestId"") AS ""CommissionMembersCount"",
				SUM(ga.""Grade"") ""GradeAdjustmentsSum"",
				COUNT(ga.""Grade"") ""GradeAdjustmentsCount"",
				a2.""Id"",
				a2.""CreatedAt""
			FROM
				""Problems"" p2
				LEFT JOIN ""Attempts"" a2 ON p2.""Id"" = a2.""ProblemId""
				LEFT JOIN ""GradeAdjustments"" ga ON ga.""AttemptId"" = a2.""Id""
			WHERE
				a2.""Status"" = 5
				AND a2.""AuthorId"" = attempt_data.""AuthorId""
				AND a2.""ProblemId"" = attempt_data.""ProblemId""
			GROUP BY
				a2.""Id"", a2.""CreatedAt"", p2.""ContestId""
			ORDER BY
				a2.""CreatedAt"" DESC
			LIMIT 1
		) solved_data ON true
)
SELECT
	rd.""ContestId"",
	u.""Id"" AS ""UserId"",
	u.""FirstName"",
	u.""LastName"",
	u.""Patronymic"",
	u.""AdditionalInfo"",
	0 AS ""Fee"",
	SUM((rd).problem_cell.""Grade"") AS ""FinalGrade"",
	jsonb_agg(to_jsonb((rd).problem_cell) ORDER BY ""ProblemOrdinal"") AS ""Problems""
FROM row_data rd
	JOIN ""Users"" u ON u.""Id"" = rd.""AuthorId""
GROUP BY rd.""ContestId"", rd.""AuthorId"", u.""Id"", u.""FirstName"", u.""LastName"", u.""Patronymic"", u.""AdditionalInfo""
ORDER BY SUM((rd).problem_cell.""Grade"") DESC, MAX((rd).problem_cell.""SolvedAt"");

CREATE INDEX ""Scoreboard_ContestId"" ON ""Scoreboard""(""ContestId"");

");
        }
    }
}
