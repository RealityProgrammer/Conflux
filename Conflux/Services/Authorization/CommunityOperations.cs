using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Conflux.Services.Authorization;

public static class CommunityOperations {
    public static OperationAuthorizationRequirement CreateChannelCategory = new() {
        Name = nameof(CreateChannelCategory)
    };
}