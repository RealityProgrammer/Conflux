using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using RolePermissions = Conflux.Application.Dto.RolePermissions;

namespace Conflux.Web.Authorization;

public class DeleteMemberMessageAuthorizationHandler : AuthorizationHandler<DeleteMemberMessageRequirement, RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DeleteMemberMessageRequirement requirement,
        RolePermissions permissions)
    {
        if (permissions.Management.HasFlag(ManagementPermissions.DeleteMemberMessage))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}