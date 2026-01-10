using Conflux.Application.Dto;
using Conflux.Domain.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class DeleteCommunityRoleAuthorizationHandler : AuthorizationHandler<DeleteCommunityRoleRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        DeleteCommunityRoleRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(CommunityRole.RolePermissionFlags.DeleteRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}