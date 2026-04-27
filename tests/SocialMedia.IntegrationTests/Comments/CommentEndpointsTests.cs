using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using SocialMedia.Application.Comments.Responses;
using SocialMedia.Application.Dtos;
using SocialMedia.IntegrationTests.Infrastructure;

using Xunit;

namespace SocialMedia.IntegrationTests.Comments;

[Trait("Category", "Integration")]
public class CommentEndpointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateComment_WithoutAuthorization_ReturnsUnauthorized()
    {
        var response = await HttpClient.PostAsJsonAsync("/api/posts/1/comments", new { content = "No auth" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateComment_WithValidPayload_ReturnsComment()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-create"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Post for comment");

        var comment = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "First comment");

        comment.Id.Should().BeGreaterThan(0);
        comment.PostId.Should().Be(post.Id);
        comment.Content.Should().Be("First comment");
        comment.ParentCommentId.Should().BeNull();
    }

    [Fact]
    public async Task CreateComment_WithInvalidPayload_ReturnsBadRequest()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-invalid"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Post for invalid comment");

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/comments",
            user.Token,
            new { content = string.Empty });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateComment_ForUnknownPost_ReturnsNotFound()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-post-notfound"));

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            "/api/posts/99999/comments",
            user.Token,
            new { content = "Comment" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReplyComment_WithValidParent_ReturnsReply()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-reply"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Post for reply");
        var parent = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "Parent");

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/comments/{parent.Id}",
            user.Token,
            new { content = "Reply comment" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reply = await IntegrationTestHelper.ReadJsonAsync<CommentResponse>(response);
        reply.ParentCommentId.Should().Be(parent.Id);
        reply.Content.Should().Be("Reply comment");
    }

    [Fact]
    public async Task ReplyComment_ToReply_ReturnsBadRequest()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-reply-on-reply"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Post for nested reply");
        var parent = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "Parent");

        var firstReplyResponse = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/comments/{parent.Id}",
            user.Token,
            new { content = "First reply" });
        var firstReply = await IntegrationTestHelper.ReadJsonAsync<CommentResponse>(firstReplyResponse);

        var secondReplyResponse = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/comments/{firstReply.Id}",
            user.Token,
            new { content = "Second level reply" });

        secondReplyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateComment_AsOwner_ReturnsUpdatedComment()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-update"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Post for update");
        var comment = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "Old content");

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Put,
            $"/api/posts/{post.Id}/comments/{comment.Id}",
            user.Token,
            new { content = "New content" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await IntegrationTestHelper.ReadJsonAsync<CommentResponse>(response);
        updated.Content.Should().Be("New content");
    }

    [Fact]
    public async Task UpdateComment_AsAnotherUser_ReturnsForbidden()
    {
        var owner = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-update-owner"));
        var other = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-update-other"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, owner.Token, "Post for forbidden update");
        var comment = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, owner.Token, post.Id, "Owner comment");

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Put,
            $"/api/posts/{post.Id}/comments/{comment.Id}",
            other.Token,
            new { content = "Hijacked content" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteComment_AsOwner_ReturnsOk()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-delete"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Post for delete");
        var comment = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "Delete me");

        var response = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/posts/{post.Id}/comments/{comment.Id}",
            user.Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteComment_AsAnotherUser_ReturnsForbidden()
    {
        var owner = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-delete-owner"));
        var other = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-delete-other"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, owner.Token, "Post for forbidden delete");
        var comment = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, owner.Token, post.Id, "Owner comment");

        var response = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/posts/{post.Id}/comments/{comment.Id}",
            other.Token);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPostComments_ReturnsOnlyTopLevelComments()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-list"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Post for comments list");

        var parent = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "Parent comment");

        await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/comments/{parent.Id}",
            user.Token,
            new { content = "Reply comment" });

        await FlushProjectionOutboxAsync();

        var response = await HttpClient.GetFromJsonAsync<PagedList<CommentResponse>>($"/api/posts/{post.Id}/comments?page=1&pageSize=10");

        response.Should().NotBeNull();
        response!.TotalCount.Should().Be(1);
        response.Data.Should().HaveCount(1);
        response.Data.First().ParentCommentId.Should().BeNull();
    }

    [Fact]
    public async Task GetCommentReplies_ReturnsRepliesOrNotFound()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("comments-replies"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Post for replies");
        var parent = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "Parent");

        await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/comments/{parent.Id}",
            user.Token,
            new { content = "Reply" });

        await FlushProjectionOutboxAsync();

        var repliesResponse = await HttpClient.GetFromJsonAsync<PagedList<CommentResponse>>(
            $"/api/posts/{post.Id}/comments/{parent.Id}/replies?page=1&pageSize=10");

        repliesResponse.Should().NotBeNull();
        repliesResponse!.TotalCount.Should().Be(1);
        repliesResponse.Data.Should().HaveCount(1);

        var missingParentResponse = await HttpClient.GetAsync($"/api/posts/{post.Id}/comments/99999/replies");
        missingParentResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
