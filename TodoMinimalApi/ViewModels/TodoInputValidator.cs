using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TodoMinimalApi.Data;

namespace TodoMinimalApi.ViewModels;

public class TodoInputValidator: AbstractValidator<TodoInput>
{
    private readonly TodoDbContext _todoDbContext;
    public TodoInputValidator(IDbContextFactory<TodoDbContext> dbContextFactory)
    {
        _todoDbContext = dbContextFactory.CreateDbContext();

        RuleFor(t => t.Title).NotEmpty().MaximumLength(100).MinimumLength(3)
            .Must(IsUnique).WithMessage("Title should be unique.");
    }

    private bool IsUnique(string title)
    {
        return !_todoDbContext.TodoItems.Any(t => t.Title == title);
    }
}