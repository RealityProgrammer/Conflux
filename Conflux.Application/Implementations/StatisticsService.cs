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

        int userCount = await dbContext.Users.CountAsync();

        return new(userCount);
    }
}