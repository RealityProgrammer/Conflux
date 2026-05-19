using Conflux.Application.Dto;
using Conflux.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

using RolePermissions = Conflux.Application.Dto.RolePermissions;

namespace Conflux.Web.Authorization;

public class CreateCommunityChannelCategoryAuthorizationHandler : AuthorizationHandler<CreateCommunityChannelCategoryRequirement, RolePermissions>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateCommunityChannelCategoryRequirement requirement,
        RolePermissions permissions)
    {
        if (permissions.Channel.HasFlag(ChannelPermissions.CreateChannelCategory))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}