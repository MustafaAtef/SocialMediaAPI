using MediatR;

using SocialMedia.Core.Abstractions;

namespace SocialMedia.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{

}

public interface ICurrentUserQuery<TResponse> : IQuery<TResponse>
{
    public int UserId { get; set; }
}
