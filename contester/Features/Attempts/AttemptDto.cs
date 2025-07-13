using AutoMapper;
using contester.Application.AutoMapper;

namespace contester.Features.Attempts;

public class AttemptDto
{
    public Guid Id { get; set; }
    public Guid ProblemId { get; set; }
    public Guid AuthorId { get; set; }
    public AttemptStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string AuthorFirstName { get; set; } = null!;
    public string AuthorLastName { get; set; } = null!;
    public string? AuthorPatronymic { get; set; } = null!;
    public string ProblemName { get; set; } = null!;
    public int MaxGrade { get; set; }
    public int Grade { get; set; }
    public string Dbms { get; set; } = null!;
    public int? Originality { get; set; }
}

public class AttemptProfile : Profile
{
    public AttemptProfile()
    {
        CreateMap<Attempt, AttemptDto>()
            .ForMember(x => x.AuthorFirstName, opt => opt.MapFrom(x => x.Author.FirstName))
            .ForMember(x => x.AuthorLastName, opt => opt.MapFrom(x => x.Author.LastName))
            .ForMember(x => x.AuthorPatronymic, opt => opt.MapFrom(x => x.Author.Patronymic))
            .ForMember(x => x.ProblemName, opt => opt.MapFrom(x => x.Problem.Name))
            .ForMember(x => x.Originality, opt => opt.MapFrom(x => x.Originality));
    }
}

public class SingleAttemptDto : AttemptDto
{
    public string Solution { get; set; } = null!;
    public Guid OriginalAttemptId { get; set; }
}

public class SingleAttemptProfile : Profile
{
    public SingleAttemptProfile()
    {
        CreateMap<Attempt, SingleAttemptDto>()
            .ForMember(x => x.AuthorFirstName, opt => opt.MapFrom(x => x.Author.FirstName))
            .ForMember(x => x.AuthorLastName, opt => opt.MapFrom(x => x.Author.LastName))
            .ForMember(x => x.AuthorPatronymic, opt => opt.MapFrom(x => x.Author.Patronymic))
            .ForMember(x => x.ProblemName, opt => opt.MapFrom(x => x.Problem.Name))
            .ForMember(x => x.Solution, opt => opt.MapFrom<FileTextValueResolver, string>(s => s.SolutionPath))
            .ForMember(x => x.Originality, opt => opt.MapFrom(x => x.Originality))
            .ForMember(x => x.OriginalAttemptId, opt => opt.MapFrom(x => x.OriginalAttemptId));
    }
}
