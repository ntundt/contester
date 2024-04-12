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
    public int TimeUntilStartSeconds => (int)(StartDate - DateTime.UtcNow).TotalSeconds;
    public int TimeUntilFinishSeconds => (int)(FinishDate - DateTime.UtcNow).TotalSeconds;
    public bool IsPublic { get; set; }
    public List<UserDto> CommissionMembers { get; set; } = null!;
}

public class ContestSettingsDtoProfile : Profile
{
    public ContestSettingsDtoProfile()
    {
        CreateMap<Contest, ContestSettingsDto>()
            .ForMember(d => d.CommissionMembers, opt => opt.MapFrom(s => s.CommissionMembers))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => File.ReadAllText(s.DescriptionPath)));
    }
}