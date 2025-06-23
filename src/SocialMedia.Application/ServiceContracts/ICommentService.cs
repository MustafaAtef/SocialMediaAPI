using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface ICommentService
{
    Task<CreatedCommentDto> CreateAsync(CreateCommentDto createCommentDto);
    Task<CreatedCommentDto> ReplyAsync(ReplyCommentDto replyCommentDto);
    Task<CommentDto> UpdateAsync(UpdateCommentDto updateCommentDto);
}
