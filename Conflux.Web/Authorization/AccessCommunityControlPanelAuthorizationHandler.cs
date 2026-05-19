using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

using RolePermissions = Conflux.Application.Dto.RolePermissions;

namespace Conflux.Web.Authorization;

public sealed class AccessCommunityControlPanelAuthorizationHandler : AuthorizationHandler<AccessCommunityControlPanelRequirement, RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccessCommunityControlPanelRequirement requirement,
        RolePermissions permissions
    )
    {
        if (permissions.Access.HasFlag(AccessPermissions.AccessControlPanel))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}