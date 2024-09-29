using AutoMapper;
using diploma.Application.AutoMapper;

namespace diploma.Features.Contests;

public class ContestDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? FinishDate { get; set; }
    public int? TimeUntilStartSeconds => (int?)(StartDate - DateTime.UtcNow)?.TotalSeconds;
    public int? TimeUntilFinishSeconds => (int?)(FinishDate - DateTime.UtcNow)?.TotalSeconds;
    public Guid AuthorId { get; set; }
}

public class ContestDtoProfile : Profile
{
    public ContestDtoProfile()
    {
        CreateMap<Contest, ContestDto>()
            .ForMember(d => d.Description, opt => opt.MapFrom<FileTextValueResolver, string>(d => d.DescriptionPath));
    }
}

public class ContestParticipationDto : ContestDto
{
    public bool UserParticipates { get; set; }
}

public class ContestParticipationDtoProfile : Profile
{
    public ContestParticipationDtoProfile()
    {
        CreateMap<Contest, ContestParticipationDto>()
            .ForMember(d => d.Description, opt => opt.MapFrom<FileTextValueResolver, string>(d => d.DescriptionPath))
            .ForMember(d => d.UserParticipates, opt => opt.MapFrom<UserIsContestParticipantCustomResolver>());
    }
}

public class UserIsContestParticipantCustomResolver : IValueResolver<Contest, ContestDto, bool>
{
    public bool Resolve(Contest source, ContestDto destination, bool destMember, ResolutionContext context)
    {
        return source.Participants.Any(p => {
            if (!context.TryGetItems(out var items)) return false;
            return p.Id == (Guid)items["UserId"]!;
        });
    }
}
