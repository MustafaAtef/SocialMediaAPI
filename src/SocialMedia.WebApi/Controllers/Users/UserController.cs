using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SocialMedia.Api.Controllers;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Queries.GetPagedPosts;
using SocialMedia.Application.Posts.Responses;
using SocialMedia.Application.Users.Commands.Follow;
using SocialMedia.Application.Users.Commands.UnFollow;
using SocialMedia.Application.Users.Commands.Update;
using SocialMedia.Application.Users.Queries.GetAllGroupMessages;
using SocialMedia.Application.Users.Queries.GetPagedFollowers;
using SocialMedia.Application.Users.Queries.GetPagedFollowing;
using SocialMedia.Application.Users.Queries.GetPagedGroupMessages;
using SocialMedia.Application.Users.Responses;
using SocialMedia.WebApi.Controllers.Users.Requests;

namespace SocialMedia.WebApi.Controllers.Users;

[Route("api/users")]
public class UserController(ISender sender) : ApiController
{
    [HttpPost("{userId}/follow")]
    [Authorize]
    public async Task<ActionResult> Follow([FromRoute] int userId)
    {
        var result = await sender.Send(new FollowUserCommand(userId));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }

    [HttpDelete("{userId}/follow")]
    [Authorize]
    public async Task<ActionResult> UnFollow([FromRoute] int userId)
    {
        var result = await sender.Send(new UnFollowUserCommand(userId));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }

    [HttpGet("{userId}/followers")]
    public async Task<ActionResult<PagedList<UserResponse>>> GetFollowers(
        [FromRoute] int userId,
        int page = 1,
        int pageSize = 100)
    {
        var result = await sender.Send(new GetPagedFollowersQuery(userId, page, pageSize));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpGet("{userId}/followings")]
    public async Task<ActionResult<PagedList<UserResponse>>> GetFollowings(
        [FromRoute] int userId,
        int page = 1,
        int pageSize = 100)
    {
        var result = await sender.Send(new GetPagedFollowingQuery(userId, page, pageSize));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpGet("{userId}/posts")]
    public async Task<ActionResult<PagedList<PostResponse>>> GetAllPosts(
        [FromRoute] int userId,
        int page = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new GetPagedPostsQuery(userId, page, pageSize));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpGet("/messages")]
    public async Task<ActionResult<ICollection<GroupMessagesDto>>> GetAllGroupsMessages(
        [FromQuery] int olderMessagesSize = 25)
    {
        var result = await sender.Send(new GetAllGroupMessagesQuery(olderMessagesSize));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpGet("/messages/groups/{groupId}")]
    public async Task<ActionResult<GroupMessagesDto>> GetGroupMessages(
        [FromRoute] Guid groupId,
        [FromQuery] int? lastMessageId,
        [FromQuery] int olderMessagesSize = 25)
    {

        var result = await sender.Send(new GetPagedGroupMessagesQuery(
            groupId,
            lastMessageId,
            olderMessagesSize));

        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<UserResponse>> UpdateUser([FromForm] UpdateUserRequest request)
    {
        var result = await sender.Send(new UpdateUserCommand(request.FirstName, request.LastName, request.Avatar));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }
}