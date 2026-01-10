using Conflux.Core;
using Conflux.Domain.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class UpdateCommunityRolePermissionsAuthorizationHandler : AuthorizationHandler<UpdateCommunityRolePermissionsRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        UpdateCommunityRolePermissionsRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(CommunityRole.RolePermissionFlags.ModifyRolePermissions)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}