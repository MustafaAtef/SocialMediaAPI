

namespace EducationCenter.Core.RepositoryContracts;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }

    Task SaveChangesAsync();

}
