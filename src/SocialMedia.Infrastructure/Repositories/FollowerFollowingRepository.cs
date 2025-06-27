using System;
using EducationCenter.Core.RepositoryContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;

namespace SocialMedia.Infrastructure.Repositories;

public class FollowerFollowingRepository : Repository<FollowerFollowing>, IFollowerFollowingRepository
{
    public FollowerFollowingRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }

}
