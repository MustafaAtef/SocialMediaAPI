using System;
using EducationCenter.Core.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;

namespace SocialMedia.Infrastructure.Repositories;

public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(DbContext dbContext) : base(dbContext)
    {
    }
}
