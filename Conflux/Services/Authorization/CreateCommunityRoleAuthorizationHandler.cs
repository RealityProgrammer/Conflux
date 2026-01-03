using Conflux.Core;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class CreateCommunityRoleAuthorizationHandler : AuthorizationHandler<CreateCommunityRoleRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CreateCommunityRoleRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(CommunityRole.RolePermissionFlags.CreateRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}