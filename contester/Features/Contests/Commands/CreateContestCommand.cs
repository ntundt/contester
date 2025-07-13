using AutoMapper;
using contester.Data;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Users.Exceptions;
using contester.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Commands;

public class CreateContestCommand : IRequest<ContestDto>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublic { get; set; }
    public Guid CallerId { get; set; }
    public List<Guid> Participants { get; set; } = null!;
}

public class CreateContestCommandHandler(
    ApplicationDbContext context,
    IMapper mapper,
    IPermissionService permissionService,
    IDirectoryService directoryService,
    IFileService fileService)
    : IRequestHandler<CreateContestCommand, ContestDto>
{
    public async Task<ContestDto> Handle(CreateContestCommand request, CancellationToken cancellationToken)
    {
        var author = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.CallerId, cancellationToken);
        if (author == null)
        {
            throw new UserNotFoundException();
        }
        
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageContests, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageContests);
        }
        
        var contest = new Contest()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            StartDate = request.StartDate,
            FinishDate = request.EndDate,
            IsPublic = request.IsPublic,
            AuthorId = author.Id,
            Participants = await context.Users.Where(u => request.Participants.Contains(u.Id)).ToListAsync(cancellationToken),
            CommissionMembers = [author],
        };
        contest.DescriptionPath = directoryService.GetContestDescriptionRelativePath(contest.Id);
        
        await fileService.SaveContestDescriptionToFileAsync(contest.Id, request.Description, cancellationToken);
        
        context.Contests.Add(contest);
        await context.SaveChangesAsync(cancellationToken);
        
        return mapper.Map<ContestDto>(contest);
    }
}
