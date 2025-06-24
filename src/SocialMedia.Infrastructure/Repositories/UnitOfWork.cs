

using EducationCenter.Core.RepositoryContracts;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;
using SocialMedia.Infrastructure.Repositories;

namespace EducationCenterAPI.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _appDbContext;
    public UnitOfWork(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    private IUserRepository? _users = null;
    private IPostRepository? _posts = null;
    private ICommentRepository? _comments = null;
    private IPostReactRepository? _postReacts = null;
    private ICommentReactRepository? _commentReacts = null;
    public IUserRepository Users
    {
        get
        {
            if (_users == null)
                _users = new UserRepository(_appDbContext);
            return _users;
        }
    }

    public IPostRepository Posts
    {
        get
        {
            if (_posts == null)
                _posts = new PostRepository(_appDbContext);
            return _posts;
        }
    }

    public ICommentRepository Comments
    {
        get
        {
            if (_comments == null)
                _comments = new CommentRepository(_appDbContext);
            return _comments;
        }
    }

    public IPostReactRepository PostReacts
    {
        get
        {
            if (_postReacts == null)
                _postReacts = new PostReactRepository(_appDbContext);
            return _postReacts;
        }
    }

    public ICommentReactRepository CommentReacts
    {
        get
        {
            if (_commentReacts == null)
                _commentReacts = new CommentReactRepository(_appDbContext);
            return _commentReacts;
        }
    }
    public void Dispose()
    {
        _appDbContext.Dispose();
    }

    public Task SaveChangesAsync()
    {
        return _appDbContext.SaveChangesAsync();
    }
}
