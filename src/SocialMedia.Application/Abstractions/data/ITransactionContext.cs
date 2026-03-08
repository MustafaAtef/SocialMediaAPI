using System.Data;

namespace SocialMedia.Application.Abstractions.Data;

public interface ITransactionContext
{
    IDbConnection Connection { get; set; }
    IDbTransaction Transaction { get; set; }
}
