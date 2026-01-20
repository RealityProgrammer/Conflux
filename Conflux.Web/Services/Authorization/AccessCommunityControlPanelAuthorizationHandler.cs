using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Services.Authorization;

public sealed class AccessCommunityControlPanelAuthorizationHandler : AuthorizationHandler<AccessCommunityControlPanelRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        AccessCommunityControlPanelRequirement requirement, 
        RolePermissions permissions
    ) {
        if (permissions.Access.HasFlag(AccessPermissionFlags.AccessControlPanel)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}