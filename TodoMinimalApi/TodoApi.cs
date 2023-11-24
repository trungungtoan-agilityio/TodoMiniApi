using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoMinimalApi.Data;
using TodoMinimalApi.Models;
using TodoMinimalApi.ViewModels;

namespace TodoMinimalApi;

public static class TodoApi
{
    public static RouteGroupBuilder MapApiEndpoints(this RouteGroupBuilder groups)
    {
        groups.MapGet("/", GetAllTodoItems).Produces(200, typeof(PagedResults<TodoOutput>)).ProducesProblem(401).Produces(429);
        groups.MapGet("/{id}", GetTodoItemById).Produces(200, typeof(TodoOutput)).ProducesProblem(401).Produces(429);
        groups.MapPost("/", CreateTodoItem).Accepts<TodoInput>("application/json").Produces(201).ProducesProblem(401).ProducesProblem(400).Produces(429);
        groups.MapPut("/{id}", UpdateTodoItem).Accepts<TodoInput>("application/json").Produces(201).ProducesProblem(404).ProducesProblem(401).Produces(429);
        groups.MapDelete("/{id}", DeleteTodoItem).Produces(204).ProducesProblem(404).ProducesProblem(401).Produces(429);

        return groups;
    }

    public static async Task<IResult> GetAllTodoItems(IDbContextFactory<TodoDbContext> dbContextFactory, ClaimsPrincipal user, [FromQuery(Name = "page")] int? page = 1, [FromQuery(Name = "pageSize")] int? pageSize = 10)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        pageSize ??= 10;
        page ??= 1;

        var skipAmount = pageSize * (page - 1);
        var queryable = dbContext.TodoItems.Where(t => t.User.Username == user.FindFirst(ClaimTypes.NameIdentifier)!.Value).AsQueryable();
        var results = await queryable
            .Skip(skipAmount ?? 1)
            .Take(pageSize ?? 10).Select(t => new TodoOutput(t.Title, t.IsCompleted, t.CreatedOn)).ToListAsync();
        var totalNumberOfRecords = await queryable.CountAsync();
        var mod = totalNumberOfRecords % pageSize;
        var totalPageCount = (totalNumberOfRecords / pageSize) + (mod == 0 ? 0 : 1);

        return TypedResults.Ok(new PagedResults<TodoOutput>()
        {
            PageNumber = page.Value,
            PageSize = pageSize!.Value,
            Results = results,
            TotalNumberOfPages = totalPageCount!.Value,
            TotalNumberOfRecords = totalNumberOfRecords
        });
    }

    public static async Task<IResult> GetTodoItemById(IDbContextFactory<TodoDbContext> dbContextFactory, ClaimsPrincipal user, int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.TodoItems.FirstOrDefaultAsync(t => t.User.Username == user.FindFirst(ClaimTypes.NameIdentifier)!.Value && t.Id == id) is { } todo ? TypedResults.Ok(new TodoOutput(todo.Title, todo.IsCompleted, todo.CreatedOn)) : TypedResults.NotFound();
    }

    public static async Task<IResult> CreateTodoItem(IDbContextFactory<TodoDbContext> dbContextFactory, ClaimsPrincipal user, TodoInput todoItemInput, IValidator<TodoInput> todoItemInputValidator)
    {
        var validationResult = await todoItemInputValidator.ValidateAsync(todoItemInput);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var todoItem = new Todo
        {
            Title = todoItemInput.Title,
            IsCompleted = todoItemInput.IsCompleted,
        };

        var currentUser = await dbContext.Users.FirstOrDefaultAsync(t => t.Username == user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        todoItem.User = currentUser!;
        todoItem.UserId = currentUser!.Id;
        todoItem.CreatedOn = DateTime.UtcNow;
        dbContext.TodoItems.Add(todoItem);
        await dbContext.SaveChangesAsync();
        return TypedResults.Created($"/todos/{todoItem.Id}", new TodoOutput(todoItem.Title, todoItem.IsCompleted, todoItem.CreatedOn));
    }

    public static async Task<IResult> UpdateTodoItem(IDbContextFactory<TodoDbContext> dbContextFactory, ClaimsPrincipal user, int id, TodoInput todoItemInput)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        if (await dbContext.TodoItems.FirstOrDefaultAsync(t =>
                t.User.Username == user.FindFirst(ClaimTypes.NameIdentifier)!.Value && t.Id == id) is not
            { } todoItem) return TypedResults.NotFound();
        todoItem.IsCompleted = todoItemInput.IsCompleted;
        await dbContext.SaveChangesAsync();
        return TypedResults.NoContent();

    }

    public static async Task<IResult> DeleteTodoItem(IDbContextFactory<TodoDbContext> dbContextFactory, ClaimsPrincipal user, int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        if (await dbContext.TodoItems.FirstOrDefaultAsync(t =>
                t.User.Username == user.FindFirst(ClaimTypes.NameIdentifier)!.Value && t.Id == id) is not
            { } todoItem) return TypedResults.NotFound();
        dbContext.TodoItems.Remove(todoItem);
        await dbContext.SaveChangesAsync();
        return TypedResults.NoContent();

    }
}