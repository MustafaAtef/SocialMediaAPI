using MediatR;

using SocialMedia.Core.Abstractions;

namespace SocialMedia.Application.Abstractions.Messaging;

public interface IBaseCommand
{

}

public interface ICommand : IRequest<Result>, IBaseCommand
{

}

public interface ICommand<TResult> : IRequest<Result<TResult>>, IBaseCommand
{

}
