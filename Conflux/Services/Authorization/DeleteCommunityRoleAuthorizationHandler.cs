using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class DeleteCommunityRoleAuthorizationHandler : AuthorizationHandler<DeleteCommunityRoleRequirement, ICommunityService.Permissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        DeleteCommunityRoleRequirement requirement, 
        ICommunityService.Permissions permissions)
    {
        if (permissions.RolePermissions.HasFlag(CommunityRole.RolePermissionFlags.DeleteRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}