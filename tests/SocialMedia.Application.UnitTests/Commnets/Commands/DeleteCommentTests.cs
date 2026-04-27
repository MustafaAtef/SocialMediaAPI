using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Comments.Commands.Delete;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Comments;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

[Trait("Category", "Unit")]
public class DeleteCommentCommandHandlerTests
{
    private readonly DeleteCommentCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;
    private readonly ICommentRepository _comments;

    public DeleteCommentCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();
        _comments = Substitute.For<ICommentRepository>();

        _unitOfWork.Posts.Returns(_posts);
        _unitOfWork.Comments.Returns(_comments);
        _sut = new DeleteCommentCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new DeleteCommentCommand(1, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ReturnsPostNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns((Post?)null);

        var result = await _sut.Handle(new DeleteCommentCommand(1, 2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ReturnsCommentNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(new Post { Id = 1 });
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>()).Returns((Comment?)null);

        var result = await _sut.Handle(new DeleteCommentCommand(1, 2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUnauthorized_ReturnsUnauthorizedError()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 9));
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(new Post { Id = 1 });
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>())
            .Returns(new Comment { Id = 2, PostId = 1, UserId = 3 });

        var result = await _sut.Handle(new DeleteCommentCommand(1, 2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenCommentHasReplies_RemovesRepliesAndComment()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 3));

        var post = new Post { Id = 1, CommentsCount = 10 };
        var replies = new List<Comment>
        {
            new() { Id = 7, PostId = 1, ParentCommentId = 2 },
            new() { Id = 8, PostId = 1, ParentCommentId = 2 }
        };
        var comment = new Comment
        {
            Id = 2,
            PostId = 1,
            UserId = 3,
            RepliesCount = 2,
            Replies = replies
        };

        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(post);
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>()).Returns(comment);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(3));

        var result = await _sut.Handle(new DeleteCommentCommand(1, 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _comments.Received(1).RemoveRange(replies);
        _comments.Received(1).Remove(comment);
        post.CommentsCount.Should().Be(7);

        var domainEvent = post.GetDomainEvents().OfType<CommentDeletedDomainEvent>().Single();
        domainEvent.CommentId.Should().Be(2);
        domainEvent.PostId.Should().Be(1);
        domainEvent.RepliesCount.Should().Be(2);

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenCommentHasNoReplies_DecrementsParentRepliesCount()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 3));

        var post = new Post { Id = 1, CommentsCount = 5 };
        var targetComment = new Comment
        {
            Id = 4,
            PostId = 1,
            UserId = 3,
            ParentCommentId = 1,
            RepliesCount = 0,
            Replies = new List<Comment>()
        };
        var parentComment = new Comment { Id = 1, PostId = 1, RepliesCount = 3 };

        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(post);
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>())
            .Returns(targetComment, parentComment);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new DeleteCommentCommand(1, 4), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        parentComment.RepliesCount.Should().Be(2);
        _comments.Received(1).Remove(targetComment);
        post.CommentsCount.Should().Be(4);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesReturnsZero_ReturnsDeleteFailed()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 3));

        var post = new Post { Id = 1, CommentsCount = 2 };
        var comment = new Comment { Id = 4, PostId = 1, UserId = 3, RepliesCount = 0, Replies = null };

        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(post);
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>()).Returns(comment, (Comment?)null);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(0));

        var result = await _sut.Handle(new DeleteCommentCommand(1, 4), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.DeleteFailed);
    }
}

