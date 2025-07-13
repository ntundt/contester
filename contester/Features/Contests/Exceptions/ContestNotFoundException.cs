namespace contester.Features.Contests.Exceptions;

public class ContestNotFoundException(Guid contestId) : Exception($"Contest with id {contestId} was not found");