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

    public TodosApi(ILogger<TodosApi> log) => _logger = log;

    [FunctionName("todos-get")]
    [OpenApiOperation(operationId: "get-todos", tags: new[] { "todos" })]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(List<string>),
        Description = "The OK response")]
    public async Task<IActionResult> Gets(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
        HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            return new OkObjectResult(new List<string>());
        }
        catch (Exception e)
        {
            return HandleException(e);
        }
    }

    [FunctionName("todos-post")]
    [OpenApiOperation(operationId: "post-todos", tags: new[] { "todos" })]
    [OpenApiRequestBody("application/json", typeof(string), Description = "JSON request body containing { title}")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string),
        Description = "The OK response")]
    public async Task<IActionResult> Post(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            return new OkResult();
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