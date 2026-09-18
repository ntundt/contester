using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.ApplicationSettings.Services;
using MediatR;

namespace contester.Features.ApplicationSettings.Queries;

public class GetAllRuntimeSettingsQuery : IRequest<AllRuntimeSettings>, IAuthorizedRequest
{
    [JsonIgnore] public Guid CallerId { get; set; }
    [JsonIgnore] public Constants.Permission RequiredPermission => Constants.Permission.ManageContests;
}

public class GetAllRuntimeSettingsQueryHandler(
    ISettingsService settingsService
) : IRequestHandler<GetAllRuntimeSettingsQuery, AllRuntimeSettings>
{
    public Task<AllRuntimeSettings> Handle(GetAllRuntimeSettingsQuery request, CancellationToken ct)
    {
        return settingsService.GetAllRuntimeSettings(ct);
    }
}
