using contester.Features.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace contester.Infrastructure.Persistence;

public class AuditableInterceptor(
    Features.Authentication.Services.IAuthorizationService authorizationService) : SaveChangesInterceptor
{
    private static string? SerializeProperties(
        IEnumerable<PropertyEntry> properties,
        bool original)
    {
        var dict = properties
            .ToDictionary(
                p => p.Metadata.Name,
                p => original ? p.OriginalValue : p.CurrentValue
            );

        return dict.Count == 0
            ? null
            : System.Text.Json.JsonSerializer.Serialize(dict);
    }
    
    private static Audit CreateAuditEntry(EntityEntry entry, Guid? userId, DateTime now)
    {
        var oldValues = entry.State is EntityState.Modified or EntityState.Deleted
            ? SerializeProperties(entry.Properties, original: true)
            : null;

        var newValues = entry.State is EntityState.Added or EntityState.Modified
            ? SerializeProperties(entry.Properties, original: false)
            : null;

        return new Audit
        {
            UserId = userId,
            Type = entry.State.ToString(),
            TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
            Date = now,
            OldValues = oldValues,
            NewValues = newValues
        };
    }
    
    private void HandleAuditing(DbContext context)
    {
        var now = DateTime.UtcNow;
        var userId = authorizationService.TryGetUserId();

        var auditEntries = new List<Audit>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditableEntity auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditable.CreatedAt = now;
                        auditable.UpdatedAt = now;
                        break;
                    case EntityState.Modified:
                        auditable.UpdatedAt = now;
                        break;
                }
            }

            // skip audit entity itself to avoid recursion
            if (entry.Entity is Audit)
                continue;

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var audit = CreateAuditEntry(entry, userId, now);
            auditEntries.Add(audit);
        }

        context.Set<Audit>().AddRange(auditEntries);
    }
    
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, 
        InterceptionResult<int> result, CancellationToken cancellationToken = new())
    {
        if (eventData.Context != null)
        {
            HandleAuditing(eventData.Context);
        }

        return ValueTask.FromResult(result);
    }
    
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            HandleAuditing(eventData.Context);
        }

        return result;
    }}