using EducationCenter.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;
using SocialMedia.Core.Entities;

namespace SocialMedia.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }

}
