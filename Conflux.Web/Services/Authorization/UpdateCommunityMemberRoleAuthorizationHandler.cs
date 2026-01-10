using Conflux.Application.Dto;
using Conflux.Domain.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class UpdateCommunityMemberRoleAuthorizationHandler : AuthorizationHandler<UpdateCommunityMemberRoleRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        UpdateCommunityMemberRoleRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Role.HasFlag(CommunityRole.RolePermissionFlags.ModifyMemberRole)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}