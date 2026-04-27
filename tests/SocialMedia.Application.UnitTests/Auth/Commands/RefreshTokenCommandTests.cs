using System.Linq.Expressions;
using System.Security.Claims;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Auth.Commands.RefreshToken;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

[Trait("Category", "Unit")]
public class RefreshTokenCommandHandlerTests
{
    private readonly RefreshTokenCommandHandler _sut;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();
        _jwtService = Substitute.For<IJwtService>();

        _unitOfWork.Users.Returns(_users);
        _sut = new RefreshTokenCommandHandler(_unitOfWork, _jwtService);
    }

    [Fact]
    public async Task Handle_WhenJwtInvalid_ReturnsInvalidRefreshTokenError()
    {
        _jwtService.ValidateJwt("token").Returns((ClaimsPrincipal?)null);

        var result = await _sut.Handle(new RefreshTokenCommand("token", "refresh"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsInvalidRefreshTokenError()
    {
        _jwtService.ValidateJwt("token").Returns(AuthUserTestData.PrincipalWithUserId(11));
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns((User?)null);

        var result = await _sut.Handle(new RefreshTokenCommand("token", "refresh"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenMismatched_ReturnsInvalidRefreshTokenError()
    {
        var user = AuthUserTestData.Entity(id: 1);
        user.RefreshToken = "stored-refresh";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1);

        _jwtService.ValidateJwt("token").Returns(AuthUserTestData.PrincipalWithUserId(1));
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns(user);

        var result = await _sut.Handle(new RefreshTokenCommand("token", "other-refresh"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenExpired_ReturnsInvalidRefreshTokenError()
    {
        var user = AuthUserTestData.Entity(id: 1);
        user.RefreshToken = "stored-refresh";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1);

        _jwtService.ValidateJwt("token").Returns(AuthUserTestData.PrincipalWithUserId(1));
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns(user);

        var result = await _sut.Handle(new RefreshTokenCommand("token", "stored-refresh"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ReturnsNewAuthenticatedUserResponse()
    {
        var user = AuthUserTestData.Entity(id: 3, isEmailVerified: true);
        user.RefreshToken = "stored-refresh";
        user.RefreshTokenExpiryTime = DateTime.Now.AddHours(1);
        user.Avatar = new Avatar { Url = "https://cdn/avatar.jpg" };

        _jwtService.ValidateJwt("token").Returns(AuthUserTestData.PrincipalWithUserId(3));
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns(user);

        var jwt = AuthUserTestData.Jwt(token: "new-jwt", refreshToken: "new-refresh");
        _jwtService.GenerateToken(user).Returns(jwt);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new RefreshTokenCommand("token", "stored-refresh"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(3);
        result.Value.Token.Should().Be("new-jwt");
        result.Value.RefreshToken.Should().Be("new-refresh");
        result.Value.AvatarUrl.Should().Be("https://cdn/avatar.jpg");

        user.RefreshToken.Should().Be("new-refresh");
        user.RefreshTokenExpiryTime.Should().Be(jwt.RefreshTokenExpirationDate);
        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

