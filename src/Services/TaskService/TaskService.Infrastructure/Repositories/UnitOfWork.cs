
using TaskService.Domain.Interfaces;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TaskDbContext _context;

    public UnitOfWork(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}