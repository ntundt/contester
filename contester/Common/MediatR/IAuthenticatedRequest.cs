namespace contester.Common.MediatR;

public interface IAuthenticatedRequest
{
    public Guid CallerId { get; }
}
