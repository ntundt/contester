using System.Globalization;
using System.Reflection;
using System.Text.Json;
using contester.Features.Common.Exceptions;
using contester.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ApplicationSettings.Services;

public interface ISettingsService
{
    Task<string> GetSetting(string key, CancellationToken ct);
    Task SetSetting(string key, string value, CancellationToken ct);
    Task<AllRuntimeSettings> GetAllRuntimeSettings(CancellationToken ct);
}

public class SettingsService(ApplicationDbContext context) : ISettingsService
{
    private static string SettingKey(PropertyInfo property) =>
        JsonNamingPolicy.CamelCase.ConvertName(property.Name);

    public async Task<string> GetSetting(string key, CancellationToken ct = default)
    {
        var setting = await context.Settings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null)
            throw new EntityNotFoundException(typeof(Setting), key);
        
        return setting.Value;
    }

    public async Task SetSetting(string key, string value, CancellationToken ct = default)
    {
        var setting = await context.Settings
            .FirstOrDefaultAsync(s => s.Key == key, ct);

        if (setting is null)
        {
            setting = new Setting
            {
                Key = key,
                Value = value
            };
            await context.Settings.AddAsync(setting, ct);
        }
        else
        {
            setting.Value = value;
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task<AllRuntimeSettings> GetAllRuntimeSettings(CancellationToken ct = default)
    {
        var result = new AllRuntimeSettings();
        var properties = typeof(AllRuntimeSettings)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        var keys = properties.Select(SettingKey).ToList();
        var stored = await context.Settings.AsNoTracking()
            .Where(s => keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, s => s.Value, ct);

        foreach (var property in properties)
        {
            if (!stored.TryGetValue(SettingKey(property), out var rawValue))
                continue;

            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var converted = Convert.ChangeType(rawValue, targetType, CultureInfo.InvariantCulture);
            property.SetValue(result, converted);
        }

        return result;
    }
}