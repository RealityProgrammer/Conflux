using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class UpdateCommunityMemberRoleAuthorizationHandler : AuthorizationHandler<UpdateCommunityMemberRoleRequirement, ICommunityService.Permissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        UpdateCommunityMemberRoleRequirement requirement, 
        ICommunityService.Permissions permissions)
    {
        if (permissions.RolePermissions.HasFlag(CommunityRole.RolePermissionFlags.ModifyMemberRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}