using SocialMedia.Core.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialMedia.Infrastructure.Outbox;
using Newtonsoft.Json;

namespace SocialMedia.Infrastructure.Database;

public class AppDbContext : DbContext
{
    private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

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
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        modelBuilder.Entity<Avatar>(entity =>
        {
            entity.Property(a => a.StorageProvider).HasConversion<string>();
        });

        modelBuilder.Entity<PostAttachment>(entity =>
        {
            entity.Property(pa => pa.StorageProvider).HasConversion<string>();
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

        modelBuilder.Entity<UserConnection>(entity =>
        {
            entity.HasKey(p => new { p.UserId, p.ConnectionId });
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne(m => m.FromUser).WithMany(u => u.SentMessages).HasForeignKey(m => m.FromId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");

        });
        modelBuilder.Entity<MessageStatus>(entity =>
        {
            entity.HasKey(e => new { e.MessageId, e.RecieverId });
            entity.HasOne(e => e.Message).WithMany(m => m.MessageStatuses).HasForeignKey(e => e.MessageId);
            entity.HasOne(e => e.Reciever).WithMany(u => u.MessageStatuses).HasForeignKey(e => e.RecieverId);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(om => om.Id);
            entity.Property(om => om.Type).IsRequired();
            entity.Property(om => om.Payload).IsRequired().HasColumnType("nvarchar(max)");
            entity.HasIndex(om => new { om.ProcessedOn, om.OccurredOn }).HasFilter("[ProcessedOn] IS NULL");
        });

        modelBuilder.Entity<EmailOutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.To).IsRequired().HasMaxLength(320);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            entity.Property(e => e.HtmlBody).IsRequired().HasColumnType("nvarchar(max)");
            entity.HasIndex(e => new { e.ProcessedOn, e.CreatedAt }).HasFilter("[ProcessedOn] IS NULL");
        });
    }
    public DbSet<EmailOutboxMessage> EmailOutboxMessages { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<FollowerFollowing> FollowerFollowing { get; set; } = null!;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // If we're already inside an ambient transaction (e.g. unit-of-work), just piggy-back on it.
        if (Database.CurrentTransaction is not null)
        {
            int r = await base.SaveChangesAsync(cancellationToken);
            _addDomainEventsAsOutboxMessages();
            await base.SaveChangesAsync(cancellationToken);
            return r;
        }

        // Otherwise open our own transaction so both the entity rows and the
        // outbox messages land atomically in the database.
        await using var tx = await Database.BeginTransactionAsync(cancellationToken);

        // 1. Persist entities — EF now back-fills database-generated IDs onto the objects.
        int result = await base.SaveChangesAsync(cancellationToken);

        // 2. Collect domain events; IDs are real values at this point.
        _addDomainEventsAsOutboxMessages();

        // 3. Persist outbox messages within the same transaction.
        await base.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
        return result;
    }

    private void _addDomainEventsAsOutboxMessages()
    {
        var outboxMessages = ChangeTracker.Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.GetDomainEvents();
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = domainEvent.GetType().Name!,
                Payload = JsonConvert.SerializeObject(domainEvent, jsonSerializerSettings),
                OccurredOn = DateTime.UtcNow
            }).ToList();

        AddRange(outboxMessages);
    }
}
