namespace contester.Common.MediatR;

public interface IAuthorizedRequest : IAuthenticatedRequest
{
    public Constants.Permission RequiredPermission { get; set; } 
}
