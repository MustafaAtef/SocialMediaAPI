
using System.Data;

using SocialMedia.Application.Abstractions.Data;
namespace SocialMedia.Infrastructure.Data;

public class TransactionContext : ITransactionContext
{
    public IDbConnection Connection { get; set; } = default!;
    public IDbTransaction Transaction { get; set; } = default!;
}
