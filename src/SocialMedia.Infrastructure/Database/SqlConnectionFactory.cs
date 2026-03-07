using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using SocialMedia.Application.Abstractions.data;

namespace SocialMedia.Infrastructure.Database;

public sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(configuration.GetConnectionString("sqlserverConnectionString"));
        connection.Open();
        return connection;
    }
}
