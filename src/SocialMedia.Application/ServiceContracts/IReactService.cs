using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IReactService
{
    Task ReactToPostAsync(ReactToPostDto reactToPostDto);
    Task ReactToCommentAsync(ReactToCommentDto reactToCommentDto);
    Task RemovePostReactAsync(int postId, int userId);
    Task RemoveCommentReactAsync(int commentId, int userId);
}
