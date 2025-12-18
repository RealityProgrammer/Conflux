using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Conflux.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace Conflux.Services;

public class CommunityService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IContentService contentService,
    IHubContext<CommunityHub> hubContext,
    NavigationManager navigationManager,
    IHttpContextAccessor httpContextAccessor,
    ILogger<CommunityService> logger
) : ICommunityService, IAsyncDisposable {
    private const string ChannelCategoryCreatedEventName = "ChannelCategoryCreated";
    private const string ChannelCreatedEventName = "ChannelCreated";
    
    private readonly ConcurrentDictionary<Guid, HubConnection> _hubConnections = [];

    public event Action<ChannelCategoryCreatedEventArgs>? OnChannelCategoryCreated;
    public event Action<ChannelCreatedEventArgs>? OnChannelCreated;
    
    public async Task JoinCommunityHubAsync(Guid communityId) {
        // TODO: Revise this code due to possible race-condition.
        if (_hubConnections.ContainsKey(communityId)) return;
        
        var connection = CreateHubConnection(communityId);

        connection.On<ChannelCategoryCreatedEventArgs>(ChannelCategoryCreatedEventName, args => {
            OnChannelCategoryCreated?.Invoke(args);
        });

        connection.On<ChannelCreatedEventArgs>(ChannelCreatedEventName, args => {
            OnChannelCreated?.Invoke(args);
        });
        
        await connection.StartAsync();
        
        bool add = _hubConnections.TryAdd(communityId, connection);
        Debug.Assert(add, $"Failed to register hub connection for community {communityId}. Possible race-condition?");
    }

    public async Task LeaveCommunityHubAsync(Guid communityId) {
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

                options.HttpMessageHandlerFactory = (input) => {
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
    
    public async Task<bool> CreateServerAsync(string name, Stream? avatarStream, string creatorId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        Community community = new() {
            Name = name,
            CreatorId = creatorId,
            OwnerId = creatorId,
            AvatarPath = null,
        };

        dbContext.Communities.Add(community);

        if (await dbContext.SaveChangesAsync() > 0) {
            dbContext.CommunityMembers.Add(new() {
                Community = community,
                UserId = creatorId,
            });
            
            await dbContext.SaveChangesAsync();
            
            await transaction.CommitAsync();

            if (avatarStream != null) {
                await contentService.UploadServerAvatarAsync(avatarStream, community.Id);
            }
            
            return true;
        }

        return false;
    }

    public async Task CreateChannelCategoryAsync(string name, Guid communityId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        CommunityChannelCategory category = new() {
            Name = name,
            CommunityId = communityId,
        };
        
        dbContext.CommunityChannelCategories.Add(category);

        if (await dbContext.SaveChangesAsync() > 0) {
            await hubContext.Clients.Group(communityId.ToString()).SendAsync(ChannelCategoryCreatedEventName, new ChannelCategoryCreatedEventArgs(communityId, category.Id));
        }
    }

    public async Task CreateChannelAsync(string name, CommunityChannelType type, Guid channelCategoryId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        
        Guid communityId = await dbContext.CommunityChannelCategories
            .Where(x => x.Id == channelCategoryId)
            .Select(x => x.CommunityId)
            .FirstOrDefaultAsync();
        
        if (communityId == Guid.Empty) return;
        
        CommunityChannel channel = new() {
            Name = name,
            Type = type,
            ChannelCategoryId = channelCategoryId,
        };
        
        dbContext.CommunityChannels.Add(channel);
        
        if (await dbContext.SaveChangesAsync() > 0) {
            await hubContext.Clients.Group(communityId.ToString()).SendAsync(ChannelCreatedEventName, new ChannelCreatedEventArgs(channelCategoryId, channel.Id));
        }
    }

    public async ValueTask DisposeAsync() {
        foreach ((_, var connection) in _hubConnections) {
            await connection.DisposeAsync();
        }

        _hubConnections.Clear();
    }
}