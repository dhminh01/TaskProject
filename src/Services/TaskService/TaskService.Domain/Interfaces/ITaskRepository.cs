using TaskService.Domain.Entities;

namespace TaskService.Domain.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}