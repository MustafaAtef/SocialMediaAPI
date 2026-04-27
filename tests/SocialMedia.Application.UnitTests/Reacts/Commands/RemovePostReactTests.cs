using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Reacts.Commands.RemovePostReact;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.PostReacts;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

[Trait("Category", "Unit")]
public class RemovePostReactCommandHandlerTests
{
    private readonly RemovePostReactCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;
    private readonly IPostReactRepository _postReacts;

    public RemovePostReactCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();
        _postReacts = Substitute.For<IPostReactRepository>();

        _unitOfWork.Posts.Returns(_posts);
        _unitOfWork.PostReacts.Returns(_postReacts);
        _sut = new RemovePostReactCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new RemovePostReactCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ReturnsPostNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _posts.GetByIdAsync(1).Returns((Post?)null);

        var result = await _sut.Handle(new RemovePostReactCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenReactNotFound_ReturnsPostReactNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 6));
        _posts.GetByIdAsync(2).Returns(new Post { Id = 2 });
        _postReacts.GetAsync(Arg.Any<Expression<Func<PostReact, bool>>>(), Arg.Any<string[]?>()).Returns((PostReact?)null);

        var result = await _sut.Handle(new RemovePostReactCommand(2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReactErrors.PostReactNotFound);
    }

    [Fact]
    public async Task Handle_WhenReactBelongsToAnotherUser_ReturnsUnauthorized()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 7));
        _posts.GetByIdAsync(2).Returns(new Post { Id = 2 });
        _postReacts.GetAsync(Arg.Any<Expression<Func<PostReact, bool>>>(), Arg.Any<string[]?>())
            .Returns(new PostReact { Id = 4, PostId = 2, UserId = 99 });

        var result = await _sut.Handle(new RemovePostReactCommand(2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReactErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenValid_RemovesReactAndRaisesEvent()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 7));

        var post = new Post { Id = 2, ReactionsCount = 5 };
        var postReact = new PostReact { Id = 4, PostId = 2, UserId = 7 };

        _posts.GetByIdAsync(2).Returns(post);
        _postReacts.GetAsync(Arg.Any<Expression<Func<PostReact, bool>>>(), Arg.Any<string[]?>()).Returns(postReact);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new RemovePostReactCommand(2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        post.ReactionsCount.Should().Be(4);
        _postReacts.Received(1).Remove(postReact);

        var domainEvent = post.GetDomainEvents().OfType<PostReactRemovedDomainEvent>().Single();
        domainEvent.Id.Should().Be(4);
        domainEvent.PostId.Should().Be(2);

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

