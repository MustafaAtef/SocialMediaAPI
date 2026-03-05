using System;
using SocialMedia.Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Core.Entities;

namespace SocialMedia.Infrastructure.Repositories;

public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(DbContext dbContext) : base(dbContext)
    {
    }
}
