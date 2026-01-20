using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Services.Authorization;

public class DeleteCommunityRoleAuthorizationHandler : AuthorizationHandler<DeleteCommunityRoleRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        DeleteCommunityRoleRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(RolePermissionFlags.DeleteRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}