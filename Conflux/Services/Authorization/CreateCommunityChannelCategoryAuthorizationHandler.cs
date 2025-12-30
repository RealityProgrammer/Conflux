using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class CreateCommunityChannelCategoryAuthorizationHandler : AuthorizationHandler<CreateCommunityChannelCategoryRequirement, ICommunityService.Permissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CreateCommunityChannelCategoryRequirement requirement, 
        ICommunityService.Permissions permissions)
    {
        if (permissions.ChannelPermissions.HasFlag(CommunityRole.ChannelPermissionFlags.CreateChannelCategory)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}