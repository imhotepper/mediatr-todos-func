using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace mediatr_todos;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation($"LoggingBehavior: handling {typeof(TRequest).Name}");
        var response = await next();
//        _logger.LogInformation($"LoggingBehavior: handled {typeof(TResponse).Name}");
        _logger.LogInformation($"LoggingBehavior: handled {typeof(TResponse).FullName}");

        return response;
    }
}