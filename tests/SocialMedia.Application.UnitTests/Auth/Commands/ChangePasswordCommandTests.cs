using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Auth.Commands.ChangePassword;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class ChangePasswordCommandHandlerTests
{
    private readonly ChangePasswordCommandHandler _sut;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserService _userService;

    public ChangePasswordCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _userService = Substitute.For<IUserService>();

        _unitOfWork.Users.Returns(_users);
        _sut = new ChangePasswordCommandHandler(_unitOfWork, _passwordHasher, _userService);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new ChangePasswordCommand("old", "newpassword123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsUserNotFoundError()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 2));
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns((User?)null);

        var result = await _sut.Handle(new ChangePasswordCommand("old", "newpassword123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenOldPasswordIncorrect_ReturnsIncorrectOldPasswordError()
    {
        var user = AuthUserTestData.Entity(id: 2, password: "HASHED");
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 2));
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns(user);
        _passwordHasher.VerifyPassword("old", "HASHED").Returns(false);

        var result = await _sut.Handle(new ChangePasswordCommand("old", "newpassword123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.IncorrectOldPassword);
    }

    [Fact]
    public async Task Handle_WhenValid_ChangesPasswordAndSaves()
    {
        var user = AuthUserTestData.Entity(id: 2, password: "HASHED");
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 2));
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns(user);
        _passwordHasher.VerifyPassword("old", "HASHED").Returns(true);
        _passwordHasher.HashPassword("newpassword123").Returns("NEW_HASH");

        var result = await _sut.Handle(new ChangePasswordCommand("old", "newpassword123"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.Password.Should().Be("NEW_HASH");
        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

