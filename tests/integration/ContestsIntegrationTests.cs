using contester.Features.Common.Exceptions;
using contester.Features.Contests.Commands;
using contester.Features.Contests.Queries;

namespace tests.integration;

[TestFixture]
public class ContestsIntegrationTests
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
    public async Task CreateContest_AsAdmin_ReturnsContest()
    {
        var result = await _fixture.SendAsync(new CreateContestCommand
        {
            Name = "New Contest",
            Description = "Contest description body",
            StartDate = DateTime.UtcNow.AddHours(-1),
            EndDate = DateTime.UtcNow.AddDays(2),
            IsPublic = true,
            Participants = [],
        }, _fixture.AdminUserId);

        Assert.That(result.Name, Is.EqualTo("New Contest"));
        Assert.That(result.Description, Is.EqualTo("Contest description body"));
        Assert.That(result.AuthorId, Is.EqualTo(_fixture.AdminUserId));
        Assert.That(result.IsPublic, Is.True);
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void CreateContest_AsContestant_ThrowsNotifyUserException()
    {
        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new CreateContestCommand
            {
                Name = "Denied Contest",
                Description = "nope",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
                IsPublic = false,
                Participants = [],
            }, _fixture.ContestantUserId));
    }

    [Test]
    public async Task UpdateContest_AsAdmin_UpdatesFields()
    {
        var created = await _fixture.SendAsync(new CreateContestCommand
        {
            Name = "To update",
            Description = "Original description",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            IsPublic = false,
            Participants = [],
        }, _fixture.AdminUserId);

        var result = await _fixture.SendAsync(new UpdateContestCommand
        {
            ContestId = created.Id,
            Name = "Renamed Contest",
            Description = "Updated description",
            StartDate = DateTime.UtcNow.AddDays(-2),
            FinishDate = DateTime.UtcNow.AddDays(3),
            IsPublic = true,
            CommissionMembers = [_fixture.AdminUserId, _fixture.ContestantUserId],
        }, _fixture.AdminUserId);

        Assert.That(result.Name, Is.EqualTo("Renamed Contest"));
        Assert.That(result.Description, Is.EqualTo("Updated description"));
        Assert.That(result.IsPublic, Is.True);
    }

    [Test]
    public void UpdateContest_UnknownContest_ThrowsEntityNotFoundException()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(async () =>
            await _fixture.SendAsync(new UpdateContestCommand
            {
                ContestId = Guid.NewGuid(),
                Name = "Missing",
                Description = "Missing",
                StartDate = DateTime.UtcNow,
                FinishDate = DateTime.UtcNow.AddDays(1),
                IsPublic = true,
                CommissionMembers = [_fixture.AdminUserId],
            }, _fixture.AdminUserId));
    }

    [Test]
    public async Task GetContests_ReturnsSeededContest()
    {
        var result = await _fixture.SendAsync(new GetContestsQuery
        {
            UserId = null,
        }, _fixture.ContestantUserId);

        var contest = result.Contests.Single(c => c.Id == _fixture.ContestId);
        Assert.That(contest.Name, Is.EqualTo("Integration Contest"));
        Assert.That(contest.Description, Is.Not.Empty);
        Assert.That(contest.UserParticipates, Is.False);
    }

    [Test]
    public async Task GetContests_WithUserId_MarksParticipationAcrossMultipleContests()
    {
        var extra = await _fixture.SendAsync(new CreateContestCommand
        {
            Name = "Extra contest for participation",
            Description = "desc",
            StartDate = DateTime.UtcNow.AddHours(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            IsPublic = true,
            Participants = [],
        }, _fixture.AdminUserId);

        var result = await _fixture.SendAsync(new GetContestsQuery
        {
            UserId = _fixture.ContestantUserId,
        }, _fixture.ContestantUserId);

        Assert.That(result.Contests.Count, Is.GreaterThanOrEqualTo(2));

        var seeded = result.Contests.Single(c => c.Id == _fixture.ContestId);
        Assert.That(seeded.UserParticipates, Is.True);

        var notJoined = result.Contests.Single(c => c.Id == extra.Id);
        Assert.That(notJoined.UserParticipates, Is.False);
    }

    [Test]
    public async Task GetContestSettings_ReturnsCommissionMembers()
    {
        var result = await _fixture.SendAsync(new GetContestSettingsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.AdminUserId);

        Assert.That(result.Id, Is.EqualTo(_fixture.ContestId));
        Assert.That(result.CommissionMembers.Any(m => m.Id == _fixture.AdminUserId), Is.True);
        Assert.That(result.Description, Is.Not.Empty);
    }

    [Test]
    public async Task AddAndRemoveContestParticipantUser_AsAdmin()
    {
        var added = await _fixture.SendAsync(new AddContestParticipantUserCommand
        {
            ContestId = _fixture.ContestId,
            ParticipantId = _fixture.OtherUserId,
        }, _fixture.AdminUserId);

        Assert.That(added.Id, Is.EqualTo(_fixture.ContestId));

        var participants = await _fixture.SendAsync(new GetContestParticipantsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.AdminUserId);

        Assert.That(participants.ContestParticipants.Any(p => p.Id == _fixture.OtherUserId), Is.True);

        await _fixture.SendAsync(new RemoveContestParticipantUserCommand
        {
            ContestId = _fixture.ContestId,
            ParticipantId = _fixture.OtherUserId,
        }, _fixture.AdminUserId);

        participants = await _fixture.SendAsync(new GetContestParticipantsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.AdminUserId);

        Assert.That(participants.ContestParticipants.Any(p => p.Id == _fixture.OtherUserId), Is.False);
    }

    [Test]
    public async Task AddAndRemoveContestParticipantGroup_AsAdmin()
    {
        await _fixture.SendAsync(new AddContestParticipantGroupCommand
        {
            ContestId = _fixture.ContestId,
            GroupId = _fixture.ExtraGroupId,
        }, _fixture.AdminUserId);

        var participants = await _fixture.SendAsync(new GetContestParticipantsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.AdminUserId);

        Assert.That(participants.ContestParticipants.Any(p => p.Id == _fixture.ExtraGroupId), Is.True);

        await _fixture.SendAsync(new RemoveContestParticipantGroupCommand
        {
            ContestId = _fixture.ContestId,
            GroupId = _fixture.ExtraGroupId,
        }, _fixture.AdminUserId);

        participants = await _fixture.SendAsync(new GetContestParticipantsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.AdminUserId);

        Assert.That(participants.ContestParticipants.Any(p => p.Id == _fixture.ExtraGroupId), Is.False);
    }

    [Test]
    public void AddContestParticipantUser_AsContestant_ThrowsNotifyUserException()
    {
        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new AddContestParticipantUserCommand
            {
                ContestId = _fixture.ContestId,
                ParticipantId = _fixture.OtherUserId,
            }, _fixture.ContestantUserId));
    }

    [Test]
    public async Task GetContestApplications_AsAdmin_ReturnsPending()
    {
        var result = await _fixture.SendAsync(new GetContestApplicationsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.AdminUserId);

        Assert.That(result.Any(p => p.Id == _fixture.OtherUserId), Is.True);
    }

    [Test]
    public void GetContestApplications_AsContestant_ThrowsNotifyUserException()
    {
        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new GetContestApplicationsQuery
            {
                ContestId = _fixture.ContestId,
            }, _fixture.ContestantUserId));
    }

    [Test]
    public async Task GetContestReport_AsAdmin_ReturnsReport()
    {
        var result = await _fixture.SendAsync(new GetContestReportQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.AdminUserId);

        Assert.That(result.ContestId, Is.EqualTo(_fixture.ContestId));
        Assert.That(result.ContestName, Is.Not.Empty);
        Assert.That(result.Participants, Is.Not.Null);
    }

    [Test]
    public void GetContestReport_AsUnrelatedUser_ThrowsNotifyUserException()
    {
        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new GetContestReportQuery
            {
                ContestId = _fixture.ContestId,
            }, _fixture.OtherUserId));
    }

    [Test]
    public async Task GetContestParticipants_IncludesSeededContestant()
    {
        var result = await _fixture.SendAsync(new GetContestParticipantsQuery
        {
            ContestId = _fixture.ContestId,
        }, _fixture.AdminUserId);

        Assert.That(result.ContestParticipants.Any(p => p.Id == _fixture.ContestantUserId), Is.True);
    }
}
