using System;
using EducationCenter.Core.RepositoryContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;

namespace SocialMedia.Infrastructure.Repositories;

public class PostReactRepository : Repository<PostReact>, IPostReactRepository
{
    public PostReactRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }
}
