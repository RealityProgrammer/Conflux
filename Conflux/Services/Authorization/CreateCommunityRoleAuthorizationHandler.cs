using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class CreateCommunityRoleAuthorizationHandler : AuthorizationHandler<CreateCommunityRoleRequirement, ICommunityService.Permissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CreateCommunityRoleRequirement requirement, 
        ICommunityService.Permissions permissions)
    {
        if (permissions.RolePermissions.HasFlag(CommunityRole.RolePermissionFlags.CreateRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}