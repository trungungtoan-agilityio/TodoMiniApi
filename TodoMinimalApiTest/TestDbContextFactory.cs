using Microsoft.EntityFrameworkCore;
using TodoMinimalApi.Data;

namespace TodoMinimalApiTest;

public class TestDbContextFactory(string databaseName = "InMemoryTest") : IDbContextFactory<TodoDbContext>
{
    private readonly DbContextOptions<TodoDbContext> _options = new DbContextOptionsBuilder<TodoDbContext>()
        .UseInMemoryDatabase(databaseName)
        .Options;

    public TodoDbContext CreateDbContext()
    {
        var todoDbContext = new TodoDbContext(_options);
        todoDbContext.Database.EnsureCreated();
        return todoDbContext;
    }
}