using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Conflux.Services.Hubs;

[Authorize]
public sealed class FriendshipHub : Hub;