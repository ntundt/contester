using contester.Features.Common.Exceptions;
using contester.Features.Contests.Commands;
using contester.Features.Problems.Commands;
using contester.Features.Problems.Exceptions;
using contester.Features.Problems.Queries;

namespace tests.integration;

[TestFixture]
public class ProblemsIntegrationTests
{
    private ContesterIntegrationFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = new ContesterIntegrationFixture();
        await _fixture.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _fixture.DisposeAsync();
    }

    [Test]
    public async Task CreateProblem_AsAdmin_ReturnsProblem()
    {
        var result = await _fixture.SendAsync(new CreateProblemCommand
        {
            Name = "Second problem",
            Statement = "Find banknotes",
            OrderMatters = false,
            FloatMaxDelta = 0.001m,
            CaseSensitive = false,
            TimeLimit = TimeSpan.FromSeconds(3),
            MaxGrade = 5,
            ContestId = _fixture.ContestId,
            SchemaDescriptionId = _fixture.SchemaDescriptionId,
            Solution = "SELECT amount FROM banknotes ORDER BY amount;",
            SolutionDbms = "Postgres",
        }, _fixture.AdminUserId);

        Assert.That(result.Name, Is.EqualTo("Second problem"));
        Assert.That(result.Statement, Is.EqualTo("Find banknotes"));
        Assert.That(result.MaxGrade, Is.EqualTo(5));
        Assert.That(result.Ordinal, Is.EqualTo(2));
        Assert.That(result.SchemaDescriptionId, Is.EqualTo(_fixture.SchemaDescriptionId));

        var listed = await _fixture.SendAsync(new GetProblemsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.AdminUserId);
        var created = listed.Problems.Single(p => p.Id == result.Id);
        Assert.That(created.AvailableDbms, Does.Contain("Postgres"));
    }

    [Test]
    public async Task CreateProblem_ContestWithoutSchemas_ThrowsNotifyUserException()
    {
        var contest = await _fixture.SendAsync(new CreateContestCommand
        {
            Name = "Empty schema contest",
            Description = "no schemas",
            StartDate = DateTime.UtcNow.AddHours(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            IsPublic = true,
            Participants = [],
        }, _fixture.AdminUserId);

        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new CreateProblemCommand
            {
                Name = "Impossible",
                Statement = "x",
                OrderMatters = true,
                FloatMaxDelta = 0.01m,
                CaseSensitive = true,
                TimeLimit = TimeSpan.FromSeconds(1),
                MaxGrade = 1,
                ContestId = contest.Id,
                Solution = "SELECT 1;",
                SolutionDbms = "Postgres",
            }, _fixture.AdminUserId));
    }

    [Test]
    public void CreateProblem_AsContestant_ThrowsNotifyUserException()
    {
        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new CreateProblemCommand
            {
                Name = "Denied",
                Statement = "x",
                OrderMatters = true,
                FloatMaxDelta = 0.01m,
                CaseSensitive = true,
                TimeLimit = TimeSpan.FromSeconds(1),
                MaxGrade = 1,
                ContestId = _fixture.ContestId,
                SchemaDescriptionId = _fixture.SchemaDescriptionId,
                Solution = "SELECT 1;",
                SolutionDbms = "Postgres",
            }, _fixture.ContestantUserId));
    }

    [Test]
    public async Task GetProblems_AsContestant_ReturnsProblems()
    {
        var result = await _fixture.SendAsync(new GetProblemsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.ContestantUserId);

        Assert.That(result.Problems.Any(p => p.Id == _fixture.ProblemId), Is.True);
        Assert.That(result.Problems[0].Statement, Is.Not.Empty);
    }

    [Test]
    public void GetProblems_UnknownContest_ThrowsEntityNotFoundException()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(async () =>
            await _fixture.SendAsync(new GetProblemsQuery
            {
                ContestId = Guid.NewGuid(),
            }, _fixture.ContestantUserId));
    }

    [Test]
    public async Task UpdateProblem_ValidSolution_UpdatesFields()
    {
        var created = await _fixture.SendAsync(new CreateProblemCommand
        {
            Name = "Updatable",
            Statement = "old statement",
            OrderMatters = true,
            FloatMaxDelta = 0.01m,
            CaseSensitive = true,
            TimeLimit = TimeSpan.FromSeconds(2),
            MaxGrade = 7,
            ContestId = _fixture.ContestId,
            SchemaDescriptionId = _fixture.SchemaDescriptionId,
            Solution = _fixture.ReadAsset("Solution.sql"),
            SolutionDbms = "Postgres",
        }, _fixture.AdminUserId);

        var updated = await _fixture.SendAsync(new UpdateProblemCommand
        {
            Id = created.Id,
            Name = "Updated problem",
            Statement = "new statement",
            OrderMatters = false,
            FloatMaxDelta = 0.1m,
            CaseSensitive = false,
            TimeLimit = TimeSpan.FromSeconds(4),
            MaxGrade = 9,
            Ordinal = created.Ordinal,
            SchemaDescriptionId = _fixture.SchemaDescriptionId,
            Solution = _fixture.ReadAsset("Solution.sql"),
            SolutionDbms = "Postgres",
        }, _fixture.AdminUserId);

        Assert.That(updated.Name, Is.EqualTo("Updated problem"));
        Assert.That(updated.Statement, Is.EqualTo("new statement"));
        Assert.That(updated.MaxGrade, Is.EqualTo(9));
        Assert.That(updated.OrderMatters, Is.False);
    }

    [Test]
    public async Task UpdateProblem_InvalidSolution_ThrowsProblemSolutionInvalidException()
    {
        var created = await _fixture.SendAsync(new CreateProblemCommand
        {
            Name = "Invalid update target",
            Statement = "statement",
            OrderMatters = true,
            FloatMaxDelta = 0.01m,
            CaseSensitive = true,
            TimeLimit = TimeSpan.FromSeconds(2),
            MaxGrade = 3,
            ContestId = _fixture.ContestId,
            SchemaDescriptionId = _fixture.SchemaDescriptionId,
            Solution = _fixture.ReadAsset("Solution.sql"),
            SolutionDbms = "Postgres",
        }, _fixture.AdminUserId);

        Assert.ThrowsAsync<ProblemSolutionInvalidException>(async () =>
            await _fixture.SendAsync(new UpdateProblemCommand
            {
                Id = created.Id,
                Name = created.Name,
                Statement = "statement",
                OrderMatters = true,
                FloatMaxDelta = 0.01m,
                CaseSensitive = true,
                TimeLimit = TimeSpan.FromSeconds(2),
                MaxGrade = 3,
                Ordinal = created.Ordinal,
                SchemaDescriptionId = _fixture.SchemaDescriptionId,
                Solution = "SELECT [Broken] FROM nowhere UwU",
                SolutionDbms = "Postgres",
            }, _fixture.AdminUserId));
    }

    [Test]
    public async Task GetExpectedSolution_AsAdmin_ReturnsSolution()
    {
        var result = await _fixture.SendAsync(new GetExpectedSolutionQuery
        {
            ProblemId = _fixture.ProblemId,
        }, _fixture.AdminUserId);

        Assert.That(result.ProblemId, Is.EqualTo(_fixture.ProblemId));
        Assert.That(result.Dbms, Is.EqualTo("Postgres"));
        Assert.That(result.Solution.Trim(), Is.EqualTo(_fixture.ReadAsset("Solution.sql").Trim()));
    }

    [Test]
    public void GetExpectedSolution_AsContestant_ThrowsNotifyUserException()
    {
        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new GetExpectedSolutionQuery
            {
                ProblemId = _fixture.ProblemId,
            }, _fixture.ContestantUserId));
    }

    [Test]
    public async Task DeleteProblem_AsAdmin_RemovesProblem()
    {
        var created = await _fixture.SendAsync(new CreateProblemCommand
        {
            Name = "Delete me",
            Statement = "temp",
            OrderMatters = true,
            FloatMaxDelta = 0.01m,
            CaseSensitive = true,
            TimeLimit = TimeSpan.FromSeconds(1),
            MaxGrade = 1,
            ContestId = _fixture.ContestId,
            SchemaDescriptionId = _fixture.SchemaDescriptionId,
            Solution = "SELECT 1 AS n;",
            SolutionDbms = "Postgres",
        }, _fixture.AdminUserId);

        await _fixture.SendAsync(new DeleteProblemCommand
        {
            Id = created.Id,
        }, _fixture.AdminUserId);

        var problems = await _fixture.SendAsync(new GetProblemsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.AdminUserId);

        Assert.That(problems.Problems.Any(p => p.Id == created.Id), Is.False);
    }

    [Test]
    public void DeleteProblem_AsContestant_ThrowsNotifyUserException()
    {
        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new DeleteProblemCommand
            {
                Id = _fixture.ProblemId,
            }, _fixture.ContestantUserId));
    }

    [Test]
    public async Task CreateProblem_UsesDefaultSchemaWhenOmitted()
    {
        var result = await _fixture.SendAsync(new CreateProblemCommand
        {
            Name = "Default schema problem",
            Statement = "uses contest schema",
            OrderMatters = true,
            FloatMaxDelta = 0.01m,
            CaseSensitive = true,
            TimeLimit = TimeSpan.FromSeconds(1),
            MaxGrade = 2,
            ContestId = _fixture.ContestId,
            SchemaDescriptionId = null,
            Solution = "SELECT 1 AS n;",
            SolutionDbms = "Postgres",
        }, _fixture.AdminUserId);

        Assert.That(result.SchemaDescriptionId, Is.EqualTo(_fixture.SchemaDescriptionId));
    }
}
