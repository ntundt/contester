using contester.Features.Attempts;
using contester.Features.Attempts.Commands;
using contester.Features.Attempts.Queries;
using contester.Features.Authentication.Exceptions;
using contester.Features.Common.Exceptions;

namespace tests.integration;

[TestFixture]
public class AttemptsIntegrationTests
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
    public async Task CreateAttempt_CorrectSolution_ReturnsAccepted()
    {
        var result = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolution.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        Assert.That(result.Status, Is.EqualTo(AttemptStatus.Accepted));
        Assert.That(result.ProblemId, Is.EqualTo(_fixture.ProblemId));
        Assert.That(result.AuthorId, Is.EqualTo(_fixture.ContestantUserId));
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public async Task CreateAttempt_WrongAnswer_ReturnsWrongAnswer()
    {
        var result = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolutionWrongAnswer.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        Assert.That(result.Status, Is.EqualTo(AttemptStatus.WrongAnswer));
    }

    [Test]
    public async Task CreateAttempt_WrongOutputFormat_ReturnsWrongOutputFormat()
    {
        var result = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolutionWrongFormat.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        Assert.That(result.Status, Is.EqualTo(AttemptStatus.WrongOutputFormat));
    }

    [Test]
    public async Task CreateAttempt_SyntaxError_ReturnsError()
    {
        var result = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolutionSyntaxError.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        Assert.That(result.Status, Is.EqualTo(AttemptStatus.Error));
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task CreateAttempt_SolutionWithNulls_DoesNotThrowAndReturnsWrongAnswer()
    {
        var result = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolutionWithNulls.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        Assert.That(result.Status, Is.EqualTo(AttemptStatus.WrongAnswer));
    }

    [Test]
    public void CreateAttempt_EmptySolution_ThrowsNotifyUserException()
    {
        var exception = Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new CreateAttemptCommand
            {
                ProblemId = _fixture.ProblemId,
                Solution = "   ",
                Dbms = "Postgres",
            }, _fixture.ContestantUserId));

        Assert.That(exception!.Message, Does.Contain("Empty solutions"));
    }

    [Test]
    public void CreateAttempt_UnknownProblem_ThrowsEntityNotFoundException()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(async () =>
            await _fixture.SendAsync(new CreateAttemptCommand
            {
                ProblemId = Guid.NewGuid(),
                Solution = "SELECT 1;",
                Dbms = "Postgres",
            }, _fixture.ContestantUserId));
    }

    [Test]
    public async Task GetAttempts_FiltersByContestId()
    {
        var created = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolution.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        var result = await _fixture.SendAsync(new GetAttemptsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.ContestantUserId);

        Assert.That(result.Attempts.Any(a => a.Id == created.Id), Is.True);
        Assert.That(result.TotalCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetSingleAttempt_AsAuthor_ReturnsSolution()
    {
        var created = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolution.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        var result = await _fixture.SendAsync(new GetSingleAttemptQuery
        {
            AttemptId = created.Id,
        }, _fixture.ContestantUserId);

        Assert.That(result.Id, Is.EqualTo(created.Id));
        Assert.That(result.Solution.Trim(), Is.EqualTo(_fixture.ReadAsset("ContestantSolution.sql").Trim()));
        Assert.That(result.Dbms, Is.EqualTo("Postgres"));
        Assert.That(result.ProblemName, Is.EqualTo("High salary employees"));
        Assert.That(result.AuthorFirstName, Is.EqualTo("Contestant"));
    }

    [Test]
    public async Task GetSingleAttempt_AsAdmin_ReturnsSolution()
    {
        var created = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolutionWrongAnswer.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        var result = await _fixture.SendAsync(new GetSingleAttemptQuery
        {
            AttemptId = created.Id,
        }, _fixture.AdminUserId);

        Assert.That(result.Id, Is.EqualTo(created.Id));
        Assert.That(result.Status, Is.EqualTo(AttemptStatus.WrongAnswer));
    }

    [Test]
    public async Task GetSingleAttempt_AsUnrelatedUser_ThrowsPermissionException()
    {
        var created = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolution.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        Assert.ThrowsAsync<UserDoesNotHavePermissionException>(async () =>
            await _fixture.SendAsync(new GetSingleAttemptQuery
            {
                AttemptId = created.Id,
            }, _fixture.OtherUserId));
    }

    [Test]
    public async Task ReEvaluateAttempt_AsAdmin_RecomputesStatus()
    {
        var created = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolution.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        Assert.That(created.Status, Is.EqualTo(AttemptStatus.Accepted));

        var reEvaluated = await _fixture.SendAsync(new ReEvaluateAttemptCommand
        {
            AttemptId = created.Id,
        }, _fixture.AdminUserId);

        Assert.That(reEvaluated.Status, Is.EqualTo(AttemptStatus.Accepted));
        Assert.That(reEvaluated.Id, Is.EqualTo(created.Id));
    }

    [Test]
    public async Task ReEvaluateAttempt_AsContestant_ThrowsNotifyUserException()
    {
        var created = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolution.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new ReEvaluateAttemptCommand
            {
                AttemptId = created.Id,
            }, _fixture.ContestantUserId));
    }

    [Test]
    public async Task EvaluateResultSets_AsAdmin_ReturnsMatchingResultSets()
    {
        var created = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolution.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        var result = await _fixture.SendAsync(new EvaluateResultSetsQuery
        {
            AttemptId = created.Id,
        }, _fixture.AdminUserId);

        Assert.That(result.DeclineReason, Is.Empty);
        Assert.That(result.ExpectedResult.Columns, Is.Not.Empty);
        Assert.That(result.ActualResult.Columns.Select(c => c.Name),
            Is.EqualTo(result.ExpectedResult.Columns.Select(c => c.Name)));
        Assert.That(result.ActualResult.Rows.Count, Is.EqualTo(result.ExpectedResult.Rows.Count));
    }

    [Test]
    public async Task EvaluateResultSets_WrongAnswer_ReturnsDifferentRows()
    {
        var created = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolutionWrongAnswer.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        var result = await _fixture.SendAsync(new EvaluateResultSetsQuery
        {
            AttemptId = created.Id,
        }, _fixture.AdminUserId);

        Assert.That(result.DeclineReason, Is.Empty);
        Assert.That(result.ActualResult.Rows.Count, Is.Not.EqualTo(result.ExpectedResult.Rows.Count));
    }

    [Test]
    public async Task EvaluateResultSets_AsContestant_ThrowsNotifyUserException()
    {
        var created = await _fixture.SendAsync(new CreateAttemptCommand
        {
            ProblemId = _fixture.ProblemId,
            Solution = _fixture.ReadAsset("ContestantSolution.sql"),
            Dbms = "Postgres",
        }, _fixture.ContestantUserId);

        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new EvaluateResultSetsQuery
            {
                AttemptId = created.Id,
            }, _fixture.ContestantUserId));
    }
}
