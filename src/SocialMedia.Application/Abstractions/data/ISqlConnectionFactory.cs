using System.Data;

namespace SocialMedia.Application.Abstractions.data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
