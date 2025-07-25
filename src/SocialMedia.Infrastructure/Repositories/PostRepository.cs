using System;
using EducationCenter.Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;

namespace SocialMedia.Infrastructure.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }

    private AppDbContext _appDbContext => (AppDbContext)_dbContext;

    public async Task<ICollection<Post>> GetAllDeletedPostsAsync(int userId, int page, int pageSize)
    {
        var posts = await _appDbContext.Set<Post>().IgnoreQueryFilters().Where(p => p.IsDeleted).Include(p => p.Attachments).OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return posts;
    }
    public async Task<Post?> GetDeletedPostAsync(int postId)
    {
        return await _appDbContext.Set<Post>().IgnoreQueryFilters().Where(p => p.Id == postId && p.IsDeleted).Include(p => p.Attachments).FirstAsync();

    }

    public async Task<Post?> GetAsync(int postId)
    {
        return await _appDbContext.Set<Post>().IgnoreQueryFilters().Where(p => p.Id == postId).FirstAsync();
    }

    public async Task<bool> PermanentDeleteAsync(int postId)
    {
        var posts =
        await _appDbContext.Set<Post>()
        .Include(p => p.Attachments)
        .Include(p => p.Reactions)
        .Where(p => p.Id == postId)
        .AsSplitQuery()
        .IgnoreQueryFilters()
        .ToListAsync();

        var comments =
        await _appDbContext.Set<Comment>()
        .Include(c => c.Reactions)
        .Include(c => c.Replies)
        .Where(c => c.PostId == postId)
        .AsSplitQuery()
        .IgnoreQueryFilters()
        .ToListAsync();

        if (posts.Count == 0) return false;
        var post = posts.First();
        if (post.Reactions is not null)
            _appDbContext.Set<PostReact>().RemoveRange(post.Reactions);

        if (comments is not null)
        {
            foreach (var comment in comments)
            {
                if (comment.ParentCommentId is not null) continue;
                if (comment.Reactions is not null) _appDbContext.Set<CommentReact>().RemoveRange(comment.Reactions);
                if (comment.Replies is not null)
                {
                    foreach (var reply in comment.Replies)
                    {
                        if (reply.Reactions is not null) _appDbContext.Set<CommentReact>().RemoveRange(reply.Reactions);
                    }
                    _appDbContext.Set<Comment>().RemoveRange(comment.Replies);
                }
                _appDbContext.Set<Comment>().Remove(comment);
            }
        }
        _appDbContext.Set<Post>().Remove(post);
        var entitiesDeleted = await _appDbContext.SaveChangesAsync();
        return entitiesDeleted > 0;
    }
}
