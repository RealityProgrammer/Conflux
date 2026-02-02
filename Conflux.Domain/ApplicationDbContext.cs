using Conflux.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.Runtime.CompilerServices;

namespace Conflux.Domain;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options) {
    public DbSet<FriendRequest> FriendRequests { get; set; } = null!;
    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<Community> Communities { get; set; } = null!;
    public DbSet<CommunityMember> CommunityMembers { get; set; } = null!;
    public DbSet<CommunityChannel> CommunityChannels { get; set; } = null!;
    public DbSet<CommunityChannelCategory> CommunityChannelCategories { get; set; } = null!;
    public DbSet<CommunityRole> CommunityRoles { get; set; } = null!;
    public DbSet<MessageReport> MessageReports { get; set; } = null!;
    
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
                .HasPrincipalKey(user => user.Id)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            
            entity
                .HasMany(user => user.ReceivedFriendRequests)
                .WithOne(request => request.Receiver)
                .HasForeignKey(request => request.ReceiverId)
                .HasPrincipalKey(user => user.Id)
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
                .HasForeignKey(message => message.ConversationId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            
            entity.HasOne(conversation => conversation.FriendRequest)
                .WithOne(request => request.Conversation)
                .HasForeignKey<Conversation>(conversation => conversation.FriendRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(conversation => conversation.CommunityChannel)
                .WithOne(channel => channel.Conversation)
                .HasForeignKey<Conversation>(conversation => conversation.CommunityChannelId)
                .OnDelete(DeleteBehavior.Cascade);
        }).Entity<ChatMessage>(entity => {
            entity.HasKey(x => x.Id);
            
            entity.HasIndex(message => new { message.ConversationId, message.CreatedAt });
            
            entity.HasOne(message => message.Sender)
                .WithMany()
                .HasForeignKey(message => message.SenderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(message => message.ReplyMessage)
                .WithMany()
                .HasForeignKey(message => message.ReplyMessageId);

            entity.HasOne(message => message.DeleterUser)
                .WithMany()
                .HasForeignKey(message => message.DeleterUserId);

            entity.ComplexCollection(m => m.Attachments, action => action.ToJson());
        });

        builder.Entity<Community>(entity => {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.InvitationId).HasValueGenerator<GuidValueGenerator>();

            entity.HasOne(community => community.Creator)
                .WithMany(user => user.CreatedCommunities)
                .HasForeignKey(community => community.CreatorId)
                .HasPrincipalKey(user => user.Id)
                .IsRequired();

            entity.HasMany(community => community.ChannelCategories)
                .WithOne(channelCategory => channelCategory.Community)
                .HasForeignKey(channelCategory => channelCategory.CommunityId)
                .HasPrincipalKey(channel => channel.Id)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.HasMany(community => community.Roles)
                .WithOne(role => role.Community)
                .HasForeignKey(role => role.CommunityId)
                .HasPrincipalKey(community => community.Id)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }).Entity<CommunityMember>(entity => {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.CommunityId, x.UserId }).IsUnique();
            
            entity.HasOne(member => member.Community)
                .WithMany(community => community.Members)
                .HasForeignKey(member => member.CommunityId)
                .HasPrincipalKey(member => member.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(member => member.User)
                .WithMany(user => user.CommunityMembers)
                .HasForeignKey(member => member.UserId)
                .HasPrincipalKey(user => user.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(member => member.Role)
                .WithMany(role => role.MembersWithRole)
                .HasForeignKey(member => member.RoleId)
                .HasPrincipalKey(role => role.Id)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter("BanFilter", m => !(m.UnbanAt != null && m.UnbanAt > DateTime.UtcNow));
        }).Entity<CommunityChannelCategory>(entity => {
            entity.HasKey(x => x.Id);

            entity.HasMany(category => category.Channels)
                .WithOne(channel => channel.ChannelCategory)
                .HasForeignKey(channel => channel.ChannelCategoryId)
                .HasPrincipalKey(category => category.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }).Entity<CommunityChannel>(entity => {
            entity.HasKey(x => x.Id);
        });

        builder.Entity<MessageReport>(entity => {
            entity.HasKey(x => x.Id);

            entity.HasOne(report => report.Message)
                .WithMany()
                .HasForeignKey(report => report.MessageId)
                .HasPrincipalKey(message => message.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(report => report.Reporter)
                .WithMany()
                .HasForeignKey(report => report.ReporterId)
                .HasPrincipalKey(user => user.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(report => report.Resolver)
                .WithMany()
                .HasForeignKey(report => report.ResolverId)
                .HasPrincipalKey(member => member.Id)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.ComplexCollection(x => x.OriginalMessageAttachments, action => action.ToJson());
        });
    }
}