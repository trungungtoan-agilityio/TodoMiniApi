using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using TodoMinimalApi;
using TodoMinimalApi.ViewModels;

namespace TodoMinimalApiTest;

public class TodoApiTests
{
    [Fact]
    public async Task GetAllTodoItems_ReturnsOkResultOfIEnumerableTodoItems()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "secret-1"));

        var todoItemsResult = await TodoApi.GetAllTodoItems(testDbContextFactory, user);

        Assert.IsType<Ok<PagedResults<TodoOutput>>>(todoItemsResult);
    }
    
    [Fact]
    public async Task GetTodoItemById_ReturnsOkResultOfTodoItem()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new(ClaimTypes.NameIdentifier, "user1") }, "secret-1"));

        var todoItemResult = await TodoApi.GetTodoItemById(testDbContextFactory, user, 1);

        Assert.IsType<Ok<TodoOutput>>(todoItemResult);
    }

    [Fact]
    public async Task GetTodoItemById_ReturnsNotFound()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new(ClaimTypes.NameIdentifier, "user1") }, "secret-1"));

        var todoItemResult = await TodoApi.GetTodoItemById(testDbContextFactory, user, 100);

        Assert.IsType<NotFound>(todoItemResult);
    }

    [Fact]
    public async Task CreateTodoItem_ReturnsCreatedStatusWithLocation()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[] { new(ClaimTypes.NameIdentifier, "user1") }, "user1"));
        const string title = "This todo item from Unit test";
        var todoInput = new TodoInput() { IsCompleted = false, Title = title };
        var todoOutputResult = await TodoApi.CreateTodoItem(
            testDbContextFactory, user, todoInput, new TodoInputValidator(testDbContextFactory));

        Assert.IsType<Created<TodoOutput>>(todoOutputResult);
        var createdTodoOutput = todoOutputResult as Created<TodoOutput>;
        Assert.Equal(201, createdTodoOutput!.StatusCode);
        var actual = createdTodoOutput.Value!.Title;
        Assert.Equal(title, actual);
        var actualLocation = createdTodoOutput.Location;
        var expectedLocation = $"/todos/21";
        Assert.Equal(expectedLocation, actualLocation);
    }

    [Fact]
    public async Task CreateTodoItem_ReturnsProblem()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "secret-1"));

        var todoInput = new TodoInput();
        var todoOutputResult = await TodoApi.CreateTodoItem(testDbContextFactory, user, todoInput, new TodoInputValidator(testDbContextFactory));

        Assert.IsType<ValidationProblem>(todoOutputResult);
    }

    [Fact]
    public async Task UpdateTodoItem_ReturnsNoContent()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new(ClaimTypes.NameIdentifier, "user1") }, "secret-1"));

        var todoInput = new TodoInput() { IsCompleted = true };
        var result = await TodoApi.UpdateTodoItem(testDbContextFactory, user, 1, todoInput);

        Assert.IsType<NoContent>(result);
        var updateResult = result as NoContent;
        Assert.NotNull(updateResult);
    }

    [Fact]
    public async Task UpdateTodoItem_ReturnsNotFound()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "secret-1"));

        var todoInput = new TodoInput() { IsCompleted = true };
        var result = await TodoApi.UpdateTodoItem(testDbContextFactory, user, 205, todoInput);

        Assert.IsType<NotFound>(result);
        var updateResult = result as NotFound;
        Assert.NotNull(updateResult);
    }

    [Fact]
    public async Task DeleteTodoItem_ReturnsNoContent()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user1") }, "secret-1"));

        var result = await TodoApi.DeleteTodoItem(testDbContextFactory, user, 1);

        Assert.IsType<NoContent>(result);
        var deleteResult = result as NoContent;
        Assert.NotNull(deleteResult);
    }

    [Fact]
    public async Task DeleteTodoItem_ReturnsNotFound()
    {
        var testDbContextFactory = new TestDbContextFactory();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new(ClaimTypes.NameIdentifier, "user1") }, "secret-1"));

        var result = await TodoApi.DeleteTodoItem(testDbContextFactory, user, 105);

        Assert.IsType<NotFound>(result);
        var deleteResult = result as NotFound;
        Assert.NotNull(deleteResult);
    }
}