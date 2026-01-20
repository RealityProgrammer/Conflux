using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Services.Authorization;

public class CreateCommunityChannelCategoryAuthorizationHandler : AuthorizationHandler<CreateCommunityChannelCategoryRequirement, RolePermissions> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        CreateCommunityChannelCategoryRequirement requirement, 
        RolePermissions permissions)
    {
        if (permissions.Channel.HasFlag(ChannelPermissionFlags.CreateChannelCategory)) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}