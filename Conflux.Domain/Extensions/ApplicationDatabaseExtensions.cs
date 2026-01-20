using Conflux.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conflux.Domain.Extensions;

public static class ApplicationDatabaseExtensions {
    private static readonly IReadOnlyCollection<string> _includeBannedFilterNames = ["BanFilter"];
    
    public static IQueryable<CommunityMember> IncludeBanned(this DbSet<CommunityMember> set) {
        return set.IgnoreQueryFilters(_includeBannedFilterNames);
    }
}