using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Users.Commands.Follow;

public class FollowUserCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<FollowUserCommand>
{
    public async Task<Result> Handle(FollowUserCommand request, CancellationToken cancellationToken)
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
        if (existingFollow != null)
            return Result.Failure(UserErrors.AlreadyFollowing);

        unitOfWork.FollowersFollowings.Add(new FollowerFollowing
        {
            FollowerId = followerUser.Id,
            FollowingId = followingUser.Id
        });
        followerUser.FollowingCount++;
        followingUser.FollowersCount++;
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
