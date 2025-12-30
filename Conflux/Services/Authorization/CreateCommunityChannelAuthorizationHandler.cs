using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class CreateCommunityChannelAuthorizationHandler : AuthorizationHandler<CreateCommunityChannelRequirement, ICommunityService.Permissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CreateCommunityChannelRequirement requirement, 
        ICommunityService.Permissions permissions)
    {
        if (permissions.ChannelPermissions.HasFlag(CommunityRole.ChannelPermissionFlags.CreateChannel)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}