using System;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Infrastructure.Database;

namespace SocialMedia.Infrastructure.Repositories;

public class FollowerFollowingRepository : Repository<FollowerFollowing>, IFollowerFollowingRepository
{
    public FollowerFollowingRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }

}
