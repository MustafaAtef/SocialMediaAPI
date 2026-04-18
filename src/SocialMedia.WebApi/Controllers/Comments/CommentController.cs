using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SocialMedia.Api.Controllers;
using SocialMedia.Application.Comments.Commands.Create;
using SocialMedia.Application.Comments.Commands.Delete;
using SocialMedia.Application.Comments.Commands.Reply;
using SocialMedia.Application.Comments.Commands.Update;
using SocialMedia.Application.Comments.Queries.GetPagedComments;
using SocialMedia.Application.Comments.Queries.GetPagedReplies;
using SocialMedia.Application.Comments.Responses;
using SocialMedia.Application.Dtos;
using SocialMedia.WebApi.Controllers.Comments.Requests;

namespace SocialMedia.WebApi.Controllers.Comments;

[Route("api/posts")]
public class CommentController(ISender sender) : ApiController
{
    [HttpPost("{postId}/comments")]
    [Authorize]
    public async Task<ActionResult<CommentResponse>> CreateAsync(
        [FromRoute] int postId,
        [FromBody] CreateCommentRequest request)
    {
        var result = await sender.Send(new CreateCommentCommand(postId, request.Content));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpPost("{postId}/comments/{parentCommentId}")]
    [Authorize]
    public async Task<ActionResult<CommentResponse>> ReplyAsync(
        [FromRoute] int postId,
        [FromRoute] int parentCommentId,
        [FromBody] ReplyCommentRequest request)
    {
        var result = await sender.Send(new ReplyCommentCommand(postId, parentCommentId, request.Content));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpPut("{postId}/comments/{commentId}")]
    [Authorize]
    public async Task<ActionResult<CommentResponse>> UpdateAsync(
        [FromRoute] int postId,
        [FromRoute] int commentId,
        [FromBody] UpdateCommentRequest request)
    {
        var result = await sender.Send(new UpdateCommentCommand(commentId, postId, request.Content));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpGet("{postId}/comments")]
    public async Task<ActionResult<PagedList<CommentResponse>>> GetAllPostComments(
        int postId, int page = 1, int pageSize = 10)
    {
        var result = await sender.Send(new GetPagedCommentsQuery(
            postId,
            page,
            pageSize));

        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpGet("{postId}/comments/{commentId}/replies")]
    public async Task<ActionResult<PagedList<CommentResponse>>> GetAllCommentReplies(
        [FromRoute] int postId,
        [FromRoute] int commentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await sender.Send(new GetPagedRepliesQuery(postId, commentId, page, pageSize));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpDelete("{postId}/comments/{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteAsync([FromRoute] int postId, [FromRoute] int commentId)
    {
        var result = await sender.Send(new DeleteCommentCommand(postId, commentId));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }
}