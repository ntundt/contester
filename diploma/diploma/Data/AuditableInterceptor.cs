using diploma.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace diploma.Data;

public class AuditableInterceptor : SaveChangesInterceptor
{
    private static void UpdateAuditableEntries(IEnumerable<EntityEntry> entries)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            if (entry.Entity is not AuditableEntity auditable) continue;
            switch (entry.State)
            {
                case EntityState.Added:
                    auditable.CreatedAt = now;
                    auditable.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    auditable.UpdatedAt = now;
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                default:
                    continue;
            }
        }
    }
    
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, 
        InterceptionResult<int> result, CancellationToken cancellationToken = new())
    {
        var entries = eventData.Context!.ChangeTracker.Entries<AuditableEntity>();
        UpdateAuditableEntries(entries);
        return ValueTask.FromResult(result);
    }
    
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditableEntries(eventData.Context!.ChangeTracker.Entries<AuditableEntity>());
        return result;
    }
}