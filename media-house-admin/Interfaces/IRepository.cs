namespace MediaHouse.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<List<T>> FindAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate);
}
