using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using RolePermissions = Conflux.Application.Dto.RolePermissions;

namespace Conflux.Web.Authorization;

public class BanCommunityMemberAuthorizationHandler : AuthorizationHandler<BanCommunityMemberRequirement, RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BanCommunityMemberRequirement requirement,
        RolePermissions permissions)
    {
        if (permissions.Management.HasFlag(ManagementPermissions.BanMember))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}