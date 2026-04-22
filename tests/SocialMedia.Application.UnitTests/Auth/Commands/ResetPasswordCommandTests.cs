using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Auth.Commands.ResetPassword;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class ResetPasswordCommandHandlerTests
{
    private readonly ResetPasswordCommandHandler _sut;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();

        _unitOfWork.Users.Returns(_users);
        _sut = new ResetPasswordCommandHandler(_unitOfWork, _passwordHasher);
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundByToken_ReturnsInvalidOrExpiredToken()
    {
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns((User?)null);

        var result = await _sut.Handle(new ResetPasswordCommand("token", "newpass12345"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidOrExpiredToken);
    }

    [Fact]
    public async Task Handle_WhenTokenExpired_ReturnsInvalidOrExpiredToken()
    {
        var user = AuthUserTestData.Entity();
        user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1);

        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(user);

        var result = await _sut.Handle(new ResetPasswordCommand("token", "newpass12345"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidOrExpiredToken);
    }

    [Fact]
    public async Task Handle_WhenValid_ResetsPasswordAndClearsResetToken()
    {
        var user = AuthUserTestData.Entity(password: "OLD_HASH");
        user.PasswordResetToken = "token";
        user.PasswordResetTokenExpiryTime = DateTime.Now.AddMinutes(10);

        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(user);
        _passwordHasher.HashPassword("newpass12345").Returns("NEW_HASH");
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new ResetPasswordCommand("token", "newpass12345"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.Password.Should().Be("NEW_HASH");
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiryTime.Should().BeNull();
        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

