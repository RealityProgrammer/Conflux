using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Services.Authorization;

public sealed class AccessCommunityReportsAuthorizationHandler : AuthorizationHandler<AccessCommunityReportRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        AccessCommunityReportRequirement requirement, 
        RolePermissions permissions
    ) {
        if (permissions.Access.HasFlag(AccessPermissionFlags.AccessReports)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}