using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Commands.PermanentDelete;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Posts;
using SocialMedia.Core.Exceptions;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests.Posts;

[Trait("Category", "Unit")]
public class PermanentDeletePostCommandHandlerTests
{
    private readonly PermanentDeletePostCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;
    private readonly IUserRepository _users;

    public PermanentDeletePostCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();
        _users = Substitute.For<IUserRepository>();

        _unitOfWork.Posts.Returns(_posts);
        _unitOfWork.Users.Returns(_users);

        _sut = new PermanentDeletePostCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((UserDto?)null);

        var result = await _sut.Handle(new PermanentDeletePostCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsUserNotFound()
    {
        var tokenUser = PostTestData.AuthenticatedUser(id: 10);
        _userService.GetAuthenticatedUser().Returns(tokenUser);
        _users.GetByIdAsync(tokenUser.Id).Returns((User?)null);

        var result = await _sut.Handle(new PermanentDeletePostCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ReturnsPostNotFound()
    {
        var tokenUser = PostTestData.AuthenticatedUser(id: 7);
        _userService.GetAuthenticatedUser().Returns(tokenUser);
        _users.GetByIdAsync(tokenUser.Id).Returns(new User { Id = tokenUser.Id });
        _posts.GetAsync(123).Returns((Post?)null);

        var result = await _sut.Handle(new PermanentDeletePostCommand(123), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ThrowsUnauthorized()
    {
        var tokenUser = PostTestData.AuthenticatedUser(id: 2);
        _userService.GetAuthenticatedUser().Returns(tokenUser);
        _users.GetByIdAsync(tokenUser.Id).Returns(new User { Id = tokenUser.Id });
        _posts.GetAsync(1).Returns(new Post { Id = 1, UserId = 9 });

        var act = () => _sut.Handle(new PermanentDeletePostCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<UnAuthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenRepositoryDeleteFails_ThrowsBadRequest()
    {
        var tokenUser = PostTestData.AuthenticatedUser(id: 3);
        var user = new User { Id = 3 };
        var post = new Post { Id = 50, UserId = 3 };

        _userService.GetAuthenticatedUser().Returns(tokenUser);
        _users.GetByIdAsync(tokenUser.Id).Returns(user);
        _posts.GetAsync(50).Returns(post);
        _posts.PermanentDeleteAsync(50).Returns(Task.FromResult(false));

        var act = () => _sut.Handle(new PermanentDeletePostCommand(50), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenValid_PermanentlyDeletesAndRaisesDomainEvent()
    {
        var tokenUser = PostTestData.AuthenticatedUser(id: 5);
        var user = new User { Id = 5 };
        var post = new Post { Id = 88, UserId = 5 };

        _userService.GetAuthenticatedUser().Returns(tokenUser);
        _users.GetByIdAsync(tokenUser.Id).Returns(user);
        _posts.GetAsync(88).Returns(post);
        _posts.PermanentDeleteAsync(88).Returns(Task.FromResult(true));

        var result = await _sut.Handle(new PermanentDeletePostCommand(88), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _ = _posts.Received(1).PermanentDeleteAsync(88);

        var domainEvent = user.GetDomainEvents().OfType<PostPermanentDeletedDomainEvent>().Single();
        domainEvent.PostId.Should().Be(88);
    }
}

