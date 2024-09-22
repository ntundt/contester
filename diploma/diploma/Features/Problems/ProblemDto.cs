using AutoMapper;
using diploma.Services;

namespace diploma.Features.Problems;

public class ProblemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Statement { get; set; } = null!;
    
    public bool OrderMatters { get; set; }
    public decimal FloatMaxDelta { get; set; }
    public bool CaseSensitive { get; set; }
    public TimeSpan TimeLimit { get; set; }
    public int MaxGrade { get; set; }
    public int Ordinal { get; set; }
    public Guid SchemaDescriptionId { get; set; }
    
    public List<string> AvailableDbms { get; set; } = null!;
}

public class ExpectedSolutionDto
{
    public Guid ProblemId { get; set; }
    public string Dbms { get; set; } = null!;
    public string Solution { get; set; } = null!;
}

public class ProblemProfile : Profile
{
    public ProblemProfile(IFileService fileService)
    {
        CreateMap<Problem, ProblemDto>()
            .ForMember(d => d.Statement, o => o.MapFrom(s =>
                fileService.ReadApplicationDirectoryFileAllText(s.StatementPath)))
            .ForMember(d => d.AvailableDbms, o => o.MapFrom(s => s.SchemaDescription.Files.Select(f => f.Dbms)));
    }
}

public class ExpectedSolutionProfile : Profile
{
    public ExpectedSolutionProfile(IFileService fileService)
    {
        CreateMap<Problem, ExpectedSolutionDto>()
            .ForMember(d => d.ProblemId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Dbms, o => o.MapFrom(s => s.SolutionDbms))
            .ForMember(d => d.Solution, o => o.MapFrom(s =>
                fileService.ReadApplicationDirectoryFileAllText(s.SolutionPath)));
    }
}
