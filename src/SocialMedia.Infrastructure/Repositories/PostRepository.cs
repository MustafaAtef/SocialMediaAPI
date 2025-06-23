using System;
using EducationCenter.Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;

namespace SocialMedia.Infrastructure.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }
}
