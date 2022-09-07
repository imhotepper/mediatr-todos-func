using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;


namespace todos;

    public class TodosApi
    {
        private readonly ILogger<TodosApi> _logger;
        private readonly IMediator _mediator;

        public TodosApi(ILogger<TodosApi> log, IMediator mediator)
        {
            _logger = log;
            _mediator = mediator;
        }

        [FunctionName("todos-get")]
        [OpenApiOperation(operationId: "get-todos", tags: new[] { "todos" })]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(List<Todo>),
            Description = "The OK response")]
        public async Task<IActionResult> Gets(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                return new OkObjectResult(await _mediator.Send(new GetTodosQuery(), cancellationToken));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        [FunctionName("todos-post")]
        [OpenApiOperation(operationId: "post-todos", tags: new[] { "todos" })]
        [OpenApiRequestBody("application/json", typeof(Todo), Description = "JSON request body containing { title}")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Todo),
            Description = "The OK response")]
        public async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] Todo todo,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                return new OkObjectResult(await _mediator.Send(new PostTodoCommand(todo), cancellationToken));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }
        
        private static IActionResult HandleException(Exception e)
        {
            
            Console.WriteLine($"Exception handling: {e} ");
            if (e is ValidationException ve)
                return new UnprocessableEntityObjectResult(
                    ve.Errors.Select(x => new { Property = x.PropertyName, Error = x.ErrorMessage }));

            return new InternalServerErrorResult();
        }
    }
    
   



// Validation

public class PostTodoCommandValidator : AbstractValidator<PostTodoCommand>
{
    public PostTodoCommandValidator()
    {
        RuleFor(x => x.Todo.Title).NotEmpty();
        RuleFor(x => x.Todo.Title).MinimumLength(3);
    }
}

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults =
                await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();
            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}


public record PostTodoCommand(Todo Todo) : IRequest<Todo>;

record PostTodoCommandHandler : IRequestHandler<PostTodoCommand, Todo>
{
    TodosService _service;

    public PostTodoCommandHandler(TodosService service) => _service = service;

    public Task<Todo> Handle(PostTodoCommand request, CancellationToken cancellationToken)
        => Task.FromResult(_service.Add(request.Todo));
}

record GetTodosQuery : IRequest<List<Todo>>;

record GetTodosQueryHandler : IRequestHandler<GetTodosQuery, List<Todo>>
{
    private readonly TodosService _todosService;
    public GetTodosQueryHandler(TodosService service) => _todosService = service;

    public Task<List<Todo>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_todosService.GetTodos());
    }
}


public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation($"Handling {typeof(TRequest).Name}");
        var response = await next();
        _logger.LogInformation($"Handled {typeof(TResponse).Name}");

        return response;
    }
}


public record Todo(int? Id, string Title);


public record TodosService
{
    List<Todo> _todos = new List<Todo>();

    public Todo Add(Todo todo)
    {
        todo = todo with { Id = _todos.Count + 1 };
        _todos.Add(todo);
        return todo;
    }

    public List<Todo> GetTodos() => _todos;
}