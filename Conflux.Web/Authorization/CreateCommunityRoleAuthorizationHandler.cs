using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

using RolePermissions = Conflux.Domain.Enums.RolePermissions;

namespace Conflux.Web.Authorization;

public class CreateCommunityRoleAuthorizationHandler : AuthorizationHandler<CreateCommunityRoleRequirement, Application.Dto.RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateCommunityRoleRequirement requirement,
        Application.Dto.RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(RolePermissions.CreateRole))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}