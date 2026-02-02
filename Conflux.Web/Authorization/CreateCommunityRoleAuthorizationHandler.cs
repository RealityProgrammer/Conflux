using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Authorization;

public class CreateCommunityRoleAuthorizationHandler : AuthorizationHandler<CreateCommunityRoleRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CreateCommunityRoleRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(RolePermissionFlags.CreateRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}