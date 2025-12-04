using Conflux.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Conflux.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options) {
    public DbSet<FriendRequest> FriendRequests { get; set; } = default!;
    
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

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity => {
            entity
                .HasMany(user => user.SentFriendRequests)
                .WithOne(request => request.Sender)
                .HasForeignKey(request => request.SenderId)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            
            entity
                .HasMany(user => user.ReceivedFriendRequests)
                .WithOne(request => request.Receiver)
                .HasForeignKey(request => request.ReceiverId)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }).Entity<FriendRequest>(entity => {
            entity.HasKey(request => request.Id);
            
            entity.HasIndex(request => new { request.SenderId, request.ReceiverId }).IsUnique();
        });
    }

    public override void Dispose() {
        base.Dispose();
    }

    public override ValueTask DisposeAsync() {
        return base.DisposeAsync();
    }
}