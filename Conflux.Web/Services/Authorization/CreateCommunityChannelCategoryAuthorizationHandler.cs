using Conflux.Application.Dto;
using Conflux.Domain.Entities;
using Conflux.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Services.Authorization;

public class CreateCommunityChannelCategoryAuthorizationHandler : AuthorizationHandler<CreateCommunityChannelCategoryRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CreateCommunityChannelCategoryRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Channel.HasFlag(CommunityRole.ChannelPermissionFlags.CreateChannelCategory)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}