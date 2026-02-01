using contester.Infrastructure;
using MediatR;

namespace contester.Features.ApplicationSettings.Queries;

public class GetPublicServerConfigurationQuery : IRequest<GetPublicServerConfigurationQueryResult>;

public class GetPublicServerConfigurationQueryResult
{
    public bool PasswordlessAuthentication { get; set; }
}

public class GetPublicServerConfigurationQueryHandler(IConfigurationReaderService configuration) : IRequestHandler<GetPublicServerConfigurationQuery,
    GetPublicServerConfigurationQueryResult>
{
    public Task<GetPublicServerConfigurationQueryResult> Handle(GetPublicServerConfigurationQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GetPublicServerConfigurationQueryResult
        {
            PasswordlessAuthentication = configuration.IsPasswordlessAuthenticationEnabled(),
        });
    }
}
