using AutoMapper;
using contester.Infrastructure.AutoMapper;

namespace contester.Features.Contests;

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
