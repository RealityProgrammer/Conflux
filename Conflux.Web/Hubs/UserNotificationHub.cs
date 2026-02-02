using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Conflux.Web.Hubs;

[Authorize]
public sealed class UserNotificationHub : Hub;