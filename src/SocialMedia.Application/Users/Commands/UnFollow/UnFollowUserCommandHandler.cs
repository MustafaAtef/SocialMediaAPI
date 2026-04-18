using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.UserFollows;

namespace SocialMedia.Application.Users.Commands.UnFollow;

public class UnFollowUserCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<UnFollowUserCommand>
{
    public async Task<Result> Handle(UnFollowUserCommand request, CancellationToken cancellationToken)
    {
        var tokenUser = userService.GetAuthenticatedUser();
        if (tokenUser is null)
            return Result.Failure(UserErrors.Unauthenticated);

        if (request.UserId == tokenUser.Id)
            return Result.Failure(UserErrors.SelfFollow);

        var followingUser = await unitOfWork.Users.GetAsync(u => u.Id == request.UserId);
        var followerUser = await unitOfWork.Users.GetAsync(u => u.Id == tokenUser.Id);
        if (followingUser is null || followerUser is null)
            return Result.Failure(UserErrors.NotFound);

        var existingFollow = await unitOfWork.FollowersFollowings.GetAsync(
            ff => ff.FollowerId == followerUser.Id && ff.FollowingId == followingUser.Id);
        if (existingFollow is null)
            return Result.Failure(UserErrors.NotFollowing);

        unitOfWork.FollowersFollowings.Remove(existingFollow);
        followerUser.FollowingCount--;
        followingUser.FollowersCount--;
        followerUser.RaiseDomainEvent(() => new UserUnfollowedDomainEvent(existingFollow.FollowerId, existingFollow.FollowingId));
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
