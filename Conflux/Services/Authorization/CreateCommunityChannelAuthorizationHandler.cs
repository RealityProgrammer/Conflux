using Conflux.Core;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class CreateCommunityChannelAuthorizationHandler : AuthorizationHandler<CreateCommunityChannelRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CreateCommunityChannelRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Channel.HasFlag(CommunityRole.ChannelPermissionFlags.CreateChannel)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}