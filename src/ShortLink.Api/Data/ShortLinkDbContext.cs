using Microsoft.EntityFrameworkCore;
using ShortLink.Api.Models;

namespace ShortLink.Api.Data;

public sealed class ShortLinkDbContext(DbContextOptions<ShortLinkDbContext> options) : DbContext(options)
{
    public DbSet<Link> Links => Set<Link>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var link = modelBuilder.Entity<Link>();
        link.HasKey(item => item.Id);
        link.Property(item => item.Code).HasMaxLength(7).IsRequired();
        link.Property(item => item.LongUrl).HasMaxLength(2048).IsRequired();
        link.Property(item => item.CreatedAt).IsRequired();
        link.HasIndex(item => item.Code).IsUnique();
    }
}
