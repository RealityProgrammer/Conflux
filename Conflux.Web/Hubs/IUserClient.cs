using Conflux.Domain.Enums;
using Conflux.Domain.Events;
using Conflux.Web.Services.Abstracts;

namespace Conflux.Web.Hubs;

public interface IUserClient {
    Task SystemWarned(SystemWarnedEventArgs args);
    Task SystemBanned(SystemBannedEventArgs args);
    Task CommunityWarned(CommunityWarnedEventArgs args);
    Task CommunityBanned(CommunityBannedEventArgs args);
    Task IncomingDirectMessage(IncomingDirectMessageEventArgs args);
    Task IncomingCall(IncomingCallEventArgs args);
    Task DirectConversationCreated(DirectConversationCreatedEventArgs args);
}