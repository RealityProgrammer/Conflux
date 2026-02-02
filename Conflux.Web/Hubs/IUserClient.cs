using Conflux.Domain.Enums;
using Conflux.Domain.Events;
using Conflux.Web.Services.Abstracts;

namespace Conflux.Web.Hubs;

public interface IUserClient {
    Task CommunityBanned(CommunityBannedEventArgs args);
    Task IncomingDirectMessage(IncomingDirectMessageEventArgs args);
    Task IncomingCall(IncomingCallEventArgs args);
}