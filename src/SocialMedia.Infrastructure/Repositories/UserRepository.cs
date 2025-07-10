using EducationCenter.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;
using SocialMedia.Core.Entities;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }
    private AppDbContext _appDbContext => (AppDbContext)_dbContext;
    public async Task UpdateSentMessagesToDelivered(int userId)
    {
        await _appDbContext.Set<MessageStatus>().Where(ms => ms.RecieverId == userId).ExecuteUpdateAsync(ms => ms.SetProperty(p => p.Status, MessageStatusType.Delivered));
    }
}
