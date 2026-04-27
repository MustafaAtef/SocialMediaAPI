using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Users.Commands.Follow;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.UserFollows;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

[Trait("Category", "Unit")]
public class FollowUserCommandHandlerTests
{
    private readonly FollowUserCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;
    private readonly IFollowerFollowingRepository _followersFollowings;

    public FollowUserCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();
        _followersFollowings = Substitute.For<IFollowerFollowingRepository>();

        _unitOfWork.Users.Returns(_users);
        _unitOfWork.FollowersFollowings.Returns(_followersFollowings);
        _sut = new FollowUserCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new FollowUserCommand(2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenSelfFollow_ReturnsSelfFollowError()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 2));

        var result = await _sut.Handle(new FollowUserCommand(2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.SelfFollow);
    }

    [Fact]
    public async Task Handle_WhenEitherUserNotFound_ReturnsNotFoundError()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 1));
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns((User?)null, AuthUserTestData.Entity(id: 1));

        var result = await _sut.Handle(new FollowUserCommand(2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAlreadyFollowing_ReturnsAlreadyFollowingError()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 1));

        var followingUser = AuthUserTestData.Entity(id: 2);
        var followerUser = AuthUserTestData.Entity(id: 1);

        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(followingUser, followerUser);
        _followersFollowings.GetAsync(Arg.Any<Expression<Func<FollowerFollowing, bool>>>(), Arg.Any<string[]?>())
            .Returns(new FollowerFollowing { FollowerId = 1, FollowingId = 2 });

        var result = await _sut.Handle(new FollowUserCommand(2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.AlreadyFollowing);
    }

    [Fact]
    public async Task Handle_WhenValid_CreatesFollowAndRaisesEvent()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 1));

        var followingUser = AuthUserTestData.Entity(id: 2, firstName: "Target", lastName: "User", email: "target@example.com");
        followingUser.Avatar = new Avatar { Url = "https://cdn/following.jpg" };

        var followerUser = AuthUserTestData.Entity(id: 1, firstName: "Follower", lastName: "User", email: "follower@example.com");
        followerUser.Avatar = new Avatar { Url = "https://cdn/follower.jpg" };

        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(followingUser, followerUser);
        _followersFollowings.GetAsync(Arg.Any<Expression<Func<FollowerFollowing, bool>>>(), Arg.Any<string[]?>())
            .Returns((FollowerFollowing?)null);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        FollowerFollowing? addedFollow = null;
        _followersFollowings.When(x => x.Add(Arg.Any<FollowerFollowing>())).Do(ci => addedFollow = ci.Arg<FollowerFollowing>());

        var result = await _sut.Handle(new FollowUserCommand(2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        addedFollow.Should().NotBeNull();
        addedFollow!.FollowerId.Should().Be(1);
        addedFollow.FollowingId.Should().Be(2);

        followerUser.FollowingCount.Should().Be(1);
        followingUser.FollowersCount.Should().Be(1);

        var domainEvent = addedFollow.GetDomainEvents().OfType<UserFollowedDomainEvent>().Single();
        domainEvent.FollowerId.Should().Be(1);
        domainEvent.FollowingId.Should().Be(2);

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

