using contester.Data;
using contester.Exceptions;
using contester.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Queries;

public class GetEmailConfirmationLinkByCodeQuery : IRequest<GetEmailConfirmationLinkByCodeQueryResult>
{
    public string Code { get; set; } = null!;
    public Guid UserId { get; set; }
}

public class GetEmailConfirmationLinkByCodeQueryResult
{
    public string Link { get; set; } = null!;
}

public class GetEmailConfirmationLinkByCodeQueryHandler(
    ApplicationDbContext context,
    IAuthenticationService authenticationService)
    : IRequestHandler<GetEmailConfirmationLinkByCodeQuery,
        GetEmailConfirmationLinkByCodeQueryResult>
{
    public async Task<GetEmailConfirmationLinkByCodeQueryResult> Handle(GetEmailConfirmationLinkByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.EmailConfirmationCode == request.Code && u.Id == request.UserId);
        if (user is null)
        {
            throw new NotifyUserException("Code is invalid");
        }

        if (user.EmailConfirmationCodeExpiresAt < DateTime.UtcNow)
        {
            throw new NotifyUserException("Code is expired. Please request it once more by signing up with the same email.");
        }

        return new GetEmailConfirmationLinkByCodeQueryResult
        {
            Link = authenticationService.GetEmailConfirmationUrl(user.EmailConfirmationToken)
        };
    }
}
