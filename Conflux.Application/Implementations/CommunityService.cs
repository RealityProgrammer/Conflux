using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Conflux.Domain.Enums;
using Conflux.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Application.Implementations;

public class CommunityService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IContentService contentService,
    ICommunityEventDispatcher eventDispatcher,
    ICommunityRoleService roleService,
    IUserNotificationService userNotification
) : ICommunityService {
    public event Action<CommunityCreatedEventArgs>? OnUserCreatedCommunity;

    public async Task<bool> CreateCommunityAsync(string name, Stream? avatarStream, string creatorId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        Community community = new() {
            Name = name,
            CreatorId = creatorId,
            AvatarPath = null,
        };

        dbContext.Communities.Add(community);

        CommunityRole ownerRole = new() {
            Name = "Owners",
            AccessPermissions = AccessPermissionFlags.All,
            ChannelPermissions = ChannelPermissionFlags.All,
            RolePermissions = RolePermissionFlags.All,
            CommunityId = community.Id,
        };

        dbContext.CommunityRoles.Add(ownerRole);

        if (await dbContext.SaveChangesAsync() > 0) {
            dbContext.CommunityMembers.Add(new() {
                Community = community,
                UserId = creatorId,
                RoleId = ownerRole.Id,
            });
            
            if (avatarStream != null) {
                string path = await contentService.UploadCommunityAvatarAsync(avatarStream, community.Id);
                
                community.AvatarPath = path;
            }
            
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
            OnUserCreatedCommunity?.Invoke(new(community));
            
            return true;
        }

        return false;
    }

    public async Task CreateChannelCategoryAsync(string name, Guid communityId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        CommunityChannelCategory category = new() {
            Name = name,
            CommunityId = communityId,
        };
        
        dbContext.CommunityChannelCategories.Add(category);

        if (await dbContext.SaveChangesAsync() > 0) {
            await eventDispatcher.Dispatch(new ChannelCategoryCreatedEventArgs(communityId, category.Id, name));
        }
    }

    public async Task CreateChannelAsync(string name, CommunityChannelType type, Guid channelCategoryId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        
        Guid communityId = await dbContext.CommunityChannelCategories
            .Where(x => x.Id == channelCategoryId)
            .Select(x => x.CommunityId)
            .FirstOrDefaultAsync();

        if (communityId == Guid.Empty) {
            return;
        }
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        
        CommunityChannel channel = new() {
            Name = name,
            Type = type,
            ChannelCategoryId = channelCategoryId,
        };
        
        dbContext.CommunityChannels.Add(channel);

        Conversation conversation = new() {
            CommunityChannelId = channel.Id,
        };

        dbContext.Conversations.Add(conversation);
        
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        
        await eventDispatcher.Dispatch(new ChannelCreatedEventArgs(communityId, channelCategoryId, channel.Id, name, type));
    }

    public async Task<bool> JoinCommunityAsync(string userId, Guid communityId, Guid invitationId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        
        // Early return if user already in the community.
        if (await dbContext.CommunityMembers.Where(x => x.UserId == userId && x.CommunityId == communityId).AnyAsync()) {
            return false;
        }
        
        // Determine if the invitation id is valid.
        if (!await dbContext.Communities.Where(x => x.Id == communityId && x.InvitationId == invitationId).AnyAsync()) {
            return false;
        }

        CommunityMember member = new() {
            UserId = userId,
            CommunityId = communityId,
        };

        dbContext.CommunityMembers.Add(member);

        if (await dbContext.SaveChangesAsync() > 0) {
            await eventDispatcher.Dispatch(new CommunityMemberJoinedEventArgs(communityId, userId));
            
            return true;
        }

        return false;
    }

    public async Task<RolePermissionsWithId?> GetUserRolePermissionsAsync(string userId, Guid communityId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        Guid? memberRoleId = await dbContext.CommunityMembers
            .Where(x => x.UserId == userId && x.CommunityId == communityId)
            .Select(x => x.RoleId)
            .FirstOrDefaultAsync();

        if (memberRoleId is not { } roleId) {
            return null;
        }

        if (await roleService.GetPermissionsAsync(dbContext, roleId) is not { } permissions) {
            return new(roleId, RolePermissions.Default);
        }

        return new(roleId, permissions);
    }

    public async Task<bool> SetMembersRoleAsync(Guid communityId, IReadOnlyCollection<Guid> memberIds, Guid? roleId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        int affected = await dbContext.CommunityMembers
            .Where(x => memberIds.Contains(x.Id))
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(x => x.RoleId, roleId);
            });

        if (affected > 0) {
            await eventDispatcher.Dispatch(new MemberRoleChangedEventArgs(communityId, roleId));
        }

        return true;
    }

    public async Task<Guid> GetMemberId(Guid communityId, string userId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.CommunityMembers
            .Where(x => x.CommunityId == communityId && x.UserId == userId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<MemberDisplayDTO?> GetMemberDisplayAsync(Guid memberId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.CommunityMembers
            .Where(x => x.Id == memberId)
            .Include(x => x.User)
            .Select(x => new MemberDisplayDTO(x.Id, x.UserId, x.User.DisplayName, x.User.AvatarProfilePath))
            .Cast<MemberDisplayDTO?>()
            .FirstOrDefaultAsync();
    }

    public async Task<MemberDisplayDTO?> GetMemberDisplayAsync(Guid communityId, string userId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.CommunityMembers
            .Where(x => x.CommunityId == communityId && x.UserId == userId)
            .Include(x => x.User)
            .Select(x => new MemberDisplayDTO(x.Id, x.UserId, x.User.DisplayName, x.User.AvatarProfilePath))
            .Cast<MemberDisplayDTO?>()
            .FirstOrDefaultAsync();
    }
    
    public async Task<bool> BanMemberAsync(Guid communityId, Guid memberId, TimeSpan banDuration) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await BanMemberAsync(dbContext, communityId, memberId, banDuration);
    }

    public async Task<bool> BanMemberAsync(ApplicationDbContext dbContext, Guid communityId, Guid memberId, TimeSpan banDuration) {
        int affected = await dbContext.CommunityMembers
            .Where(m => m.CommunityId == communityId && m.Id == memberId)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(
                    m => m.UnbanAt, 
                    m => m.UnbanAt == null ? DateTime.UtcNow + banDuration : m.UnbanAt.Value + banDuration
                );
            });

        if (affected > 0) {
            var userId = await dbContext.CommunityMembers
                .Where(m => m.CommunityId == communityId && m.Id == memberId)
                .Select(m => m.UserId)
                .FirstAsync();

            await userNotification.Dispatch(new CommunityBannedEventArgs(communityId, memberId, userId));
            return true;
        }

        return false;
    }

    public async Task<MemberInformationDTO?> GetMemberInformationAsync(Guid memberId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.CommunityMembers
            .Where(m => m.Id == memberId)
            .Include(m => m.Role)
            .Select(m => 
                new MemberInformationDTO(m.Id, m.Role == null ? 
                    new RolePermissionsWithId(null, RolePermissions.Default) : 
                    new(m.RoleId, new(m.Role.ChannelPermissions, m.Role.RolePermissions, m.Role.AccessPermissions)), m.UnbanAt))
            .Cast<MemberInformationDTO?>()
            .FirstOrDefaultAsync();
    }

    public async Task<MemberInformationDTO?> GetMemberInformationAsync(Guid communityId, string userId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await dbContext.CommunityMembers
            .Where(m => m.CommunityId == communityId && m.UserId == userId)
            .Include(m => m.Role)
            .Select(m => 
                new MemberInformationDTO(m.Id, m.Role == null ? 
                    new RolePermissionsWithId(null, RolePermissions.Default) : 
                    new(m.RoleId, new(m.Role.ChannelPermissions, m.Role.RolePermissions, m.Role.AccessPermissions)), m.UnbanAt))
            .Cast<MemberInformationDTO?>()
            .FirstOrDefaultAsync();
    }
}