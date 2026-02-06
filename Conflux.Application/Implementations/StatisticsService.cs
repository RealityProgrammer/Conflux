// ReSharper disable AccessToDisposedClosure

using Conflux.Application.Abstracts;
using Conflux.Application.Dto;
using Conflux.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Application.Implementations;

public sealed class StatisticsService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory
) : IStatisticsService
{
    public async Task<UserStatisticsDTO> GetUserStatistics() {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        // Get number of users.
        int userCount = await dbContext.Users.CountAsync();
        
        return new(userCount, 0, 0);
    }
}