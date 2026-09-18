using contester.Features.ApplicationSettings.Services;
using MediatR;

namespace contester.Features.ApplicationSettings.Queries;

public class GetPrivacyPolicyQuery : IRequest<GetPrivacyPolicyQueryResult>;

public class GetPrivacyPolicyQueryResult
{
    public string PrivacyPolicy { get; set; } = string.Empty;
    public bool PrivacyPolicyConsentRequired { get; set; }
}

public class GetPrivacyPolicyQueryHandler(ISettingsService settingsService) : IRequestHandler<GetPrivacyPolicyQuery, GetPrivacyPolicyQueryResult>
{
    public async Task<GetPrivacyPolicyQueryResult> Handle(GetPrivacyPolicyQuery request, CancellationToken ct)
    {
        var policy = await settingsService.GetAllRuntimeSettings(ct);
        return new GetPrivacyPolicyQueryResult
        {
            PrivacyPolicy = policy.PrivacyPolicy,
            PrivacyPolicyConsentRequired = policy.PrivacyPolicyConsentRequired,
        };
    }
}