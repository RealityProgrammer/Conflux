using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Services.Authorization;

public class RenameCommunityRoleAuthorizationHandler : AuthorizationHandler<RenameCommunityRoleRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        RenameCommunityRoleRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(RolePermissionFlags.RenameRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}