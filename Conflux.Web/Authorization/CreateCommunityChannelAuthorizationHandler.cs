using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Authorization;

public class CreateCommunityChannelAuthorizationHandler : AuthorizationHandler<CreateCommunityChannelRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CreateCommunityChannelRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Channel.HasFlag(ChannelPermissionFlags.CreateChannel)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}