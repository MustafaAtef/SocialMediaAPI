using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Responses;
using SocialMedia.Application.Users.Responses;
using SocialMedia.IntegrationTests.Infrastructure;

using Xunit;

namespace SocialMedia.IntegrationTests.Users;

public class UserEndpointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Follow_WithValidTarget_ReturnsOk_AndAppearsInFollowersAndFollowings()
    {
        var follower = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-follow-follower"));
        var following = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-follow-following"));

        var followResponse = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/users/{following.Id}/follow",
            follower.Token);

        followResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await FlushProjectionOutboxAsync();

        var followersResponse = await HttpClient.GetFromJsonAsync<PagedList<UserResponse>>($"/api/users/{following.Id}/followers");
        followersResponse.Should().NotBeNull();
        followersResponse!.TotalCount.Should().Be(1);
        followersResponse.Data.Select(u => u.Id).Should().Contain(follower.Id);

        var followingsResponse = await HttpClient.GetFromJsonAsync<PagedList<UserResponse>>($"/api/users/{follower.Id}/followings");
        followingsResponse.Should().NotBeNull();
        followingsResponse!.TotalCount.Should().Be(1);
        followingsResponse.Data.Select(u => u.Id).Should().Contain(following.Id);
    }

    [Fact]
    public async Task Follow_Self_ReturnsBadRequest()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-self-follow"));

        var response = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/users/{user.Id}/follow",
            user.Token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Follow_UnknownUser_ReturnsNotFound()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-follow-missing"));

        var response = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Post,
            "/api/users/99999/follow",
            user.Token);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Follow_SameUserTwice_ReturnsConflictOnSecondAttempt()
    {
        var follower = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-dup-follow-a"));
        var following = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-dup-follow-b"));

        var first = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/users/{following.Id}/follow",
            follower.Token);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/users/{following.Id}/follow",
            follower.Token);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UnFollow_WhenFollowingExists_ReturnsOk()
    {
        var follower = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-unfollow-a"));
        var following = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-unfollow-b"));

        await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Post,
            $"/api/users/{following.Id}/follow",
            follower.Token);

        var unfollowResponse = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/users/{following.Id}/follow",
            follower.Token);

        unfollowResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await FlushProjectionOutboxAsync();

        var followersResponse = await HttpClient.GetFromJsonAsync<PagedList<UserResponse>>($"/api/users/{following.Id}/followers");
        followersResponse.Should().NotBeNull();
        followersResponse!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task UnFollow_WhenNotFollowing_ReturnsConflict()
    {
        var follower = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-unfollow-missing-a"));
        var following = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-unfollow-missing-b"));

        var response = await IntegrationTestHelper.SendAuthorizedAsync(
            HttpClient,
            HttpMethod.Delete,
            $"/api/users/{following.Id}/follow",
            follower.Token);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateUser_WithValidPayload_ReturnsUpdatedUser()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-update"));

        using var form = IntegrationTestHelper.BuildUpdateUserForm(firstName: "UpdatedFirst", lastName: "UpdatedLast");
        var response = await IntegrationTestHelper.SendAuthorizedFormAsync(
            HttpClient,
            HttpMethod.Put,
            "/api/users",
            user.Token,
            form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await IntegrationTestHelper.ReadJsonAsync<UserResponse>(response);
        updated.Name.Should().Be("UpdatedFirst UpdatedLast");
    }

    [Fact]
    public async Task UpdateUser_WithInvalidPayload_ReturnsBadRequest()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-invalid-update"));

        using var form = IntegrationTestHelper.BuildUpdateUserForm(firstName: "xy");
        var response = await IntegrationTestHelper.SendAuthorizedFormAsync(
            HttpClient,
            HttpMethod.Put,
            "/api/users",
            user.Token,
            form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_WithoutAuthorization_ReturnsUnauthorized()
    {
        using var form = IntegrationTestHelper.BuildUpdateUserForm(firstName: "NoAuthName");
        var response = await HttpClient.PutAsync("/api/users", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserPosts_ReturnsPagedPosts()
    {
        var user = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, IntegrationTestHelper.CreateUser("users-posts"));

        await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "First post");
        await IntegrationTestHelper.CreatePostAndReadAsync(HttpClient, user.Token, "Second post");

        await FlushProjectionOutboxAsync();

        var response = await HttpClient.GetFromJsonAsync<PagedList<PostResponse>>($"/api/users/{user.Id}/posts?page=1&pageSize=10");

        response.Should().NotBeNull();
        response!.TotalCount.Should().Be(2);
        response.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFollowers_ForUnknownUser_ReturnsNotFound()
    {
        var response = await HttpClient.GetAsync("/api/users/99999/followers");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllGroupsMessages_WithoutAuthorization_ReturnsInternalServerError()
    {
        var response = await HttpClient.GetAsync("/messages");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
