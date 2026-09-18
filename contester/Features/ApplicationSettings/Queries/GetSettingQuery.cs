using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.ApplicationSettings.Services;
using MediatR;

namespace contester.Features.ApplicationSettings.Queries;

public class GetSettingQuery : IRequest<Setting>, IAuthorizedRequest
{
    public required string Key { get; set; }
    [JsonIgnore] public Guid CallerId { get; set; }
    [JsonIgnore] public Constants.Permission RequiredPermission => Constants.Permission.ManageContests;
}

public class GetSettingQueryHandler(
    ISettingsService settingsService
) : IRequestHandler<GetSettingQuery, Setting>
{
    public async Task<Setting> Handle(GetSettingQuery request, CancellationToken ct)
    {
        return new Setting
        {
            Key = request.Key,
            Value = await settingsService.GetSetting(request.Key, ct),
        };
    }
}
