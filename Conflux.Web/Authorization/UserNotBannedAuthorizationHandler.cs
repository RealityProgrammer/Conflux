using Conflux.Application.Dto;
using Microsoft.AspNetCore.Authorization;

namespace Conflux.Web.Authorization;

public class UserNotBannedAuthorizationHandler : AuthorizationHandler<UserNotBannedRequirement, UserBanState> {
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        UserNotBannedRequirement requirement, 
        UserBanState state)
    {
        if (DateTime.UtcNow >= state.ExpiresAt) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}