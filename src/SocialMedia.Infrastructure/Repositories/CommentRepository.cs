using System;
using EducationCenter.Core.RepositoryContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;

namespace SocialMedia.Infrastructure.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }
}
