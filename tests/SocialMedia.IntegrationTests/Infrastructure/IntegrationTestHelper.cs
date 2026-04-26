using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;

using FluentAssertions;

using SocialMedia.Application.Auth.Responses;
using SocialMedia.Application.Comments.Responses;
using SocialMedia.Application.Posts.Responses;

namespace SocialMedia.IntegrationTests.Infrastructure;

internal sealed record TestUserRegistration(
    string FirstName,
    string LastName,
    string Email,
    string Password);

internal static class IntegrationTestHelper
{
    private static int _counter;

    public static TestUserRegistration CreateUser(string? prefix = null)
    {
        var id = Interlocked.Increment(ref _counter);
        var key = prefix ?? "user";

        return new TestUserRegistration(
            FirstName: $"{key}fn{id}",
            LastName: $"{key}ln{id}",
            Email: $"{key}{id}@example.com",
            Password: $"StrongPass!{id}x");
    }

    public static MultipartFormDataContent BuildRegisterForm(TestUserRegistration user)
    {
        return new MultipartFormDataContent
        {
            { new StringContent(user.FirstName), "FirstName" },
            { new StringContent(user.LastName), "LastName" },
            { new StringContent(user.Email), "Email" },
            { new StringContent(user.Password), "Password" }
        };
    }

    public static MultipartFormDataContent BuildCreatePostForm(string content)
    {
        return new MultipartFormDataContent
        {
            { new StringContent(content), "Content" }
        };
    }

    public static MultipartFormDataContent BuildUpdatePostForm(string? content = null)
    {
        var form = new MultipartFormDataContent();

        if (content is not null)
        {
            form.Add(new StringContent(content), "Content");
        }

        return form;
    }

    public static MultipartFormDataContent BuildUpdateUserForm(string? firstName = null, string? lastName = null)
    {
        var form = new MultipartFormDataContent();

        if (firstName is not null)
        {
            form.Add(new StringContent(firstName), "FirstName");
        }

        if (lastName is not null)
        {
            form.Add(new StringContent(lastName), "LastName");
        }

        return form;
    }

    public static async Task<HttpResponseMessage> RegisterAsync(HttpClient httpClient, TestUserRegistration user)
    {
        using var form = BuildRegisterForm(user);
        return await httpClient.PostAsync("/api/auth/register", form);
    }

    public static async Task<AuthenticatedUserResponse> RegisterAndReadAsync(HttpClient httpClient, TestUserRegistration user)
    {
        var response = await RegisterAsync(httpClient, user);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Register currently issues a token before the user ID is persisted,
        // so we always re-login to get a token with a valid NameIdentifier claim.
        return await LoginAndReadAsync(httpClient, user.Email, user.Password);
    }

    public static async Task<AuthenticatedUserResponse> LoginAndReadAsync(
        HttpClient httpClient,
        string email,
        string password)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await ReadJsonAsync<AuthenticatedUserResponse>(response);
    }

    public static async Task<HttpResponseMessage> SendAuthorizedJsonAsync<TBody>(
        HttpClient httpClient,
        HttpMethod method,
        string uri,
        string token,
        TBody body)
    {
        using var request = new HttpRequestMessage(method, uri)
        {
            Content = JsonContent.Create(body)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await httpClient.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> SendAuthorizedFormAsync(
        HttpClient httpClient,
        HttpMethod method,
        string uri,
        string token,
        MultipartFormDataContent form)
    {
        using var request = new HttpRequestMessage(method, uri)
        {
            Content = form
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await httpClient.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpClient httpClient,
        HttpMethod method,
        string uri,
        string token)
    {
        using var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await httpClient.SendAsync(request);
    }

    public static async Task<PostResponse> CreatePostAndReadAsync(
        HttpClient httpClient,
        string token,
        string content = "Integration post")
    {
        using var form = BuildCreatePostForm(content);
        var response = await SendAuthorizedFormAsync(httpClient, HttpMethod.Post, "/api/posts", token, form);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return await ReadJsonAsync<PostResponse>(response);
    }

    public static async Task<CommentResponse> CreateCommentAndReadAsync(
        HttpClient httpClient,
        string token,
        int postId,
        string content = "Integration comment")
    {
        var response = await SendAuthorizedJsonAsync(
            httpClient,
            HttpMethod.Post,
            $"/api/posts/{postId}/comments",
            token,
            new { content });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await ReadJsonAsync<CommentResponse>(response);
    }

    public static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<T>();
        payload.Should().NotBeNull();

        return payload!;
    }
}
