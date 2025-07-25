using System;
using SocialMedia.Core.Entities;

namespace SocialMedia.Core.RepositoryContracts;

public interface IPostRepository : IRepository<Post>
{
    Task<ICollection<Post>> GetAllDeletedPostsAsync(int userId, int page, int pageSize);
    Task<Post?> GetDeletedPostAsync(int postId);
    Task<Post?> GetAsync(int postId);
    Task<bool> PermanentDeleteAsync(int postId);

}
