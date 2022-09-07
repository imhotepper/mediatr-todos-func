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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace mediatr_todos
{
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
            GetTodosQuery gtq,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                return new OkObjectResult(await _mediator.Send(gtq, cancellationToken));
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
        [OpenApiSecurity("Autorization", SecuritySchemeType.Http, Scheme = Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums.OpenApiSecuritySchemeType.Basic, In = Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums.OpenApiSecurityLocationType.Header,
            Description = "Basic authorization user and password!")]
        public async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            PostTodoCommand todoCommand,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                return new OkObjectResult(await _mediator.Send(todoCommand, cancellationToken));
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
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Title).MinimumLength(3);
        }
    }

    // Pipelines
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


    public class BaseRequest
    {
        public int UserId { get; set; }
    }

    public class PostTodoCommand:BaseRequest, IRequest<Todo>{
        public string Title { get; set; }
    }
//public record PostTodoCommand(Todo Todo) : IRequest<Todo>;


    record PostTodoCommandHandler : IRequestHandler<PostTodoCommand, Todo>
    {
        TodosService _service;

        public PostTodoCommandHandler(TodosService service) => _service = service;

        public Task<Todo> Handle(PostTodoCommand request, CancellationToken cancellationToken)
            => Task.FromResult(_service.Add(request.Title));
    }

    public class GetTodosQuery : BaseRequest, IRequest<List<Todo>>
    {
    }

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
    
    public class AuthBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<AuthBehavior<TRequest, TResponse>> _logger;
        private readonly IHttpContextAccessor _req;

        public AuthBehavior(ILogger<AuthBehavior<TRequest, TResponse>> logger, IHttpContextAccessor req)
        {
            _logger = logger;
            _req = req;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            if (
                request is BaseRequest br) //&&
                // !string.IsNullOrEmpty( _req.HttpContext.Request.Headers["Authorization"]))
            {
                //TODO: get the ID
                br.UserId = 100;
            }

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

        public Todo Add(string title)
        {
            var todo = new Todo(_todos.Count + 1, title);
            _todos.Add(todo);
            return todo;
        }

        public List<Todo> GetTodos() => _todos;
    }
}