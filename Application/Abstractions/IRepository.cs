namespace BimAiAssistant.Application.Abstractions;

public interface IRepository<TEntity>
{
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
}
