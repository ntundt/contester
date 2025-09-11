using System.Security.Authentication;
using contester.Data;
using contester.Features.Authentication.Commands.Common;
using contester.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

public class RenewCredentialsCommand : IRequest<SignInResult>
{
    public string RefreshToken { get; set; } = null!;
}

public class RenewCredentialsCommandHandler(
    IJwtService jwtService, 
    IAuthenticationService authenticationService, 
    ApplicationDbContext context)
    : IRequestHandler<RenewCredentialsCommand, SignInResult>
{
    public async Task<SignInResult> Handle(RenewCredentialsCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Id == jwtService.ExtractUserId(request.RefreshToken), cancellationToken);
        
        if (user is null)
            throw new AuthenticationException("User does not exist");
        
        return authenticationService.ReIssueCredentials(request.RefreshToken, user.UserRole.Name);
    }
}
