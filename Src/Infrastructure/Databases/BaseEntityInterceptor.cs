using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VerticalSlicesDemo.Domain.Entities;

namespace VerticalSlicesDemo.Infrastructure.Databases;

internal sealed class BaseEntityInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditableEntities(DbContext context)
    {
        var utcNow = DateTime.UtcNow;
        var entities = context.ChangeTracker.Entries<BaseEntity>().ToList();

        foreach (EntityEntry<BaseEntity> entry in entities)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Property(x => x.CreatedAt).IsModified = false;

                    if (entry.Properties.Any(p => p.IsModified))
                    {
                        entry.Entity.UpdatedAt = utcNow;
                    }

                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.UpdatedAt = utcNow;
                    entry.Entity.DeletedAt = utcNow;
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                    break;
            }
        }
    }
}
