using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Conflux.Web.Services.Hubs;

[Authorize]
public sealed class UserNotificationHub : Hub;