using System.Linq.Expressions;

namespace Ecommerce.Repositories
{
    public interface IEntityRepository<T, TKey> where T : class
    {
        List<T> GetAll();
        T? GetById(TKey id);
        void Add(T entity);
        void Update(T entity);
        void Delete(TKey id);
        List<T> FindBy(Expression<Func<T, bool>> predicate);
        void SaveChanges();
    }
}
