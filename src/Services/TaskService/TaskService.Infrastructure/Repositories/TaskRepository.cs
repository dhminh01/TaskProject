using Microsoft.EntityFrameworkCore;
using TaskService.Domain.Entities;
using TaskService.Domain.Interfaces;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext _context;

    public TaskRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems
            .OrderBy(t => t.DateCreated)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _context.TaskItems.AddAsync(task, cancellationToken);
    }

    public Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.TaskItems.Update(task);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.TaskItems.Remove(task);
        return Task.CompletedTask;
    }
}
