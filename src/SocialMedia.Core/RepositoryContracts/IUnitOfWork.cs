

using SocialMedia.Core.RepositoryContracts;

namespace EducationCenter.Core.RepositoryContracts;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPostRepository Posts { get; }
    ICommentRepository Commnets { get; }
    IPostReactRepository PostReacts { get; }
    ICommentReactRepository CommnetReacts { get; }
    Task SaveChangesAsync();

}
