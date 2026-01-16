using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Conflux.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Application.Implementations;

public class CommunityService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IContentService contentService,
    ICommunityEventDispatcher eventDispatcher,
    ICommunityCacheService communityCache
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
            AccessPermissions = CommunityRole.AccessPermissionFlags.All,
            ChannelPermissions = CommunityRole.ChannelPermissionFlags.All,
            RolePermissions = CommunityRole.RolePermissionFlags.All,
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

    public async Task<ICommunityService.CreateRoleStatus> CreateRoleAsync(Guid communityId, string roleName) {
        if (roleName == "Owners") {
            return ICommunityService.CreateRoleStatus.ReservedName;
        }
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        if (dbContext.CommunityRoles.Any(x => x.CommunityId == communityId && x.Name == roleName)) {
            return ICommunityService.CreateRoleStatus.NameExists;
        }
        
        CommunityRole role = new() {
            CommunityId = communityId,
            Name = roleName,
        };
        
        dbContext.CommunityRoles.Add(role);

        if (await dbContext.SaveChangesAsync() > 0) {
            await eventDispatcher.Dispatch(new CommunityRoleCreatedEventArgs(communityId, role.Id, roleName));
            
            return ICommunityService.CreateRoleStatus.Success;
        }
        
        return ICommunityService.CreateRoleStatus.Failure;
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
    
    public async Task<RolePermissions?> GetPermissionsAsync(Guid roleId) {
        if (await communityCache.GetPermissionsAsync(roleId) is { } cached) {
            return cached;
        }
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var permissions = await CollectPermissions(dbContext, roleId);

        if (permissions == null) {
            return null;
        }
        
        await communityCache.StorePermissionsAsync(roleId, permissions);

        return permissions;
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
        
        if (await communityCache.GetPermissionsAsync(roleId) is { } permissions) {
            return new(roleId, permissions);
        }
        
        permissions = await CollectPermissions(dbContext, roleId);

        if (permissions == null) {
            return null;
        }
        
        await communityCache.StorePermissionsAsync(roleId, permissions);

        return new(roleId, permissions);
    }

    public async Task<bool> UpdatePermissionsAsync(Guid communityId, Guid roleId, RolePermissions permissions) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        int numUpdatedRows = await dbContext.CommunityRoles
            .Where(x => x.CommunityId == communityId && x.Id == roleId)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(x => x.ChannelPermissions, permissions.Channel);
                builder.SetProperty(x => x.RolePermissions, permissions.Role);
                builder.SetProperty(x => x.AccessPermissions, permissions.Access);
            });

        if (numUpdatedRows == 0) {
            return false;
        }
        
        await communityCache.StorePermissionsAsync(roleId, permissions);
        await eventDispatcher.Dispatch(new CommunityRolePermissionUpdatedEventArg(communityId, roleId));
        
        return true;
    }

    private async Task<RolePermissions?> CollectPermissions(ApplicationDbContext dbContext, Guid roleId) {
        return await dbContext.CommunityRoles
            .Where(x => x.Id == roleId)
            .Select(x => new RolePermissions(x.ChannelPermissions, x.RolePermissions, x.AccessPermissions))
            .FirstOrDefaultAsync();
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

    public async Task<bool> RenameRoleAsync(Guid communityId, Guid roleId, string name) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        int affected = await dbContext.CommunityRoles
            .Where(x => x.CommunityId == communityId && x.Id == roleId)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(x => x.Name, name);
            });

        if (affected > 0) {
            await eventDispatcher.Dispatch(new CommunityRoleRenamedEventArgs(communityId, roleId, name));
            
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteRoleAsync(Guid communityId, Guid roleId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        string? roleName = await dbContext.CommunityRoles
            .Where(r => r.CommunityId == communityId && r.Id == roleId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(roleName) || roleName == "Owners") {
            return false;
        }

        int affected = await dbContext.CommunityRoles
            .Where(r => r.CommunityId == communityId && r.Id == roleId)
            .ExecuteDeleteAsync();

        if (affected > 0) {
            await eventDispatcher.Dispatch(new CommunityRoleDeletedEventArgs(communityId, roleId));
            
            return true;
        }

        return false;
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
        throw new NotImplementedException();
    }
}