using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using FluentValidation;
using MediatR;
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
        [OpenApiSecurity("basic_auth",SecuritySchemeType.Http,Scheme = OpenApiSecuritySchemeType.Basic)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Todo>),
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
        [OpenApiRequestBody("application/json", typeof(PostTodoCommand), Description = "JSON request body containing { title}")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Todo), Description = "The OK response")]
        [OpenApiSecurity("basic_auth",SecuritySchemeType.Http,Scheme = OpenApiSecuritySchemeType.Basic)]
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
}