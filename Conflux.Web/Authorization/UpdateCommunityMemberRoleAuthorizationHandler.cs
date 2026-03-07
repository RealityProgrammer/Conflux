using Conflux.Application.Dto;
using Conflux.Domain.Enums;

using Microsoft.AspNetCore.Authorization;

using RolePermissions = Conflux.Domain.Enums.RolePermissions;

namespace Conflux.Web.Authorization;

public class UpdateCommunityMemberRoleAuthorizationHandler : AuthorizationHandler<UpdateCommunityMemberRoleRequirement, Application.Dto.RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateCommunityMemberRoleRequirement requirement,
        Application.Dto.RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(RolePermissions.ModifyMemberRole))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}