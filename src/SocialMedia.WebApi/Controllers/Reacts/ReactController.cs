using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SocialMedia.Api.Controllers;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Reacts.Commands.ReactToComment;
using SocialMedia.Application.Reacts.Commands.ReactToPost;
using SocialMedia.Application.Reacts.Commands.RemoveCommentReact;
using SocialMedia.Application.Reacts.Commands.RemovePostReact;
using SocialMedia.Application.Reacts.Queries.GetPagedCommentReacts;
using SocialMedia.Application.Reacts.Queries.GetPagedPostReacts;
using SocialMedia.Application.Reacts.Responses;
using SocialMedia.WebApi.Controllers.Reacts.Requests;

namespace SocialMedia.WebApi.Controllers.Reacts;

[Route("api/posts")]
[Authorize]
public class ReactController(ISender sender) : ApiController
{
    [HttpPost("{postId}/reacts")]
    public async Task<ActionResult<PostReactResponse>> ReactToPostAsync(
        [FromRoute] int postId,
        [FromBody] ReactToPostRequest request)
    {
        var result = await sender.Send(new ReactToPostCommand(postId, request.ReactType));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpPost("{postId}/comments/{commentId}/reacts")]
    public async Task<ActionResult<CommentReactResponse>> ReactToCommentAsync(
        [FromRoute] int postId,
        [FromRoute] int commentId,
        [FromBody] ReactToCommentRequest request)
    {
        var result = await sender.Send(new ReactToCommentCommand(postId, commentId, request.ReactType));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpDelete("{postId}/reacts")]
    public async Task<IActionResult> RemovePostReactAsync([FromRoute] int postId)
    {
        var result = await sender.Send(new RemovePostReactCommand(postId));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }

    [HttpDelete("{postId}/comments/{commentId}/reacts")]
    public async Task<IActionResult> RemoveCommentReactAsync([FromRoute] int postId, [FromRoute] int commentId)
    {
        var result = await sender.Send(new RemoveCommentReactCommand(postId, commentId));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }

    [HttpGet("{postId}/reacts")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedList<PostReactResponse>>> GetPostReactsAsync(
        [FromRoute] int postId,
        int page = 1,
        int pageSize = 100)
    {
        var result = await sender.Send(new GetPagedPostReactsQuery(postId, page, pageSize));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpGet("{postId}/comments/{commentId}/reacts")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedList<CommentReactResponse>>> GetCommentReactsAsync(
        [FromRoute] int postId,
        [FromRoute] int commentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var result = await sender.Send(new GetPagedCommentReactsQuery(postId, commentId, page, pageSize));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }
}