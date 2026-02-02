using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Conflux.Domain;
using Conflux.Domain.Entities;
using Conflux.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Application.Implementations;

public class CommunityRoleService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    ICommunityEventDispatcher eventDispatcher
) : ICommunityRoleService {
    public async Task<ICommunityRoleService.CreateRoleStatus> CreateRoleAsync(Guid communityId, string roleName) {
        if (roleName == "Owners") {
            return ICommunityRoleService.CreateRoleStatus.ReservedName;
        }
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        if (dbContext.CommunityRoles.Any(x => x.CommunityId == communityId && x.Name == roleName)) {
            return ICommunityRoleService.CreateRoleStatus.NameExists;
        }
        
        CommunityRole role = new() {
            CommunityId = communityId,
            Name = roleName,
            CreatedAt = DateTime.UtcNow,
        };
        
        dbContext.CommunityRoles.Add(role);

        if (await dbContext.SaveChangesAsync() > 0) {
            await eventDispatcher.Dispatch(new CommunityRoleCreatedEventArgs(communityId, role.Id, roleName));
            
            return ICommunityRoleService.CreateRoleStatus.Success;
        }
        
        return ICommunityRoleService.CreateRoleStatus.Failure;
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
    
    public async Task<RolePermissions?> GetPermissionsAsync(Guid roleId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return await GetPermissionsAsync(dbContext, roleId);
    }
    
    public async Task<RolePermissions?> GetPermissionsAsync(ApplicationDbContext dbContext, Guid roleId) {
        return await dbContext.CommunityRoles
            .Where(r => r.Id == roleId)
            .Select(r => new RolePermissions(r.ChannelPermissions, r.RolePermissions, r.AccessPermissions, r.ManagementPermissions))
            .FirstOrDefaultAsync();
    }
    
    public async Task<bool> UpdatePermissionsAsync(Guid roleId, RolePermissions permissions) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        Guid communityId = await dbContext.CommunityRoles
            .Where(x => x.Id == roleId)
            .Select(x => x.CommunityId)
            .FirstOrDefaultAsync();

        if (communityId == Guid.Empty) {
            return false;
        }

        int numUpdatedRows = await dbContext.CommunityRoles
            .Where(x => x.Id == roleId)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(x => x.ChannelPermissions, permissions.Channel);
                builder.SetProperty(x => x.RolePermissions, permissions.Role);
                builder.SetProperty(x => x.AccessPermissions, permissions.Access);
                builder.SetProperty(x => x.ManagementPermissions, permissions.Management);
            });

        if (numUpdatedRows == 0) {
            return false;
        }
        
        await eventDispatcher.Dispatch(new CommunityRolePermissionUpdatedEventArg(communityId, roleId));
        
        return true;
    }
}