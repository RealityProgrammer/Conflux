using Conflux.Application.Dto;
using Conflux.Domain.Enums;

using Microsoft.AspNetCore.Authorization;

using RolePermissions = Conflux.Domain.Enums.RolePermissions;

namespace Conflux.Web.Authorization;

public class RenameCommunityRoleAuthorizationHandler : AuthorizationHandler<RenameCommunityRoleRequirement, Application.Dto.RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RenameCommunityRoleRequirement requirement,
        Application.Dto.RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(RolePermissions.RenameRole))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}