using FluentValidation;
using MediatR;
using mediatr_todos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace mediatr_todos;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddLogging();
        
        builder.Services.AddMediatR(typeof(GetTodosQuery).Assembly)
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        builder.Services.AddValidatorsFromAssembly(typeof(PostTodoCommandValidator).Assembly);

        builder.Services.AddSingleton<TodosService>();
    }
}