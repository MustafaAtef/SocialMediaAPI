using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Reacts.Commands.ReactToComment;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.CommentReacts;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class ReactToCommentCommandHandlerTests
{
    private readonly ReactToCommentCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;
    private readonly ICommentRepository _comments;
    private readonly ICommentReactRepository _commentReacts;

    public ReactToCommentCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();
        _comments = Substitute.For<ICommentRepository>();
        _commentReacts = Substitute.For<ICommentReactRepository>();

        _unitOfWork.Posts.Returns(_posts);
        _unitOfWork.Comments.Returns(_comments);
        _unitOfWork.CommentReacts.Returns(_commentReacts);
        _sut = new ReactToCommentCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new ReactToCommentCommand(1, 2, ReactType.Love), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ReturnsPostNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _posts.GetByIdAsync(1).Returns((Post?)null);

        var result = await _sut.Handle(new ReactToCommentCommand(1, 2, ReactType.Like), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ReturnsCommentNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _posts.GetByIdAsync(1).Returns(new Post { Id = 1 });
        _comments.GetByIdAsync(2).Returns((Comment?)null);

        var result = await _sut.Handle(new ReactToCommentCommand(1, 2, ReactType.Like), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenReactExists_UpdatesReactType()
    {
        var user = CommentUserTestData.AuthenticatedUser(id: 4);
        _userService.GetAuthenticatedUser().Returns(user);

        var post = new Post { Id = 1 };
        var comment = new Comment { Id = 2, ReactionsCount = 3 };
        var existingReact = new CommentReact { Id = 8, CommentId = 2, UserId = 4, ReactType = ReactType.Sad, CreatedAt = DateTime.UtcNow.AddDays(-1) };

        _posts.GetByIdAsync(1).Returns(post);
        _comments.GetByIdAsync(2).Returns(comment);
        _commentReacts.GetAsync(Arg.Any<Expression<Func<CommentReact, bool>>>(), Arg.Any<string[]?>()).Returns(existingReact);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new ReactToCommentCommand(1, 2, ReactType.Angry), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CommentId.Should().Be(2);
        result.Value.TypeNo.Should().Be(ReactType.Angry);
        result.Value.TypeName.Should().Be("Angry");

        existingReact.ReactType.Should().Be(ReactType.Angry);
        comment.ReactionsCount.Should().Be(3);

        var domainEvent = existingReact.GetDomainEvents().OfType<CommentReactUpdatedDomainEvent>().Single();
        domainEvent.Id.Should().Be(8);
        domainEvent.ReactType.Should().Be(ReactType.Angry);

        _commentReacts.DidNotReceive().Add(Arg.Any<CommentReact>());
    }

    [Fact]
    public async Task Handle_WhenReactDoesNotExist_AddsReactAndIncrementsCount()
    {
        var user = CommentUserTestData.AuthenticatedUser(id: 11);
        _userService.GetAuthenticatedUser().Returns(user);

        var post = new Post { Id = 1 };
        var comment = new Comment { Id = 3, ReactionsCount = 1 };

        _posts.GetByIdAsync(1).Returns(post);
        _comments.GetByIdAsync(3).Returns(comment);
        _commentReacts.GetAsync(Arg.Any<Expression<Func<CommentReact, bool>>>(), Arg.Any<string[]?>()).Returns((CommentReact?)null);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        CommentReact? addedReact = null;
        _commentReacts.When(x => x.Add(Arg.Any<CommentReact>())).Do(ci => addedReact = ci.Arg<CommentReact>());

        var result = await _sut.Handle(new ReactToCommentCommand(1, 3, ReactType.Wow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CommentId.Should().Be(3);
        result.Value.TypeNo.Should().Be(ReactType.Wow);
        result.Value.ReactedBy.Id.Should().Be(11);

        comment.ReactionsCount.Should().Be(2);

        addedReact.Should().NotBeNull();
        addedReact!.CommentId.Should().Be(3);
        addedReact.UserId.Should().Be(11);

        var domainEvent = addedReact.GetDomainEvents().OfType<CommentReactAddedDomainEvent>().Single();
        domainEvent.CommentId.Should().Be(3);
        domainEvent.UserId.Should().Be(11);
        domainEvent.ReactType.Should().Be(ReactType.Wow);
    }
}

