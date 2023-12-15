namespace diploma.Features.Contests.Exceptions;

public class ContestNotFoundException : Exception
{
    public ContestNotFoundException(Guid contestId) : base($"Contest with id {contestId} was not found")
    {
    }
}