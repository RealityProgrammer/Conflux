using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Conflux.Services.Hubs;

[Authorize]
public sealed class NotificationHub : Hub;