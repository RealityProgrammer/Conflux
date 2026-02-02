using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Authorization;

public class UpdateCommunityRolePermissionsAuthorizationHandler : AuthorizationHandler<UpdateCommunityRolePermissionsRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        UpdateCommunityRolePermissionsRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(RolePermissionFlags.ModifyRolePermissions)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}