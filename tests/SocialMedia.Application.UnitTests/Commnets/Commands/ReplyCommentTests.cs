using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Comments.Commands.Reply;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Comments;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class ReplyCommentCommandHandlerTests
{
    private readonly ReplyCommentCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;
    private readonly ICommentRepository _comments;

    public ReplyCommentCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();
        _comments = Substitute.For<ICommentRepository>();

        _unitOfWork.Posts.Returns(_posts);
        _unitOfWork.Comments.Returns(_comments);
        _sut = new ReplyCommentCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new ReplyCommentCommand(1, 2, "reply"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ReturnsPostNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns((Post?)null);

        var result = await _sut.Handle(new ReplyCommentCommand(10, 2, "reply"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenParentCommentNotFound_ReturnsParentNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(new Post { Id = 10 });
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>()).Returns((Comment?)null);

        var result = await _sut.Handle(new ReplyCommentCommand(10, 33, "reply"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.ParentNotFound);
    }

    [Fact]
    public async Task Handle_WhenParentIsReply_ReturnsReplyOnReplyNotAllowed()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(new Post { Id = 1 });
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>())
            .Returns(new Comment { Id = 2, PostId = 1, ParentCommentId = 5 });

        var result = await _sut.Handle(new ReplyCommentCommand(1, 2, "reply"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.ReplyOnReplyNotAllowed);
    }

    [Fact]
    public async Task Handle_WhenValid_CreatesReplyAndReturnsResponse()
    {
        var user = CommentUserTestData.AuthenticatedUser(id: 9);
        _userService.GetAuthenticatedUser().Returns(user);

        var post = new Post { Id = 4, CommentsCount = 6 };
        var parentComment = new Comment { Id = 20, PostId = 4, ParentCommentId = null, RepliesCount = 2 };

        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(post);
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>()).Returns(parentComment);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        Comment? addedReply = null;
        _comments.When(x => x.Add(Arg.Any<Comment>())).Do(ci => addedReply = ci.Arg<Comment>());

        var result = await _sut.Handle(new ReplyCommentCommand(4, 20, "my reply"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PostId.Should().Be(4);
        result.Value.ParentCommentId.Should().Be(20);
        result.Value.Content.Should().Be("my reply");
        result.Value.CreatedBy.Id.Should().Be(9);

        parentComment.RepliesCount.Should().Be(3);
        post.CommentsCount.Should().Be(7);

        addedReply.Should().NotBeNull();
        addedReply!.ParentCommentId.Should().Be(20);
        addedReply.PostId.Should().Be(4);

        var domainEvent = addedReply.GetDomainEvents().OfType<CommentCreatedDomainEvent>().Single();
        domainEvent.ParentCommentId.Should().Be(20);
        domainEvent.PostId.Should().Be(4);

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

