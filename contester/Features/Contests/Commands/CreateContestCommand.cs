using System.Text.Json.Serialization;
using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.UserGroups.Services;
using contester.Features.Users;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Commands;

public class CreateContestCommand : IRequest<ContestDto>, IAuthorizedRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublic { get; set; }
    public List<Guid> Participants { get; set; } = null!;
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageContests;
}

public class CreateContestCommandHandler(
    ApplicationDbContext context,
    IMapper mapper,
    IDirectoryService directoryService,
    IUserGroupService userGroupService,
    IFileService fileService)
    : IRequestHandler<CreateContestCommand, ContestDto>
{
    public async Task<ContestDto> Handle(CreateContestCommand request, CancellationToken ct)
    {
        var author = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.CallerId, ct);
        if (author == null)
            throw new EntityNotFoundException(typeof(User), request.CallerId);

        var contestId = Guid.NewGuid();
        
        var participantsGroupId
            = await userGroupService.CreateUserGroupAsync($"Contest {contestId} participants", true, ct);
        foreach (var participantId in request.Participants)
        {
            await userGroupService.AddUserToGroup(participantsGroupId, participantId, ct);
        }
        
        var contest = new Contest
        {
            Id = contestId,
            Name = request.Name,
            StartDate = request.StartDate,
            FinishDate = request.EndDate,
            IsPublic = request.IsPublic,
            AuthorId = author.Id,
            ParticipantsGroupId = participantsGroupId,
            CommissionMembers = [author],
        };
        contest.DescriptionPath = directoryService.GetContestDescriptionRelativePath(contest.Id);
        
        await fileService.SaveContestDescriptionToFileAsync(contest.Id, request.Description, ct);
        
        context.Contests.Add(contest);
        await context.SaveChangesAsync(ct);
        
        return mapper.Map<ContestDto>(contest);
    }
}
