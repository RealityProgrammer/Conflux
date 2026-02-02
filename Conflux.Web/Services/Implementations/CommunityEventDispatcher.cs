using Conflux.Application.Abstracts;
using Conflux.Domain.Events;
using Conflux.Web.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace Conflux.Web.Services.Implementations;

public sealed class CommunityEventDispatcher(
    IHubContext<CommunityHub, ICommunityClient> hubContext, 
    IHttpContextAccessor httpContextAccessor, 
    NavigationManager navigationManager
) : ICommunityEventDispatcher, IAsyncDisposable {
    private readonly ConcurrentDictionary<Guid, HubConnection> _hubConnections = [];

    public event Action<ChannelCategoryCreatedEventArgs>? OnChannelCategoryCreated;
    public event Action<ChannelCreatedEventArgs>? OnChannelCreated;
    public event Action<CommunityMemberJoinedEventArgs>? OnMemberJoined;
    public event Action<CommunityRoleCreatedEventArgs>? OnRoleCreated;
    public event Action<CommunityRoleRenamedEventArgs>? OnRoleRenamed;
    public event Action<CommunityRoleDeletedEventArgs>? OnRoleDeleted;
    public event Action<CommunityRolePermissionUpdatedEventArg>? OnRolePermissionUpdated;
    public event Action<MemberRoleChangedEventArgs>? OnMemberRoleChanged;
    
    public async Task Connect(Guid communityId) {
        // TODO: Revise this code due to possible race-condition.
        if (_hubConnections.ContainsKey(communityId)) return;
        
        var connection = CreateHubConnection(communityId);

        connection.On<ChannelCategoryCreatedEventArgs>(nameof(ICommunityClient.ChannelCategoryCreated), args => {
            OnChannelCategoryCreated?.Invoke(args);
        });

        connection.On<ChannelCreatedEventArgs>(nameof(ICommunityClient.ChannelCreated), args => {
            OnChannelCreated?.Invoke(args);
        });

        connection.On<CommunityMemberJoinedEventArgs>(nameof(ICommunityClient.MemberJoined), args => {
            OnMemberJoined?.Invoke(args);
        });

        connection.On<CommunityRoleCreatedEventArgs>(nameof(ICommunityClient.RoleCreated), args => {
            OnRoleCreated?.Invoke(args);
        });

        connection.On<CommunityRoleRenamedEventArgs>(nameof(ICommunityClient.RoleRenamed), args => {
            OnRoleRenamed?.Invoke(args);
        });

        connection.On<CommunityRoleDeletedEventArgs>(nameof(ICommunityClient.RoleDeleted), args => {
            OnRoleDeleted?.Invoke(args);
        });

        connection.On<CommunityRolePermissionUpdatedEventArg>(nameof(ICommunityClient.RolePermissionUpdated), args => {
            OnRolePermissionUpdated?.Invoke(args);
        });
        
        connection.On<MemberRoleChangedEventArgs>(nameof(ICommunityClient.MemberRoleChanged), args => {
            OnMemberRoleChanged?.Invoke(args);
        });
        
        await connection.StartAsync();
        
        bool add = _hubConnections.TryAdd(communityId, connection);
        Debug.Assert(add, $"Failed to register hub connection for community {communityId}. Possible race-condition?");
    }

    public async Task Disconnect(Guid communityId) {
        if (_hubConnections.TryRemove(communityId, out var connection)) {
            await connection.DisposeAsync();
        }
    }
    
    private HubConnection CreateHubConnection(Guid communityId) {
        return new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri($"/hub/community?CommunityId={communityId}"), options => {
                var cookies = httpContextAccessor.HttpContext!.Request.Cookies.ToDictionary();
                
                options.UseDefaultCredentials = true;
                
                var cookieContainer = cookies.Count != 0 ? new(cookies.Count) : new CookieContainer();

                foreach (var cookie in cookies) {
                    cookieContainer.Add(new Cookie(
                        cookie.Key,
                        WebUtility.UrlEncode(cookie.Value),
                        path: "/",
                        domain: navigationManager.ToAbsoluteUri("/").Host));
                }

                options.Cookies = cookieContainer;

                foreach (var header in cookies) {
                    options.Headers.Add(header.Key, header.Value);
                }

                options.HttpMessageHandlerFactory = _ => {
                    var clientHandler = new HttpClientHandler {
                        PreAuthenticate = true,
                        CookieContainer = cookieContainer,
                        UseCookies = true,
                        UseDefaultCredentials = true,
                    };
                    return clientHandler;
                };
            })
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task Dispatch(ChannelCategoryCreatedEventArgs args) {
        await hubContext.Clients.Group(args.CommunityId.ToString()).ChannelCategoryCreated(args);
    }

    public async Task Dispatch(ChannelCreatedEventArgs args) {
        await hubContext.Clients.Group(args.CommunityId.ToString()).ChannelCreated(args);
    }

    public async Task Dispatch(CommunityMemberJoinedEventArgs args) {
        await hubContext.Clients.Group(args.CommunityId.ToString()).MemberJoined(args);
    }

    public async Task Dispatch(CommunityRoleCreatedEventArgs args) {
        await hubContext.Clients.Group(args.CommunityId.ToString()).RoleCreated(args);
    }

    public async Task Dispatch(CommunityRoleRenamedEventArgs args) {
        await hubContext.Clients.Group(args.CommunityId.ToString()).RoleRenamed(args);
    }
    
    public async Task Dispatch(CommunityRoleDeletedEventArgs args) {
        await hubContext.Clients.Group(args.CommunityId.ToString()).RoleDeleted(args);
    }

    public async Task Dispatch(CommunityRolePermissionUpdatedEventArg args) {
        await hubContext.Clients.Group(args.CommunityId.ToString()).RolePermissionUpdated(args);
    }

    public async Task Dispatch(MemberRoleChangedEventArgs args) {
        await hubContext.Clients.Group(args.CommunityId.ToString()).MemberRoleChanged(args);
    }

    public async ValueTask DisposeAsync() {
        foreach ((_, var connection) in _hubConnections) {
            await connection.DisposeAsync();
        }

        _hubConnections.Clear();
    }
}