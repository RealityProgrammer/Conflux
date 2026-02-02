using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Authorization;

public class UpdateCommunityMemberRoleAuthorizationHandler : AuthorizationHandler<UpdateCommunityMemberRoleRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        UpdateCommunityMemberRoleRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(RolePermissionFlags.ModifyMemberRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}