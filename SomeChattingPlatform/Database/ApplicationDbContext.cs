using Microsoft.EntityFrameworkCore;
using SomeChattingPlatform.Database.Models;

namespace SomeChattingPlatform.Database;

public sealed class ApplicationDbContext : DbContext {
    public DbSet<User> Users { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
    }
}