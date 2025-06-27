using System;
using EducationCenter.Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;

namespace SocialMedia.Infrastructure.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }

    private AppDbContext _appDbContext => (AppDbContext)_dbContext;

    public async Task<List<Comment>> GetAllAsync(int page, int pageSize, int repliesSize, int postId)
    {
        return await _appDbContext.Comments
        .Where(c => c.PostId == postId && c.ParentComment == null)
        .Include(c => c.Replies.OrderByDescending(r => r.CreatedAt).Take(repliesSize))
        .ThenInclude(r => r.User)
        .ThenInclude(u => u.Avatar)
        .Include(c => c.User)
        .ThenInclude(u => u.Avatar)
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    }

    public async Task<Comment?> GetParentCommentWithReplies(int parentCommentId, int page, int pageSize)
    {
        return (await _appDbContext.Comments.Where(c => c.Id == parentCommentId).Include(c => c.Replies.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize)).ThenInclude(r => r.User).ThenInclude(u => u.Avatar).Take(pageSize).ToListAsync())[0];
    }
}
