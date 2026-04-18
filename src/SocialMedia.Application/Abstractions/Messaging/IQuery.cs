using MediatR;

using SocialMedia.Core.Abstractions;

namespace SocialMedia.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{

}

public interface ICurrentUserQuery
{
    public int UserId { get; set; }
}

public interface ICurrentUserQuery<TResponse> : IQuery<TResponse>, ICurrentUserQuery
{
}
