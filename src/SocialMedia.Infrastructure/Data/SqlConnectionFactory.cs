using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using SocialMedia.Application.Abstractions.Data;
namespace SocialMedia.Infrastructure.Data;

public sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(configuration.GetConnectionString("sqlserverConnectionString"));
        connection.Open();
        return connection;
    }
}
