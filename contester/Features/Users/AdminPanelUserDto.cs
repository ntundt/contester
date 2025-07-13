using AutoMapper;

namespace contester.Features.Users;

public class AdminPanelUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string AdditionalInfo { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsEmailConfirmed { get; set; }
}

public class AdminPanelUserProfile : Profile
{
    public AdminPanelUserProfile()
    {
        CreateMap<User, AdminPanelUserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.UserRole.Name));
    }
}