using AutoMapper;
using contester.Features.Users;

namespace contester.Features.Contests;

public class ContestParticipantDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string AdditionalInfo { get; set; } = null!;

    public bool IsApplicationApproved { get; set; }
    public Guid? ApplicationId { get; set; }
}

public class ContestParticipantDtoProfile : Profile
{
    public ContestParticipantDtoProfile()
    {
        CreateMap<User, ContestParticipantDto>();
    }
}
