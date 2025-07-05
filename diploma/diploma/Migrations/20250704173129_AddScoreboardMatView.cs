using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace diploma.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreboardMatView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
migrationBuilder.Sql(@"

CREATE TYPE ""ScoreboardProblemEntry"" AS (
	""ProblemId"" uuid,
	""Name"" varchar(255),
	""AttemptsCount"" bigint,
	""IsSolved"" boolean,
	""MaxGrade"" int,
	""Grade"" int,
	""SolvedAt"" timestamptz,
	""SolvingAttemptId"" uuid
);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql(@"
			DROP MATERIALIZED VIEW ""Scoreboard"";
			DROP TYPE ""ScoreboardProblemEntry"";
			");
        }
    }
}
