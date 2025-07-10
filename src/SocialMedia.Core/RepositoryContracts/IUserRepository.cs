using System;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;

namespace EducationCenter.Core.RepositoryContracts;

public interface IUserRepository : IRepository<User>
{
    Task UpdateSentMessagesToDelivered(int userId);
}
