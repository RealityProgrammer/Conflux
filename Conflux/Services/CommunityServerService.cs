using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Conflux.Services;

public class CommunityServerService : ICommunityServerService {
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IContentService _contentService;
    
    public CommunityServerService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IContentService contentService) {
        _dbContextFactory = dbContextFactory;
        _contentService = contentService;
    }
    
    public async Task<bool> CreateServerAsync(string name, Stream? avatarStream, string creatorId) {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        CommunityServer communityServer = new() {
            Name = name,
            CreatorId = creatorId,
            OwnerId = creatorId,
            AvatarPath = null,
        };

        dbContext.CommunityServers.Add(communityServer);

        if (await dbContext.SaveChangesAsync() > 0) {
            dbContext.CommunityMembers.Add(new() {
                CommunityServer = communityServer,
                UserId = creatorId,
            });
            
            await dbContext.SaveChangesAsync();

            if (avatarStream != null) {
                await _contentService.UploadServerAvatarAsync(avatarStream, communityServer.Id);
            }
            
            return true;
        }

        return false;
    }
}