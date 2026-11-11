using Microsoft.EntityFrameworkCore;
using Social.Application;
using Social.Domain.Blogs;
using Social.Domain.Clubs;
using Social.Domain.Profiles;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.Clubs;
using Social.Infrastructure.Persistence.Profiles;

namespace Social.Infrastructure.Persistence;

internal sealed class SocialDbContext : DbContext, IUnitOfWork
{
    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Profile> Profiles => Set<Profile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("social");
        builder.ConfigureBlogs();
        builder.ConfigureClubs();
        builder.ConfigureProfiles();
    }
}
