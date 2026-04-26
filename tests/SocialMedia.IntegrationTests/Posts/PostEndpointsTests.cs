using System.Net;

using FluentAssertions;

using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Responses;
using SocialMedia.IntegrationTests.Infrastructure;

using Xunit;

namespace SocialMedia.IntegrationTests.Posts;

public class PostEndpointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreatePost_WithoutAuthorization_ReturnsUnauthorized()
    {
        using var form = IntegrationTestHelper.BuildCreatePostForm("Unauthorized post");

        var response = await HttpClient.PostAsync("/api/posts", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePost_WithValidPayload_ReturnsCreatedPost()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-create"));

        var post = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Valid post content");

        post.Id.Should().BeGreaterThan(0);
        post.Content.Should().Be("Valid post content");
        post.Author.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task CreatePost_WithEmptyContent_ReturnsBadRequest()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-invalid-create"));

        using var form = IntegrationTestHelper.BuildCreatePostForm(string.Empty);
        var response = await IntegrationTestHelper.SendAuthorizedFormAsync(
            HttpClient,
            HttpMethod.Post,
            "/api/posts",
            user.Token,
            form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPost_AsAnonymous_ReturnsPost()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-get"));
        var created = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Post for anonymous get");

        await FlushProjectionOutboxAsync();

        var response = await HttpClient.GetAsync($"/api/posts/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var post = await IntegrationTestHelper.ReadJsonAsync<PostResponse>(response);
        post.Id.Should().Be(created.Id);
        post.Content.Should().Be("Post for anonymous get");
    }

    [Fact]
    public async Task GetPost_WithUnknownId_ReturnsNotFound()
    {
        var response = await HttpClient.GetAsync("/api/posts/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePost_AsOwner_ReturnsUpdatedPost()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-update-owner"));
        var created = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Initial post content");

        using var form = IntegrationTestHelper.BuildUpdatePostForm("Updated post content");
        var response = await IntegrationTestHelper.SendAuthorizedFormAsync(
            HttpClient,
            HttpMethod.Put,
            $"/api/posts/{created.Id}",
            user.Token,
            form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await IntegrationTestHelper.ReadJsonAsync<PostResponse>(response);
        updated.Content.Should().Be("Updated post content");
    }

    [Fact]
    public async Task UpdatePost_AsDifferentUser_ReturnsInternalServerError()
    {
        var owner = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-update-owner2"));
        var other = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-update-other"));
        var created = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, owner.Token, "Owner content");

        using var form = IntegrationTestHelper.BuildUpdatePostForm("Should fail");
        var response = await IntegrationTestHelper.SendAuthorizedFormAsync(
            HttpClient,
            HttpMethod.Put,
            $"/api/posts/{created.Id}",
            other.Token,
            form);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SoftDeletePost_ThenGetPost_ReturnsNotFound()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-soft-delete"));
        var created = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Soft-delete target");

        var deleteResponse = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/posts/{created.Id}",
            user.Token);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await FlushProjectionOutboxAsync();

        var getResponse = await HttpClient.GetAsync($"/api/posts/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTrash_AfterSoftDelete_ReturnsDeletedPosts()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-trash"));
        var created = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Trash post");

        await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/posts/{created.Id}",
            user.Token);

        await FlushProjectionOutboxAsync();

        var request = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Get,
            "/api/posts/trash?page=1&pageSize=10",
            user.Token);

        request.StatusCode.Should().Be(HttpStatusCode.OK);

        var trash = await IntegrationTestHelper.ReadJsonAsync<PagedList<PostResponse>>(request);
        trash.TotalCount.Should().Be(1);
        trash.Data.Select(p => p.Id).Should().Contain(created.Id);
    }

    [Fact]
    public async Task RestoreDeletedPost_ReturnsRestoredPost()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-restore"));
        var created = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Restore target");

        await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/posts/{created.Id}",
            user.Token);

        var restoreResponse = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/posts/trash/{created.Id}/restore",
            user.Token);

        restoreResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await FlushProjectionOutboxAsync();

        var getResponse = await HttpClient.GetAsync($"/api/posts/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RestoreUnknownDeletedPost_ReturnsNotFound()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-restore-missing"));

        var response = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Post,
            "/api/posts/trash/99999/restore",
            user.Token);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PermanentDeletePost_RemovesPostCompletely()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("posts-permanent"));
        var created = await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Permanent delete target");

        var deleteResponse = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/posts/{created.Id}/permanent-delete",
            user.Token);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await FlushProjectionOutboxAsync();

        var getResponse = await HttpClient.GetAsync($"/api/posts/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
