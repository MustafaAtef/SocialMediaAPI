using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SocialMedia.Api.Controllers;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Commands.Create;
using SocialMedia.Application.Posts.Commands.PermanentDelete;
using SocialMedia.Application.Posts.Commands.Restore;
using SocialMedia.Application.Posts.Commands.SoftDelete;
using SocialMedia.Application.Posts.Commands.Update;
using SocialMedia.Application.Posts.Queries.GetPagedDeletedPosts;
using SocialMedia.Application.Posts.Queries.GetPost;
using SocialMedia.Application.Posts.Responses;
using SocialMedia.WebApi.Controllers.Posts.Requests;

namespace SocialMedia.WebApi.Controllers.Posts;

[Route("api/posts")]
[Authorize]
public class PostsController(ISender sender) : ApiController
{
    [HttpPost]
    public async Task<ActionResult<PostResponse>> CreateAsync(CreatePostRequest request)
    {
        var result = await sender.Send(new CreatePostCommand(request.Content, request.Attachments));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpPut("{postId}")]
    public async Task<ActionResult<PostResponse>> UpdateAsync(
        int postId,
        UpdatePostRequest request)
    {
        var result = await sender.Send(new UpdatePostCommand(
            postId,
            request.Content,
            request.AddedAttachments,
            request.DeletedAttachmentIds));

        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpGet("{postId}")]
    [AllowAnonymous]
    public async Task<ActionResult<PostResponse>> GetPostAsync(
        int postId
       )
    {
        var result = await sender.Send(new GetPostQuery(postId));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);

    }

    [HttpDelete("{postId}")]
    public async Task<IActionResult> DeleteAsync(int postId)
    {
        var result = await sender.Send(new SoftDeletePostCommand(postId));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }

    [HttpGet("trash")]
    public async Task<ActionResult<PagedList<PostResponse>>> GetAllDeletedPosts(int page = 1, int pageSize = 10)
    {
        var result = await sender.Send(new GetPagedDeletedPostsQuery(page, pageSize));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpPost("trash/{postId}/restore")]
    public async Task<ActionResult<PostResponse>> RestoreDeletedPost(int postId)
    {
        var result = await sender.Send(new RestorePostCommand(postId));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpDelete("{postId}/permanent-delete")]
    public async Task<IActionResult> PermanentDeleteAsync(int postId)
    {
        var result = await sender.Send(new PermanentDeletePostCommand(postId));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }
}