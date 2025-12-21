using Conflux.Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;

namespace Conflux.Services.Authorization;

public class CommunityAuthorizationCrudHandler : AuthorizationHandler<OperationAuthorizationRequirement, Community> {
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Community resource) {
        if (context.User.FindFirstValue(ClaimTypes.NameIdentifier) == resource.OwnerId &&
            requirement.Name == CommunityOperations.CreateChannelCategory.Name)
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}