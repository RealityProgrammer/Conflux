using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Services.Authorization;

public class BanCommunityMemberAuthorizationHandler : AuthorizationHandler<BanCommunityMemberRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        BanCommunityMemberRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Management.HasFlag(ManagementPermissionFlags.BanMember)) {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}