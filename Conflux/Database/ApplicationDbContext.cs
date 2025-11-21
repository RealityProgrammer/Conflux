using Conflux.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Conflux.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options) {
    public override int SaveChanges() {
        InsertTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        InsertTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void InsertTimestamps() {
        var entries = ChangeTracker.Entries().Where(e => e is { State: EntityState.Added, Entity: ICreatedAtColumn });

        DateTime now = DateTime.UtcNow;
        
        foreach (var entry in entries) {
            Unsafe.As<ICreatedAtColumn>(entry.Entity).CreatedAt = now;
        }
    }

    public override void Dispose() {
        base.Dispose();
    }

    public override ValueTask DisposeAsync() {
        return base.DisposeAsync();
    }
}