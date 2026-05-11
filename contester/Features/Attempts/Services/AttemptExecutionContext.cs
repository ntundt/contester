namespace contester.Features.Attempts.Services;

public class ProblemExecutionSettings
{
    public bool OrderMatters { get; set; }
    public decimal FloatMaxDelta { get; set; }
    public bool CaseSensitive { get; set; }
}

public class AttemptExecutionContext
{
    public string AttemptDbms { get; init; } = null!;
    public string AttemptSchema { get; init; } = null!;
    public string AttemptSolution { get; init; } = null!;
    
    public string EtalonDbms { get; init; } = null!;
    public string EtalonSchema { get; init; } = null!;
    public string EtalonSolution { get; init; } = null!;

    public ProblemExecutionSettings Settings { get; init; } = null!;
}
