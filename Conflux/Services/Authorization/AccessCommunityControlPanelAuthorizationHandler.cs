using Conflux.Core;
using Conflux.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public sealed class AccessCommunityControlPanelAuthorizationHandler : AuthorizationHandler<AccessCommunityControlPanelRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        AccessCommunityControlPanelRequirement requirement, 
        RolePermissions permissions
    ) {
        if (permissions.Access.HasFlag(CommunityRole.AccessPermissionFlags.AccessControlPanel)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}