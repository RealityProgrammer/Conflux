using Conflux.Database.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Conflux.Services.Authorization;

public class CommunityOwnershipAuthorizationHandler : AuthorizationHandler<CommunityOwnershipRequirement, Community> {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CommunityOwnershipRequirement requirement, Community resource) {
        if (context.User.FindFirstValue(ClaimTypes.NameIdentifier) == resource.OwnerId) {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}