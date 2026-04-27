using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Reacts.Commands.RemoveCommentReact;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.CommentReacts;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

[Trait("Category", "Unit")]
public class RemoveCommentReactCommandHandlerTests
{
    private readonly RemoveCommentReactCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICommentRepository _comments;
    private readonly ICommentReactRepository _commentReacts;

    public RemoveCommentReactCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _comments = Substitute.For<ICommentRepository>();
        _commentReacts = Substitute.For<ICommentReactRepository>();

        _unitOfWork.Comments.Returns(_comments);
        _unitOfWork.CommentReacts.Returns(_commentReacts);
        _sut = new RemoveCommentReactCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new RemoveCommentReactCommand(1, 2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ReturnsCommentNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>()).Returns((Comment?)null);

        var result = await _sut.Handle(new RemoveCommentReactCommand(1, 2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenReactNotFound_ReturnsCommentReactNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 8));
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>()).Returns(new Comment { Id = 2, PostId = 1 });
        _commentReacts.GetAsync(Arg.Any<Expression<Func<CommentReact, bool>>>(), Arg.Any<string[]?>()).Returns((CommentReact?)null);

        var result = await _sut.Handle(new RemoveCommentReactCommand(1, 2), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReactErrors.CommentReactNotFound);
    }

    [Fact]
    public async Task Handle_WhenValid_RemovesCommentReactAndRaisesEvent()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 8));

        var comment = new Comment { Id = 2, PostId = 1, ReactionsCount = 6 };
        var commentReact = new CommentReact { Id = 4, CommentId = 2, UserId = 8 };

        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>()).Returns(comment);
        _commentReacts.GetAsync(Arg.Any<Expression<Func<CommentReact, bool>>>(), Arg.Any<string[]?>()).Returns(commentReact);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new RemoveCommentReactCommand(1, 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        comment.ReactionsCount.Should().Be(5);
        _commentReacts.Received(1).Remove(commentReact);

        var domainEvent = comment.GetDomainEvents().OfType<CommentReactRemovedDomainEvent>().Single();
        domainEvent.Id.Should().Be(4);
        domainEvent.CommentId.Should().Be(2);

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

