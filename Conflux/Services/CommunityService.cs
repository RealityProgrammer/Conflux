using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Conflux.Services;

public class CommunityService : ICommunityService {
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IContentService _contentService;
    
    public CommunityService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IContentService contentService) {
        _dbContextFactory = dbContextFactory;
        _contentService = contentService;
    }
    
    public async Task<bool> CreateServerAsync(string name, Stream? avatarStream, string creatorId) {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

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

            if (avatarStream != null) {
                await _contentService.UploadServerAvatarAsync(avatarStream, community.Id);
            }
            
            return true;
        }

        return false;
    }

    public async Task CreateChannelCategoryAsync(string name, Guid serverId) {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        CommunityChannelCategory category = new() {
            Name = name,
            CommunityServerId = serverId,
        };
        
        dbContext.CommunityChannelCategories.Add(category);

        await dbContext.SaveChangesAsync();
    }
}