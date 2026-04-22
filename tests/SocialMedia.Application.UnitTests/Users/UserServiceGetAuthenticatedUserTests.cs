using System.Security.Claims;

using FluentAssertions;

using Microsoft.AspNetCore.Http;

using NSubstitute;

using SocialMedia.Application.Services;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class UserServiceGetAuthenticatedUserTests
{
    private static UserService CreateSut(IHttpContextAccessor accessor)
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var fileUploader = Substitute.For<IFileUploader>();
        return new UserService(accessor, unitOfWork, fileUploader);
    }

    [Fact]
    public void GetAuthenticatedUser_WhenNoClaimsPrincipalAndNoHttpContext_ThrowsNullReferenceException()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var sut = CreateSut(accessor);

        var act = () => sut.GetAuthenticatedUser();

        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void GetAuthenticatedUser_WhenProvidedClaimsPrincipal_UsesItInsteadOfHttpContext()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(AuthUserTestData.PrincipalWithUserId(100, "other@example.com", "Other User", "https://cdn/other.jpg"));
        accessor.HttpContext.Returns(httpContext);

        var sut = CreateSut(accessor);
        var providedPrincipal = AuthUserTestData.PrincipalWithUserId(5, "user@example.com", "Mostafa", "https://cdn/avatar.jpg");

        var result = sut.GetAuthenticatedUser(providedPrincipal);

        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
        result.Email.Should().Be("user@example.com");
        result.Name.Should().Be("Mostafa");
        result.AvatarUrl.Should().Be("https://cdn/avatar.jpg");
    }

    [Fact]
    public void GetAuthenticatedUser_WhenIdClaimMissing_ReturnsNull()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("name", "Mostafa")
        }, "Test")));
        accessor.HttpContext.Returns(httpContext);

        var sut = CreateSut(accessor);

        var result = sut.GetAuthenticatedUser();

        result.Should().BeNull();
    }

    [Fact]
    public void GetAuthenticatedUser_WhenAvatarClaimMissing_ReturnsEmptyAvatarUrl()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "7"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("name", "Mostafa")
        }, "Test")));
        accessor.HttpContext.Returns(httpContext);

        var sut = CreateSut(accessor);

        var result = sut.GetAuthenticatedUser();

        result.Should().NotBeNull();
        result!.Id.Should().Be(7);
        result.AvatarUrl.Should().BeEmpty();
    }

    [Fact]
    public void GetAuthenticatedUser_WhenIdClaimNotNumeric_ThrowsFormatException()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "abc")
        }, "Test")));
        accessor.HttpContext.Returns(httpContext);

        var sut = CreateSut(accessor);

        var act = () => sut.GetAuthenticatedUser();

        act.Should().Throw<FormatException>();
    }
}
