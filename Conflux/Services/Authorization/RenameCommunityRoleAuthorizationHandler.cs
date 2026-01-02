using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class RenameCommunityRoleAuthorizationHandler : AuthorizationHandler<RenameCommunityRoleRequirement, ICommunityService.Permissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        RenameCommunityRoleRequirement requirement, 
        ICommunityService.Permissions permissions)
    {
        if (permissions.RolePermissions.HasFlag(CommunityRole.RolePermissionFlags.RenameRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}