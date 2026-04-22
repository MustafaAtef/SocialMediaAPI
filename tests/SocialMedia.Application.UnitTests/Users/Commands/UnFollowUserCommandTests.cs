using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Users.Commands.UnFollow;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.UserFollows;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class UnFollowUserCommandHandlerTests
{
    private readonly UnFollowUserCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;
    private readonly IFollowerFollowingRepository _followersFollowings;

    public UnFollowUserCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();
        _followersFollowings = Substitute.For<IFollowerFollowingRepository>();

        _unitOfWork.Users.Returns(_users);
        _unitOfWork.FollowersFollowings.Returns(_followersFollowings);
        _sut = new UnFollowUserCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new UnFollowUserCommand(2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenSelfUnfollow_ReturnsSelfFollowError()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 2));

        var result = await _sut.Handle(new UnFollowUserCommand(2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.SelfFollow);
    }

    [Fact]
    public async Task Handle_WhenEitherUserNotFound_ReturnsNotFoundError()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 1));
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns((User?)null, AuthUserTestData.Entity(id: 1));

        var result = await _sut.Handle(new UnFollowUserCommand(2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNotFollowing_ReturnsNotFollowingError()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 1));

        var followingUser = AuthUserTestData.Entity(id: 2);
        var followerUser = AuthUserTestData.Entity(id: 1);
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(followingUser, followerUser);
        _followersFollowings.GetAsync(Arg.Any<Expression<Func<FollowerFollowing, bool>>>(), Arg.Any<string[]?>())
            .Returns((FollowerFollowing?)null);

        var result = await _sut.Handle(new UnFollowUserCommand(2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFollowing);
    }

    [Fact]
    public async Task Handle_WhenValid_RemovesFollowAndRaisesEvent()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 1));

        var followingUser = AuthUserTestData.Entity(id: 2);
        followingUser.FollowersCount = 3;

        var followerUser = AuthUserTestData.Entity(id: 1);
        followerUser.FollowingCount = 4;

        var existingFollow = new FollowerFollowing { FollowerId = 1, FollowingId = 2 };

        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(followingUser, followerUser);
        _followersFollowings.GetAsync(Arg.Any<Expression<Func<FollowerFollowing, bool>>>(), Arg.Any<string[]?>())
            .Returns(existingFollow);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new UnFollowUserCommand(2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _followersFollowings.Received(1).Remove(existingFollow);
        followerUser.FollowingCount.Should().Be(3);
        followingUser.FollowersCount.Should().Be(2);

        var domainEvent = followerUser.GetDomainEvents().OfType<UserUnfollowedDomainEvent>().Single();
        domainEvent.FollowerId.Should().Be(1);
        domainEvent.FollowingId.Should().Be(2);

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

