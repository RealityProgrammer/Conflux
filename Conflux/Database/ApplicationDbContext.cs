using Conflux.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Conflux.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options) {
    public DbSet<FriendRequest> FriendRequests { get; set; } = null!;
    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<ConversationMember> ConversationMembers { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<MessageAttachment> MessageAttachments { get; set; } = null!;
    public DbSet<Community> Communities { get; set; } = null!;
    public DbSet<CommunityMember> CommunityMembers { get; set; } = null!;
    public DbSet<CommunityChannel> CommunityChannels { get; set; } = null!;
    public DbSet<CommunityChannelCategory> CommunityChannelCategories { get; set; } = null!;
    
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
            entity.HasKey(x => x.Id);

            entity.HasMany(conversation => conversation.Messages)
                .WithOne(message => message.Conversation)
                .HasForeignKey(conversation => conversation.ConversationId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }).Entity<ConversationMember>(entity => {
            entity.HasKey(member => new { member.ConversationId, member.UserId });

            entity.HasIndex(member => member.UserId);
            
            entity.HasOne(member => member.Conversation)
                .WithMany(conversation => conversation.Members)
                .HasForeignKey(member => member.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }).Entity<ChatMessage>(entity => {
            entity.HasKey(x => x.Id);
            
            entity.HasIndex(message => new { message.ConversationId, message.CreatedAt });
            
            entity.HasOne(message => message.Sender)
                .WithMany()
                .HasForeignKey(message => message.SenderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(m => m.ReplyMessage)
                .WithMany()
                .HasForeignKey(m => m.ReplyMessageId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(m => m.Attachments)
                .WithOne(attachment => attachment.Message)
                .HasForeignKey(attachment => attachment.MessageId)
                .IsRequired();
        }).Entity<MessageAttachment>(entity => {
            entity.HasKey(message => message.Id);
        });

        builder.Entity<Community>(entity => {
            entity.HasKey(x => x.Id);

            entity.HasOne(server => server.Owner)
                .WithMany(owner => owner.OwnedServers)
                .HasForeignKey(server => server.OwnerId)
                .HasPrincipalKey(user => user.Id)
                .IsRequired();

            entity.HasOne(server => server.Creator)
                .WithMany(creator => creator.CreatedServers)
                .HasForeignKey(server => server.CreatorId)
                .HasPrincipalKey(user => user.Id)
                .IsRequired();

            entity.HasMany(server => server.ChannelCategories)
                .WithOne(channelCategory => channelCategory.Community)
                .HasForeignKey(channelCategory => channelCategory.CommunityId)
                .HasPrincipalKey(channel => channel.Id)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }).Entity<CommunityMember>(entity => {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.CommunityServerId, x.UserId }).IsUnique();
            
            entity.HasOne(member => member.Community)
                .WithMany(server => server.Members)
                .HasForeignKey(member => member.CommunityServerId)
                .HasPrincipalKey(member => member.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(member => member.User)
                .WithMany(user => user.CommunityMembers)
                .HasForeignKey(member => member.UserId)
                .HasPrincipalKey(user => user.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }).Entity<CommunityChannelCategory>(entity => {
            entity.HasKey(x => x.Id);

            entity.HasMany(x => x.Channels)
                .WithOne(x => x.ChannelCategory)
                .HasForeignKey(x => x.ChannelCategoryId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override void Dispose() {
        base.Dispose();
    }

    public override ValueTask DisposeAsync() {
        return base.DisposeAsync();
    }
}