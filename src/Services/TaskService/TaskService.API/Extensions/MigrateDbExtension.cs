using Microsoft.EntityFrameworkCore;
using TaskService.Infrastructure.Persistence;

namespace TaskService.API.Extensions
{
    public static class DataExtension
    {
        public static void MigrateDb(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            dbContext.Database.MigrateAsync().GetAwaiter().GetResult();
        }
    }
}
