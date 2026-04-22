using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Auth.Commands.Login;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class LoginCommandHandlerTests
{
    private readonly LoginCommandHandler _sut;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtService = Substitute.For<IJwtService>();

        _unitOfWork.Users.Returns(_users);
        _sut = new LoginCommandHandler(_unitOfWork, _passwordHasher, _jwtService);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsInvalidCredentials()
    {
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns((User?)null);

        var result = await _sut.Handle(new LoginCommand("user@example.com", "StrongPass123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ReturnsInvalidCredentials()
    {
        var user = AuthUserTestData.Entity(email: "user@example.com", password: "HASHED");
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns(user);
        _passwordHasher.VerifyPassword("wrong", "HASHED").Returns(false);

        var result = await _sut.Handle(new LoginCommand("user@example.com", "wrong"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenValid_ReturnsAuthenticatedUserAndUpdatesRefreshToken()
    {
        var user = AuthUserTestData.Entity(email: "user@example.com", password: "HASHED", isEmailVerified: true);
        user.Avatar = new Avatar { Url = "https://cdn/avatar.jpg" };

        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns(user);
        _passwordHasher.VerifyPassword("StrongPass123", "HASHED").Returns(true);

        var jwt = AuthUserTestData.Jwt(token: "jwt-token", refreshToken: "new-refresh");
        _jwtService.GenerateToken(user).Returns(jwt);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new LoginCommand("user@example.com", "StrongPass123"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("user@example.com");
        result.Value.IsEmailVerified.Should().BeTrue();
        result.Value.Token.Should().Be("jwt-token");
        result.Value.RefreshToken.Should().Be("new-refresh");
        result.Value.AvatarUrl.Should().Be("https://cdn/avatar.jpg");

        user.RefreshToken.Should().Be("new-refresh");
        user.RefreshTokenExpiryTime.Should().Be(jwt.RefreshTokenExpirationDate);
        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

