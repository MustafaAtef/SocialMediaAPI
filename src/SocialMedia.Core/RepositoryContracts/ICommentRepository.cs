using System;
using SocialMedia.Core.Entities;

namespace SocialMedia.Core.RepositoryContracts;

public interface ICommentRepository : IRepository<Comment>
{
    Task<List<Comment>> GetAllAsync(int page, int pageSize, int repliesSize, int postId);
    public Task<Comment?> GetParentCommentWithReplies(int parentCommentId, int page, int pageSize);
}
