using contester.Features.Common.Exceptions;
using contester.Features.SchemaDescriptions.Commands;
using contester.Features.SchemaDescriptions.Queries;
using Sieve.Models;

namespace tests.integration;

[TestFixture]
public class SchemaDescriptionsIntegrationTests
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
    public async Task CreateSchemaDescription_AsAdmin_ReturnsDto()
    {
        var result = await _fixture.SendAsync(new CreateSchemaDescriptionCommand
        {
            ContestId = _fixture.ContestId,
            Name = "New schema",
        }, _fixture.AdminUserId);

        Assert.That(result.Name, Is.EqualTo("New schema"));
        Assert.That(result.ContestId, Is.EqualTo(_fixture.ContestId));
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void CreateSchemaDescription_UnknownContest_ThrowsEntityNotFoundException()
    {
        Assert.ThrowsAsync<EntityNotFoundException>(async () =>
            await _fixture.SendAsync(new CreateSchemaDescriptionCommand
            {
                ContestId = Guid.NewGuid(),
                Name = "orphan schema",
            }, _fixture.AdminUserId));
    }

    [Test]
    public void CreateSchemaDescription_AsContestant_ThrowsNotifyUserException()
    {
        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new CreateSchemaDescriptionCommand
            {
                ContestId = _fixture.ContestId,
                Name = "denied",
            }, _fixture.ContestantUserId));
    }

    [Test]
    public async Task CreateSchemaDescriptionFile_ValidSql_HasNoProblems()
    {
        var schema = await _fixture.SendAsync(new CreateSchemaDescriptionCommand
        {
            ContestId = _fixture.ContestId,
            Name = "Valid file schema",
        }, _fixture.AdminUserId);

        var file = await _fixture.SendAsync(new CreateSchemaDescriptionFileCommand
        {
            SchemaDescriptionId = schema.Id,
            Dbms = "Postgres",
            Description = _fixture.ReadAsset("Schema.sql"),
        }, _fixture.AdminUserId);

        Assert.That(file.Dbms, Is.EqualTo("Postgres"));
        Assert.That(file.HasProblems, Is.False);
        Assert.That(file.Description.Trim(), Is.EqualTo(_fixture.ReadAsset("Schema.sql").Trim()));
    }

    [Test]
    public async Task CreateSchemaDescriptionFile_InvalidSql_HasProblems()
    {
        var schema = await _fixture.SendAsync(new CreateSchemaDescriptionCommand
        {
            ContestId = _fixture.ContestId,
            Name = "Broken file schema",
        }, _fixture.AdminUserId);

        var file = await _fixture.SendAsync(new CreateSchemaDescriptionFileCommand
        {
            SchemaDescriptionId = schema.Id,
            Dbms = "Postgres",
            Description = "CREATE TABLE broken (;;;",
        }, _fixture.AdminUserId);

        Assert.That(file.HasProblems, Is.True);
        Assert.That(file.Problems, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task CreateSchemaDescriptionFile_MissingDescriptionAndSource_ThrowsNotifyUserException()
    {
        var schema = await _fixture.SendAsync(new CreateSchemaDescriptionCommand
        {
            ContestId = _fixture.ContestId,
            Name = "Empty file schema",
        }, _fixture.AdminUserId);

        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new CreateSchemaDescriptionFileCommand
            {
                SchemaDescriptionId = schema.Id,
                Dbms = "Postgres",
            }, _fixture.AdminUserId));
    }

    [Test]
    public async Task UpdateSchemaDescription_AsAdmin_Renames()
    {
        var schema = await _fixture.SendAsync(new CreateSchemaDescriptionCommand
        {
            ContestId = _fixture.ContestId,
            Name = "Before rename",
        }, _fixture.AdminUserId);

        var updated = await _fixture.SendAsync(new UpdateSchemaDescriptionCommand
        {
            Id = schema.Id,
            Name = "After rename",
        }, _fixture.AdminUserId);

        Assert.That(updated.Name, Is.EqualTo("After rename"));
    }

    [Test]
    public async Task UpdateSchemaDescriptionFile_ValidSql_ClearsProblems()
    {
        var schema = await _fixture.SendAsync(new CreateSchemaDescriptionCommand
        {
            ContestId = _fixture.ContestId,
            Name = "Update file schema",
        }, _fixture.AdminUserId);

        await _fixture.SendAsync(new CreateSchemaDescriptionFileCommand
        {
            SchemaDescriptionId = schema.Id,
            Dbms = "Postgres",
            Description = "CREATE TABLE broken (;;;",
        }, _fixture.AdminUserId);

        var updated = await _fixture.SendAsync(new UpdateSchemaDescriptionFileCommand
        {
            SchemaDescriptionId = schema.Id,
            Dbms = "Postgres",
            Description = _fixture.ReadAsset("Schema.sql"),
        }, _fixture.AdminUserId);

        Assert.That(updated.HasProblems, Is.False);
    }

    [Test]
    public async Task GetSchemaDescriptions_AsAdmin_ReturnsSeeded()
    {
        var result = await _fixture.SendAsync(new GetSchemaDescriptionsQuery
        {
            SieveModel = new SieveModel
            {
                Filters = $"ContestId=={_fixture.ContestId}",
            },
        }, _fixture.AdminUserId);

        Assert.That(result.SchemaDescriptions.Any(s => s.Id == _fixture.SchemaDescriptionId), Is.True);
    }

    [Test]
    public async Task GetSchemaDescriptionFiles_AsAdmin_ReturnsSeededFile()
    {
        var result = await _fixture.SendAsync(new GetSchemaDescriptionFilesQuery
        {
            SchemaDescriptionId = _fixture.SchemaDescriptionId,
        }, _fixture.AdminUserId);

        Assert.That(result.SchemaDescriptionFiles.Count, Is.EqualTo(1));
        Assert.That(result.SchemaDescriptionFiles[0].Dbms, Is.EqualTo("Postgres"));
        Assert.That(result.SchemaDescriptionFiles[0].Description, Is.Not.Empty);
    }

    [Test]
    public async Task DeleteSchemaDescriptionFile_AsAdmin_RemovesFile()
    {
        var schema = await _fixture.SendAsync(new CreateSchemaDescriptionCommand
        {
            ContestId = _fixture.ContestId,
            Name = "Delete file schema",
        }, _fixture.AdminUserId);

        await _fixture.SendAsync(new CreateSchemaDescriptionFileCommand
        {
            SchemaDescriptionId = schema.Id,
            Dbms = "Postgres",
            Description = _fixture.ReadAsset("Schema.sql"),
        }, _fixture.AdminUserId);

        await _fixture.SendAsync(new DeleteSchemaDescriptionFileCommand
        {
            SchemaDescriptionId = schema.Id,
            Dbms = "Postgres",
        }, _fixture.AdminUserId);

        var files = await _fixture.SendAsync(new GetSchemaDescriptionFilesQuery
        {
            SchemaDescriptionId = schema.Id,
        }, _fixture.AdminUserId);

        Assert.That(files.SchemaDescriptionFiles, Is.Empty);
    }

    [Test]
    public async Task DeleteSchemaDescription_AsAdmin_RemovesSchema()
    {
        var schema = await _fixture.SendAsync(new CreateSchemaDescriptionCommand
        {
            ContestId = _fixture.ContestId,
            Name = "Delete me schema",
        }, _fixture.AdminUserId);

        await _fixture.SendAsync(new DeleteSchemaDescriptionCommand
        {
            Id = schema.Id,
        }, _fixture.AdminUserId);

        var remaining = await _fixture.SendAsync(new GetSchemaDescriptionsQuery
        {
            SieveModel = new SieveModel(),
        }, _fixture.AdminUserId);

        Assert.That(remaining.SchemaDescriptions.Any(s => s.Id == schema.Id), Is.False);
    }

    [Test]
    public void GetSchemaDescriptions_AsContestant_ThrowsNotifyUserException()
    {
        Assert.ThrowsAsync<NotifyUserException>(async () =>
            await _fixture.SendAsync(new GetSchemaDescriptionsQuery
            {
                SieveModel = new SieveModel(),
            }, _fixture.ContestantUserId));
    }
}
