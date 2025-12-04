using Conflux.Database;
using Conflux.Database.Entities;
using Conflux.Services.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Services;

public sealed class ConversationService : IConversationService {
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public ConversationService(IDbContextFactory<ApplicationDbContext> dbContextFactory) {
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<Conversation?> GetOrCreateDirectConversationAsync(string user1, string user2) {
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync()) {
            string[] userIds = [user1, user2];

            Conversation? conversation = await dbContext.Conversations
                .AsNoTracking()
                .Where(c => c.Type == ConversationType.DirectMessage)
                .Where(c => c.Members.Select(m => m.UserId).Intersect(userIds).Count() == 2)
                .FirstOrDefaultAsync();

            if (conversation != null) {
                return conversation;
            }

            conversation = new() {
                Type = ConversationType.DirectMessage,
            };
            
            dbContext.Add(conversation);
            await dbContext.SaveChangesAsync();

            dbContext.ConversationMembers.AddRange(
                new() {
                    UserId = user1,
                    ConversationId = conversation.Id,
                },
                new() {
                    UserId = user2,
                    ConversationId = conversation.Id,
                }
            );
            
            await dbContext.SaveChangesAsync();

            return conversation;
        }
    }
}