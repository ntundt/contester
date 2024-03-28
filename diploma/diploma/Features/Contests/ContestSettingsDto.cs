using AutoMapper;
using diploma.Features.Users;

namespace diploma.Features.Contests;

public class ContestSettingsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public bool IsPublic { get; set; }
    public List<UserDto> CommissionMembers { get; set; } = null!;
}

public class ContestSettingsDtoProfile : Profile
{
    public ContestSettingsDtoProfile()
    {
        CreateMap<Contest, ContestSettingsDto>()
            .ForMember(d => d.CommissionMembers, opt => opt.MapFrom(s => s.CommissionMembers));
    }
}