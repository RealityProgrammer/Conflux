using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public sealed class AccessCommunityControlPanelAuthorizationHandler : AuthorizationHandler<AccessCommunityControlPanelRequirement, ICommunityService.Permissions> {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessCommunityControlPanelRequirement requirement, ICommunityService.Permissions permissions) {
        if (permissions.AccessPermissions.HasFlag(CommunityRole.AccessPermissionFlags.AccessControlPanel)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}