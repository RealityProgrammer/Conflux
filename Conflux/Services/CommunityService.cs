using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Conflux.Services.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
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
    IMemoryCache memoryCache,
    ILogger<CommunityService> logger
) : ICommunityService, IAsyncDisposable {
    private const string ChannelCategoryCreatedEventName = "ChannelCategoryCreated";
    private const string ChannelCreatedEventName = "ChannelCreated";
    private const string MemberJoinedEventName = "MemberJoined";
    private const string RoleCreatedEventName = "RoleCreated";
    private const string MemberRoleChangedEventName = "MemberRoleChanged";
    
    private static readonly TimeSpan PermissionCacheDuration = TimeSpan.FromMinutes(5);
    
    private readonly ConcurrentDictionary<Guid, HubConnection> _hubConnections = [];

    public event Action<ChannelCategoryCreatedEventArgs>? OnChannelCategoryCreated;
    public event Action<ChannelCreatedEventArgs>? OnChannelCreated;
    public event Action<CommunityCreatedEventArgs>? OnUserCreatedCommunity;
    public event Action<CommunityMemberJoinedEventArgs>? OnMemberJoined;
    public event Action<CommunityRoleCreatedEventArgs>? OnRoleCreated;
    public event Action<MemberRoleChangedEventArgs>? OnMemberRoleChanged;
    
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

        connection.On<CommunityMemberJoinedEventArgs>(MemberJoinedEventName, args => {
            OnMemberJoined?.Invoke(args);
        });

        connection.On<CommunityRoleCreatedEventArgs>(RoleCreatedEventName, args => {
            OnRoleCreated?.Invoke(args);
        });

        connection.On<MemberRoleChangedEventArgs>(MemberRoleChangedEventName, args => {
            OnMemberRoleChanged?.Invoke(args);
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
    
    public async Task<bool> CreateCommunityAsync(string name, Stream? avatarStream, string creatorId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        Community community = new() {
            Name = name,
            CreatorId = creatorId,
            AvatarPath = null,
        };

        dbContext.Communities.Add(community);

        CommunityRole ownerRole = new() {
            Name = "Owners",
            AccessPermissions = CommunityRole.AccessPermissionFlags.All,
            ChannelPermissions = CommunityRole.ChannelPermissionFlags.All,
            RolePermissions = CommunityRole.RolePermissionFlags.All,
            CommunityId = community.Id,
        };

        dbContext.CommunityRoles.Add(ownerRole);

        if (await dbContext.SaveChangesAsync() > 0) {
            dbContext.CommunityMembers.Add(new() {
                Community = community,
                UserId = creatorId,
                RoleId = ownerRole.Id,
            });
            
            if (avatarStream != null) {
                string path = await contentService.UploadCommunityAvatarAsync(avatarStream, community.Id);
                
                community.AvatarPath = path;
            }
            
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
            OnUserCreatedCommunity?.Invoke(new(community));
            
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

        if (communityId == Guid.Empty) {
            return;
        }
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        
        CommunityChannel channel = new() {
            Name = name,
            Type = type,
            ChannelCategoryId = channelCategoryId,
        };
        
        dbContext.CommunityChannels.Add(channel);

        Conversation conversation = new() {
            CommunityChannelId = channel.Id,
        };

        dbContext.Conversations.Add(conversation);
        
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        
        await hubContext.Clients.Group(communityId.ToString()).SendAsync(ChannelCreatedEventName, new ChannelCreatedEventArgs(channelCategoryId, channel.Id));
    }

    public async Task<ICommunityService.CreateRoleStatus> CreateRoleAsync(Guid communityId, string roleName) {
        if (roleName == "Owners") {
            return ICommunityService.CreateRoleStatus.ReservedName;
        }
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        if (dbContext.CommunityRoles.Any(x => x.CommunityId == communityId && x.Name == roleName)) {
            return ICommunityService.CreateRoleStatus.NameExists;
        }
        
        CommunityRole role = new() {
            CommunityId = communityId,
            Name = roleName,
        };
        
        dbContext.CommunityRoles.Add(role);

        if (await dbContext.SaveChangesAsync() > 0) {
            await hubContext.Clients.Group(communityId.ToString()).SendAsync(RoleCreatedEventName, new CommunityRoleCreatedEventArgs(role));
            
            return ICommunityService.CreateRoleStatus.Success;
        }
        
        return ICommunityService.CreateRoleStatus.Failure;
    }

    public async Task<bool> JoinCommunityAsync(string userId, Guid communityId, Guid invitationId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        
        // Early return if user already in the community.
        if (await dbContext.CommunityMembers.Where(x => x.UserId == userId && x.CommunityId == communityId).AnyAsync()) {
            return false;
        }
        
        // Determine if the invitation id is valid.
        if (!await dbContext.Communities.Where(x => x.Id == communityId && x.InvitationId == invitationId).AnyAsync()) {
            return false;
        }

        CommunityMember member = new() {
            UserId = userId,
            CommunityId = communityId,
        };

        dbContext.CommunityMembers.Add(member);

        if (await dbContext.SaveChangesAsync() > 0) {
            await hubContext.Clients.Group(communityId.ToString()).SendAsync(MemberJoinedEventName, new CommunityMemberJoinedEventArgs(member));

            return true;
        }

        return false;
    }
    
    public async Task<ICommunityService.Permissions?> GetPermissionsAsync(Guid roleId) {
        string cacheKey = GeneratePermissionCacheKey(roleId);
        
        if (memoryCache.TryGetValue<ICommunityService.Permissions>(cacheKey, out var cached)) {
            return cached!;
        }
        
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var permissions = await CollectPermissions(dbContext, roleId);

        if (permissions == null) {
            return null;
        }
        
        memoryCache.Set(cacheKey, permissions, PermissionCacheDuration);

        return permissions;
    }

    public async Task<ICommunityService.Permissions?> GetUserRolePermissionsAsync(string userId, Guid communityId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        Guid? memberRoleId = await dbContext.CommunityMembers
            .Where(x => x.UserId == userId && x.CommunityId == communityId)
            .Select(x => x.RoleId)
            .FirstOrDefaultAsync();

        if (memberRoleId is not { } roleId) {
            return null;
        }
        
        string cacheKey = GeneratePermissionCacheKey(roleId);
        
        if (memoryCache.TryGetValue<ICommunityService.Permissions>(cacheKey, out var cached)) {
            return cached!;
        }
        
        var permissions = await CollectPermissions(dbContext, roleId);

        if (permissions == null) {
            return null;
        }
        
        memoryCache.Set(cacheKey, permissions, PermissionCacheDuration);

        return permissions;
    }

    public async Task<bool> UpdatePermissionsAsync(Guid roleId, ICommunityService.Permissions permissions) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        int numUpdatedRows = await dbContext.CommunityRoles
            .Where(x => x.Id == roleId)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(x => x.ChannelPermissions, permissions.ChannelPermissions);
                builder.SetProperty(x => x.RolePermissions, permissions.RolePermissions);
                builder.SetProperty(x => x.AccessPermissions, permissions.AccessPermissions);
            });

        if (numUpdatedRows == 0) {
            return false;
        }
        
        string cacheKey = GeneratePermissionCacheKey(roleId);
        
        memoryCache.Set(cacheKey, permissions, PermissionCacheDuration);

        return true;
    }

    private async Task<ICommunityService.Permissions?> CollectPermissions(ApplicationDbContext dbContext, Guid roleId) {
        return await dbContext.CommunityRoles
            .Where(x => x.Id == roleId)
            .Select(x => new ICommunityService.Permissions(x.ChannelPermissions, x.RolePermissions, x.AccessPermissions))
            .FirstOrDefaultAsync();
    }

    private static string GeneratePermissionCacheKey(Guid roleId) {
        return string.Create("CommunityRoles.".Length + 32 + ".Permissions".Length, roleId, CreateCallback);

        static void CreateCallback(Span<char> buffer, Guid state) {
            "CommunityRoles.".CopyTo(buffer);
            bool formatSuccessful = state.TryFormat(buffer.Slice("CommunityRoles.".Length, 32), out _, "N");
            Debug.Assert(formatSuccessful);
            ".Permissions".CopyTo(buffer[("CommunityRoles.".Length + 32)..]);
        }
    }

    public async Task<bool> SetMembersRole(IReadOnlyCollection<Guid> memberIds, Guid roleId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        Guid communityId = await dbContext.CommunityRoles
            .Where(x => x.Id == roleId)
            .Select(x => x.CommunityId)
            .FirstOrDefaultAsync();

        if (communityId == Guid.Empty) {
            return false;
        }

        int affected = await dbContext.CommunityMembers
            .Where(x => memberIds.Contains(x.Id) && x.RoleId == null)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(x => x.RoleId, roleId);
            });

        if (affected > 0) {
            await hubContext.Clients.Group(communityId.ToString()).SendAsync(MemberRoleChangedEventName, new MemberRoleChangedEventArgs(communityId, roleId));
        }

        return true;
    }
    
    public async Task<bool> RemoveMemberRole(Guid memberId) {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        int affected = await dbContext.CommunityMembers
            .Where(x => x.Id == memberId)
            .ExecuteUpdateAsync(builder => {
                builder.SetProperty(x => x.RoleId, (Guid?)null);
            });

        if (affected > 0) {
            var communityId = await dbContext.CommunityMembers
                .Where(x => x.Id == memberId)
                .Select(x => x.CommunityId)
                .FirstAsync();
            
            await hubContext.Clients.Group(communityId.ToString()).SendAsync(MemberRoleChangedEventName, new MemberRoleChangedEventArgs(communityId, null));
        }

        return true;
    }

    public async ValueTask DisposeAsync() {
        foreach ((_, var connection) in _hubConnections) {
            await connection.DisposeAsync();
        }

        _hubConnections.Clear();
    }
}