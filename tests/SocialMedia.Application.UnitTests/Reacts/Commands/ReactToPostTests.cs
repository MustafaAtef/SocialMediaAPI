using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Reacts.Commands.ReactToPost;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.PostReacts;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class ReactToPostCommandHandlerTests
{
    private readonly ReactToPostCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;
    private readonly IPostReactRepository _postReacts;

    public ReactToPostCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();
        _postReacts = Substitute.For<IPostReactRepository>();

        _unitOfWork.Posts.Returns(_posts);
        _unitOfWork.PostReacts.Returns(_postReacts);
        _sut = new ReactToPostCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new ReactToPostCommand(1, ReactType.Like), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ReturnsPostNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _posts.GetByIdAsync(10).Returns((Post?)null);

        var result = await _sut.Handle(new ReactToPostCommand(10, ReactType.Love), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenReactExists_UpdatesReactType()
    {
        var user = CommentUserTestData.AuthenticatedUser(id: 4);
        _userService.GetAuthenticatedUser().Returns(user);

        var post = new Post { Id = 2, ReactionsCount = 3 };
        var existingReact = new PostReact { Id = 9, PostId = 2, UserId = 4, ReactType = ReactType.Like, CreatedAt = DateTime.UtcNow.AddDays(-1) };

        _posts.GetByIdAsync(2).Returns(post);
        _postReacts.GetAsync(Arg.Any<Expression<Func<PostReact, bool>>>(), Arg.Any<string[]?>()).Returns(existingReact);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new ReactToPostCommand(2, ReactType.Wow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(9);
        result.Value.PostId.Should().Be(2);
        result.Value.TypeNo.Should().Be(ReactType.Wow);
        result.Value.TypeName.Should().Be("Wow");

        existingReact.ReactType.Should().Be(ReactType.Wow);
        post.ReactionsCount.Should().Be(3);

        var domainEvent = existingReact.GetDomainEvents().OfType<PostReactUpdatedDomainEvent>().Single();
        domainEvent.Id.Should().Be(9);
        domainEvent.ReactType.Should().Be(ReactType.Wow);

        _postReacts.DidNotReceive().Add(Arg.Any<PostReact>());
        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenReactDoesNotExist_AddsReactAndIncrementsCount()
    {
        var user = CommentUserTestData.AuthenticatedUser(id: 5);
        _userService.GetAuthenticatedUser().Returns(user);

        var post = new Post { Id = 3, ReactionsCount = 0 };

        _posts.GetByIdAsync(3).Returns(post);
        _postReacts.GetAsync(Arg.Any<Expression<Func<PostReact, bool>>>(), Arg.Any<string[]?>()).Returns((PostReact?)null);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        PostReact? addedReact = null;
        _postReacts.When(x => x.Add(Arg.Any<PostReact>())).Do(ci => addedReact = ci.Arg<PostReact>());

        var result = await _sut.Handle(new ReactToPostCommand(3, ReactType.Love), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PostId.Should().Be(3);
        result.Value.TypeNo.Should().Be(ReactType.Love);
        result.Value.ReactedBy.Id.Should().Be(5);

        post.ReactionsCount.Should().Be(1);

        addedReact.Should().NotBeNull();
        addedReact!.PostId.Should().Be(3);
        addedReact.UserId.Should().Be(5);
        addedReact.ReactType.Should().Be(ReactType.Love);

        var domainEvent = addedReact.GetDomainEvents().OfType<PostReactAddedDomainEvent>().Single();
        domainEvent.PostId.Should().Be(3);
        domainEvent.UserId.Should().Be(5);
        domainEvent.ReactType.Should().Be(ReactType.Love);
    }
}

