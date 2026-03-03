using MediatR;

using SocialMedia.Core.Abstractions;

namespace SocialMedia.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{

}
