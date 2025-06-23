using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public class ReactService : IReactService
{
    public Task ReactToCommentAsync(ReactToCommentDto reactToCommentDto)
    {
        throw new NotImplementedException();
    }

    public Task ReactToPostAsync(ReactToPostDto reactToPostDto)
    {
        throw new NotImplementedException();
    }

    public Task RemoveCommentReactAsync(int commentId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task RemovePostReactAsync(int postId, int userId)
    {
        throw new NotImplementedException();
    }
}
