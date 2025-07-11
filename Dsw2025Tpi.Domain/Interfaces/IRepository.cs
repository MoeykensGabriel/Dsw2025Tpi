using Dsw2025Tpi.Domain.Entities;
using System.Linq.Expressions;

namespace Dsw2025Tpi.Domain.Interfaces;

public interface IRepository
{
    Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase;
    Task<IEnumerable<T>?> GetAll<T>(params string[] include) where T : EntityBase;
    Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase;
    Task<IEnumerable<T>?> GetFiltered<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase;
    Task<T> Add<T>(T entity) where T : EntityBase;
    Task<T> Update<T>(T entity) where T : EntityBase;
    Task<T> Delete<T>(T entity) where T : EntityBase;

    Task<IEnumerable<T>> Where<T>(Expression<Func<T, bool>> predicate) where T : class;
}
