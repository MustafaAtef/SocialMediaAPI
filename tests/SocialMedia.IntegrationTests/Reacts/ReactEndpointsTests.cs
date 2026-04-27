using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using SocialMedia.Application.Dtos;
using SocialMedia.Application.Reacts.Responses;
using SocialMedia.Core.Enumerations;
using SocialMedia.IntegrationTests.Infrastructure;

using Xunit;

namespace SocialMedia.IntegrationTests.Reacts;

[Trait("Category", "Integration")]
public class ReactEndpointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task ReactToPost_WithoutAuthorization_ReturnsUnauthorized()
    {
        var response = await HttpClient.PostAsJsonAsync("/api/posts/1/reacts", new { reactType = ReactType.Like });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReactToPost_WithValidPayload_ReturnsPostReact()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-post-create"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "React post");

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/reacts",
            user.Token,
            new { reactType = ReactType.Love });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var react = await IntegrationTestHelper.ReadJsonAsync<PostReactResponse>(response);
        react.PostId.Should().Be(post.Id);
        react.TypeNo.Should().Be(ReactType.Love);
    }

    [Fact]
    public async Task ReactToPost_SameUserTwice_UpdatesExistingReact()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-post-update"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "React update post");

        var firstResponse = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/reacts",
            user.Token,
            new { reactType = ReactType.Like });
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/reacts",
            user.Token,
            new { reactType = ReactType.Wow });
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await FlushProjectionOutboxAsync();

        var getReactsResponse = await HttpClient.GetFromJsonAsync<PagedList<PostReactResponse>>($"/api/posts/{post.Id}/reacts");
        getReactsResponse.Should().NotBeNull();
        getReactsResponse!.TotalCount.Should().Be(1);
        getReactsResponse.Data.First().TypeNo.Should().Be(ReactType.Wow);
    }

    [Fact]
    public async Task ReactToPost_WithInvalidReactType_ReturnsBadRequest()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-post-invalid"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Invalid react post");

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/reacts",
            user.Token,
            new { reactType = (ReactType)999 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReactToPost_ForMissingPost_ReturnsNotFound()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-post-missing"));

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            "/api/posts/99999/reacts",
            user.Token,
            new { reactType = ReactType.Like });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemovePostReact_WhenExists_ReturnsOk_AndRemovesReact()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-post-remove"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Remove react post");

        await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/reacts",
            user.Token,
            new { reactType = ReactType.Laugh });

        var removeResponse = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/posts/{post.Id}/reacts",
            user.Token);

        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await FlushProjectionOutboxAsync();

        var list = await HttpClient.GetFromJsonAsync<PagedList<PostReactResponse>>($"/api/posts/{post.Id}/reacts");
        list.Should().NotBeNull();
        list!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task RemovePostReact_WhenNotExists_ReturnsNotFound()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-post-remove-missing"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "No react post");

        var response = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/posts/{post.Id}/reacts",
            user.Token);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReactToComment_WithValidPayload_ReturnsCommentReact()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-comment-create"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "React comment post");
        var comment = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "Comment to react");

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/comments/{comment.Id}/reacts",
            user.Token,
            new { reactType = ReactType.Angry });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var react = await IntegrationTestHelper.ReadJsonAsync<CommentReactResponse>(response);
        react.CommentId.Should().Be(comment.Id);
        react.TypeNo.Should().Be(ReactType.Angry);
    }

    [Fact]
    public async Task ReactToComment_ForMissingComment_ReturnsNotFound()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-comment-missing"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Missing comment react post");

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/comments/99999/reacts",
            user.Token,
            new { reactType = ReactType.Sad });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveCommentReact_WhenExists_ReturnsOk_AndRemovesReact()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-comment-remove"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Remove comment react post");
        var comment = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "Comment to remove react");

        await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/comments/{comment.Id}/reacts",
            user.Token,
            new { reactType = ReactType.Love });

        var removeResponse = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/posts/{post.Id}/comments/{comment.Id}/reacts",
            user.Token);

        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await FlushProjectionOutboxAsync();

        var list = await HttpClient.GetFromJsonAsync<PagedList<CommentReactResponse>>(
            $"/api/posts/{post.Id}/comments/{comment.Id}/reacts");

        list.Should().NotBeNull();
        list!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task RemoveCommentReact_WhenMissing_ReturnsNotFound()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-comment-remove-missing"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "No comment react post");
        var comment = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "No react comment");

        var response = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/posts/{post.Id}/comments/{comment.Id}/reacts",
            user.Token);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReactsEndpoints_AsAnonymous_ReturnExpectedData()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("react-anon"));
        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Anonymous list post");
        var comment = await IntegrationTestHelper.CreateCommentAndReadAsync(HttpClient, user.Token, post.Id, "Anonymous list comment");

        await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/reacts",
            user.Token,
            new { reactType = ReactType.Like });

        await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/{post.Id}/comments/{comment.Id}/reacts",
            user.Token,
            new { reactType = ReactType.Wow });

        await FlushProjectionOutboxAsync();

        var postReactsResponse = await HttpClient.GetAsync($"/api/posts/{post.Id}/reacts");
        postReactsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var postReacts = await postReactsResponse.Content.ReadFromJsonAsync<PagedList<PostReactResponse>>();
        postReacts.Should().NotBeNull();
        postReacts!.TotalCount.Should().Be(1);

        var commentReactsResponse = await HttpClient.GetAsync($"/api/posts/{post.Id}/comments/{comment.Id}/reacts");
        commentReactsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var commentReacts = await commentReactsResponse.Content.ReadFromJsonAsync<PagedList<CommentReactResponse>>();
        commentReacts.Should().NotBeNull();
        commentReacts!.TotalCount.Should().Be(1);
    }
}
