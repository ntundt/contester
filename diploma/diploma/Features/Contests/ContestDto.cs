using AutoMapper;
using diploma.Features.Users;

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
    public Guid AuthorId { get; set; }
    public List<UserDto> CommissionMembers { get; set; } = null!;
}

public class ContestDtoProfile : Profile
{
    public ContestDtoProfile()
    {
        CreateMap<Contest, ContestDto>()
            .ForMember(d => d.Description, opt => opt.MapFrom(s => File.ReadAllText(s.DescriptionPath)));
    }
}