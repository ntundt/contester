using System.Text.Json;
using System.Text.RegularExpressions;
using diploma.Services;
using MediatR;

namespace diploma.Middleware;

// For some weird reason, the compiler requires the partial keyword here, though actually there's no other part of the class
public partial class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IConfigurationReaderService _configuration;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IConfigurationReaderService configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    private string SerializeObject(TRequest request)
    {
        return JsonSerializer.Serialize(request);
    }

    [GeneratedRegex("\"Password\":\\s*\".*?\"")]
    private static partial Regex PasswordRegex();

    private string RedactPassword(string request)
    {
        return PasswordRegex().Replace(request, "\"Password\":\"REDACTED\"");
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid();
        string requestJson = "";
        try 
        {
            requestJson = RedactPassword(SerializeObject(request));
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error serializing {requestId} {typeof(TRequest).Name}");
        }
        _logger.LogInformation($"Handling {requestId} {typeof(TRequest).Name}: {requestJson}");

        try
        {
            var response = await next();
            _logger.LogInformation($"Handled {requestId}");
            return response;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error handling {requestId}");
            throw;
        }
    }
}