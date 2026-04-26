using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using SocialMedia.Application.Abstractions.Data;
namespace SocialMedia.Infrastructure.Data;

public sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}
