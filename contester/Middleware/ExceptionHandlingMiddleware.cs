using contester.Exceptions;
using contester.Features.Authentication.Exceptions;
using contester.Features.Problems.Exceptions;
using contester.Features.SchemaDescriptions.Exceptions;
using contester.Features.Users.Exceptions;
using FluentValidation;

namespace contester.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private void PrintNestedException(Exception exception)
    {
        logger.LogWarning("{}: {}", exception.GetType().Name, exception.Message);
        logger.LogWarning(exception.StackTrace);
        if (exception.InnerException != null)
        {
            PrintNestedException(exception.InnerException);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        switch (exception)
        {
            case UserAlreadyExistsException:
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { err = 101, message = "User already exists" });
                break;
            case UserNotFoundException:
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new { err = 102, message = "User not found" });
                break;
            case PasswordDoesNotMeetRequirementsException:
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { err = 103, message = "Password does not meet requirements" });
                break;
            case InvalidPasswordException:
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { err = 104, message = "Wrong password" });
                break;
            case ApplicationException:
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { err = 105, message = "Internal server error" });
                logger.LogCritical("Unhandled application exception:");
                PrintNestedException(exception);
                break;
            case UserDoesNotHavePermissionException:
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { err = 106, message = "User does not have right to do that" });
                break;
            case SchemaDescriptionFileNotFoundException:
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new { err = 107, message = "Schema description file not found" });
                break;
            case SchemaDescriptionNotFoundException:
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new { err = 108, message = "Schema description not found" });
                break;
            case EmailNotConfirmedException:
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { err = 109, message = "Email not confirmed" });
                break;
            case EmailAlreadyConfirmedException:
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { err = 110, message = "Email already confirmed" });
                break;
            case ProblemSolutionInvalidException e:
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { err = 111, message = $"Problem solution is invalid: {e.Message}" });
                break;
            case NotifyUserException e:
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { err = 112, message = e.Message });
                logger.LogWarning(e.Message);
                PrintNestedException(e);
                break;
            case ValidationException e:
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { err = 113, message = e.Message });
                break;
            default:
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { err = 109, message = "Internal server error" });
                logger.LogCritical("Unhandled exception:");
                PrintNestedException(exception);
                break;
        }
    }
}