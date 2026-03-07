using System;

using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Core.Entities;
using SocialMedia.Infrastructure.Database;

namespace SocialMedia.Infrastructure.Repositories;


public class CommentReactRepository : Repository<CommentReact>, ICommentReactRepository
{
    public CommentReactRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }
}
