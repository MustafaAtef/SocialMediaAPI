

using SocialMedia.Core.RepositoryContracts;

namespace EducationCenter.Core.RepositoryContracts;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPostRepository Posts { get; }
    ICommentRepository Comments { get; }
    IPostReactRepository PostReacts { get; }
    ICommentReactRepository CommentReacts { get; }
    IFollowerFollowingRepository FollowersFollowings { get; }
    Task SaveChangesAsync();

}
