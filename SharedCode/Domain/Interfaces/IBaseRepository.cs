namespace SharedCode.Domain.Interfaces
{
    public interface IBaseRepository<T>
    {
        Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
        Task<Boolean> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<Boolean> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<Boolean> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<Boolean> DisableAsync(T entity, CancellationToken cancellationToken = default);
    }
}
