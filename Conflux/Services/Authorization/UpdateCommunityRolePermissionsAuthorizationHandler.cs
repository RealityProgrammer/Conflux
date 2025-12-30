using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class UpdateCommunityRolePermissionsAuthorizationHandler : AuthorizationHandler<UpdateCommunityRolePermissionsRequirement, ICommunityService.Permissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        UpdateCommunityRolePermissionsRequirement requirement, 
        ICommunityService.Permissions permissions)
    {
        if (permissions.RolePermissions.HasFlag(CommunityRole.RolePermissionFlags.ModifyRolePermissions)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}