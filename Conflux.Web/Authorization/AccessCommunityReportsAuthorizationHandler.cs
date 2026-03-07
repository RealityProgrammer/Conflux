using Conflux.Application.Dto;
using Conflux.Domain.Enums;

using Microsoft.AspNetCore.Authorization;

using RolePermissions = Conflux.Application.Dto.RolePermissions;

namespace Conflux.Web.Authorization;

public sealed class AccessCommunityReportsAuthorizationHandler : AuthorizationHandler<AccessCommunityReportRequirement, RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccessCommunityReportRequirement requirement,
        RolePermissions permissions
    )
    {
        if (permissions.Access.HasFlag(AccessPermissions.AccessReports))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}