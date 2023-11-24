using FluentValidation;

namespace TodoMinimalApi.ViewModels;

public class UserInputValidator: AbstractValidator<UserInput>
{
    public UserInputValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}