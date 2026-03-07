using Conflux.Application.Dto;
using Conflux.Domain.Enums;

using Microsoft.AspNetCore.Authorization;

using RolePermissions = Conflux.Domain.Enums.RolePermissions;

namespace Conflux.Web.Authorization;

public class DeleteCommunityRoleAuthorizationHandler : AuthorizationHandler<DeleteCommunityRoleRequirement, Application.Dto.RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DeleteCommunityRoleRequirement requirement,
        Application.Dto.RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(RolePermissions.DeleteRole))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}