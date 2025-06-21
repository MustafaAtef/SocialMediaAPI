using System;
using System.Linq.Expressions;

namespace SocialMedia.Core.RepositoryContracts;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id);
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, string[]? includes = null);
    Task<List<TEntity>> GetAllAsync();
    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate, string[]? includes = null);
    Task<List<TEntity>> GetAllAsync(int page, int pageSize, Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, object>>? orderKeySelector, string? sortOrder, string[]? includes = null);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate);
    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}