using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SocialMedia.Application.Auth.Responses;
using SocialMedia.IntegrationTests.Infrastructure;

using Xunit;

namespace SocialMedia.IntegrationTests.Auth;

public class AuthEndpointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_WithValidPayload_ReturnsAuthenticatedUser()
    {
        var user = IntegrationTestHelper.CreateUser("auth-register");

        var response = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, user);

        response.Id.Should().BeGreaterThan(0);
        response.Email.Should().Be(user.Email);
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
        response.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var user = IntegrationTestHelper.CreateUser("auth-duplicate");
        await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, user);

        var secondAttempt = await IntegrationTestHelper.RegisterAsync(HttpClient, user);

        secondAttempt.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidPayload_ReturnsBadRequest()
    {
        using var invalidForm = new MultipartFormDataContent
        {
            { new StringContent("ab"), "FirstName" },
            { new StringContent("xy"), "LastName" },
            { new StringContent("invalid-email"), "Email" },
            { new StringContent("short"), "Password" }
        };

        var response = await HttpClient.PostAsync("/api/auth/register", invalidForm);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblem.Should().NotBeNull();
        validationProblem!.Errors.Keys.Should().Contain(["FirstName", "LastName", "Email", "Password"]);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthenticatedUser()
    {
        var user = IntegrationTestHelper.CreateUser("auth-login");
        await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, user);

        var response = await IntegrationTestHelper.LoginAndReadAsync(HttpClient, user.Email, user.Password);

        response.Email.Should().Be(user.Email);
        response.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsNotFound()
    {
        var user = IntegrationTestHelper.CreateUser("auth-invalid-login");
        await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, user);

        var response = await HttpClient.PostAsJsonAsync("/api/auth/login", new
        {
            email = user.Email,
            password = "WrongPassword!111"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RefreshToken_WithValidPayload_ReturnsNewTokens()
    {
        var user = IntegrationTestHelper.CreateUser("auth-refresh");
        var registered = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, user);

        var response = await HttpClient.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            token = registered.Token,
            refreshToken = registered.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await response.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();
        refreshed.Should().NotBeNull();
        refreshed!.Token.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBe(registered.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsBadRequest()
    {
        var user = IntegrationTestHelper.CreateUser("auth-invalid-refresh");
        var registered = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, user);

        var response = await HttpClient.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            token = "not-a-jwt-token",
            refreshToken = registered.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResendVerification_WithUnknownEmail_ReturnsOk()
    {
        var response = await HttpClient.PostAsJsonAsync("/api/auth/resend-verification", new
        {
            email = "unknown@example.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task VerifyEmail_WithValidToken_ReturnsOk_AndMarksUserAsVerified()
    {
        var user = IntegrationTestHelper.CreateUser("auth-verify");
        await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, user);

        var createdUser = await DbContext.Users.AsNoTracking().SingleAsync(u => u.Email == user.Email);
        createdUser.EmailVerificationToken.Should().NotBeNullOrWhiteSpace();

        var response = await HttpClient.GetAsync($"/api/auth/activate?token={createdUser.EmailVerificationToken}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await DbContext.Users.AsNoTracking().SingleAsync(u => u.Email == user.Email);
        updatedUser.IsEmailVerified.Should().BeTrue();
        updatedUser.EmailVerificationToken.Should().BeNull();
    }

    [Fact]
    public async Task VerifyEmail_WithInvalidToken_ReturnsBadRequest()
    {
        var response = await HttpClient.GetAsync("/api/auth/activate?token=invalid-token");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ForgetAndResetPassword_WithValidToken_ResetsPasswordSuccessfully()
    {
        var user = IntegrationTestHelper.CreateUser("auth-reset");
        await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, user);

        var forgetResponse = await HttpClient.PostAsJsonAsync("/api/auth/forget-password", new
        {
            email = user.Email
        });

        forgetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbUser = await DbContext.Users.SingleAsync(u => u.Email == user.Email);
        dbUser.PasswordResetToken.Should().NotBeNullOrWhiteSpace();

        var verifyTokenResponse = await HttpClient.GetAsync($"/api/auth/reset-password?token={dbUser.PasswordResetToken}");
        verifyTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var isTokenValid = await verifyTokenResponse.Content.ReadFromJsonAsync<bool>();
        isTokenValid.Should().BeTrue();

        var newPassword = "BrandNewPass!123";
        var resetResponse = await HttpClient.PostAsJsonAsync("/api/auth/reset-password", new
        {
            token = dbUser.PasswordResetToken,
            newPassword
        });

        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await IntegrationTestHelper.LoginAndReadAsync(HttpClient, user.Email, newPassword);
        loginResponse.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ChangePassword_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await HttpClient.PostAsJsonAsync("/api/auth/change-password", new
        {
            oldPassword = "OldPass!123",
            newPassword = "NewPass!123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithWrongOldPassword_ReturnsBadRequest()
    {
        var user = IntegrationTestHelper.CreateUser("auth-change-wrong");
        var registered = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, user);

        var response = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            "/api/auth/change-password",
            registered.Token,
            new
            {
                oldPassword = "WrongOldPassword!000",
                newPassword = "AnotherNewPass!123"
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_WithValidPayload_UpdatesPassword()
    {
        var user = IntegrationTestHelper.CreateUser("auth-change-ok");
        var registered = await IntegrationTestHelper.RegisterAndReadAsync(HttpClient, user);

        var newPassword = "UpdatedPassword!456";

        var changeResponse = await IntegrationTestHelper.SendAuthorizedJsonAsync(
            HttpClient,
            HttpMethod.Post,
            "/api/auth/change-password",
            registered.Token,
            new
            {
                oldPassword = user.Password,
                newPassword
            });

        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await IntegrationTestHelper.LoginAndReadAsync(HttpClient, user.Email, newPassword);
        login.Token.Should().NotBeNullOrWhiteSpace();
    }
}
