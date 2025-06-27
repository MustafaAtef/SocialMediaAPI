using SocialMedia.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SocialMedia.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {

    }

    public AppDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(u => u.UpdatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<FollowerFollowing>(entity =>
        {
            entity.HasKey(ff => new { ff.FollowerId, ff.FollowingId });

            entity.HasOne(ff => ff.Follower)
                .WithMany(u => u.Followings)
                .HasForeignKey(ff => ff.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ff => ff.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(ff => ff.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(ff => ff.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
        });
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(c => c.UpdatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<PostReact>(entity =>
        {
            entity.Property(pr => pr.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<CommentReact>(entity =>
        {
            entity.Property(cr => cr.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

    }
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<FollowerFollowing> FollowerFollowing { get; set; } = null!;
}

public static class AppDbContextExtensions
{

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddDbContext<AppDbContext>(builder =>
        {
            builder.UseSqlServer(configuration.GetConnectionString("sqlserverConnectionString"));
        });
    }
}
