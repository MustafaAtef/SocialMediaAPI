using System;
using EducationCenter.Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;

namespace SocialMedia.Infrastructure.Repositories;

public class UserConnectionRepository : Repository<UserConnection>, IUserConnectionRepository
{
    public UserConnectionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
