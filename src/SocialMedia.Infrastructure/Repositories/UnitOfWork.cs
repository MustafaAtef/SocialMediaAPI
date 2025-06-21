

using EducationCenter.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;
using SocialMedia.Infrastructure.Repositories;

namespace EducationCenterAPI.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _appDbContext;
    public UnitOfWork(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    private IUserRepository? _users = null;

    public IUserRepository Users
    {
        get
        {
            if (_users == null)
                _users = new UserRepository(_appDbContext);
            return _users;
        }
    }

    public void Dispose()
    {
        _appDbContext.Dispose();
    }

    public Task SaveChangesAsync()
    {
        return _appDbContext.SaveChangesAsync();
    }
}
