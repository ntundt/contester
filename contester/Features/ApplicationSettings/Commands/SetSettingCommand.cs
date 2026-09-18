using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.ApplicationSettings.Services;
using MediatR;

namespace contester.Features.ApplicationSettings.Commands;

public class SetSettingCommand : IRequest<Setting>, IAuthorizedRequest
{
    public required string Key { get; set; }
    public string Value { get; set; } = null!;
    [JsonIgnore] public Guid CallerId { get; set; }
    [JsonIgnore] public Constants.Permission RequiredPermission => Constants.Permission.ManageContests;
}

public class SetSettingCommandHandler(
    ISettingsService settingsService
) : IRequestHandler<SetSettingCommand, Setting>
{
    public async Task<Setting> Handle(SetSettingCommand request, CancellationToken ct)
    {
        await settingsService.SetSetting(request.Key, request.Value, ct);
        return new Setting
        {
            Key = request.Key,
            Value = request.Value,
        };
    }
}
