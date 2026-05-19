using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

using RolePermissions = Conflux.Domain.Enums.RolePermissions;

namespace Conflux.Web.Authorization;

public class UpdateCommunityRolePermissionsAuthorizationHandler : AuthorizationHandler<UpdateCommunityRolePermissionsRequirement, Application.Dto.RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateCommunityRolePermissionsRequirement requirement,
        Application.Dto.RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(RolePermissions.ModifyRolePermissions))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}