using Microsoft.EntityFrameworkCore;
using TravelUp.Domain.Entities;

namespace TravelUp.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global filter for soft delete
        modelBuilder.Entity<Item>().HasQueryFilter(i => !i.IsDeleted);
    }
}

