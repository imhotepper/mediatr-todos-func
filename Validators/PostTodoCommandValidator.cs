using FluentValidation;

namespace mediatr_todos;

public class PostTodoCommandValidator : AbstractValidator<PostTodoCommand>
{
    public PostTodoCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Title).MinimumLength(3);
    }
}