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

        builder.Entity<Conversation>(entity => {
            entity.HasKey(conversation => conversation.Id);
        }).Entity<ConversationMember>(entity => {
            entity.HasKey(member => new { member.ConversationId, member.UserId });

            entity.HasIndex(member => member.UserId).IsUnique();

            entity.HasOne(member => member.Conversation)
                .WithMany(conversation => conversation.Members)
                .HasForeignKey(member => member.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(member => member.User)
                .WithMany()
                .HasForeignKey(member => member.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }).Entity<Message>(entity => {
            entity.HasKey(message => message.Id);

            // Does indexing with CreatedAt increase pagination performance?
            entity.HasIndex(message => new { message.ConversationId, message.CreatedAt });

            entity.HasIndex(message => message.SenderId);
            
            entity.HasOne(message => message.Conversation)
                .WithMany(conversation => conversation.Messages)
                .HasForeignKey(message => message.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(message => message.Sender)
                .WithMany()
                .HasForeignKey(message => message.SenderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(m => m.ReplyMessage)
                .WithMany()
                .HasForeignKey(m => m.ReplyMessageId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override void Dispose() {
        base.Dispose();
    }

    public override ValueTask DisposeAsync() {
        return base.DisposeAsync();
    }
}