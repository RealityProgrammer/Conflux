using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using RolePermissions = Conflux.Application.Dto.RolePermissions;

namespace Conflux.Web.Authorization;

public class ManageCommunityReportsAuthorizationHandler : AuthorizationHandler<ManageCommunityReportsRequirement, RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageCommunityReportsRequirement requirement,
        RolePermissions permissions)
    {
        if (permissions.Management.HasFlag(ManagementPermissions.ManageReports))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}