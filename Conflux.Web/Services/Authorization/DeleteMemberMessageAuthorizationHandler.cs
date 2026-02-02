using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Services.Authorization;

public class DeleteMemberMessageAuthorizationHandler : AuthorizationHandler<DeleteMemberMessageRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        DeleteMemberMessageRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Management.HasFlag(ManagementPermissionFlags.DeleteMemberMessage)) {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}