using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ExceptionHandleNH
{
    public class GlobalExceptionNH : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            switch (exception)
            {
                case ArgumentNullException:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                case UnauthorizedAccessException:
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    break;
                case InvalidOperationException:
                    httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                    break;
                default:
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }
            var problem = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An error occurred",
                Detail = "An unexpected error occurred while processing your request. Please try again later.",
                Instance = httpContext.Request.Path
                
            };
            problem.Extensions["message"] = exception.Message;
            httpContext.Response.StatusCode = problem.Status.GetValueOrDefault((int)HttpStatusCode.InternalServerError);
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    }
}
