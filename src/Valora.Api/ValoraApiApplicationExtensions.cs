using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Valora.Application;

namespace Valora.Api;

public static class ValoraApiApplicationExtensions
{
    public static WebApplication UseValoraApi(this WebApplication app)
    {
        app.UseExceptionHandler(handler => handler.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            var status = exception is ValoraException valoraException ? valoraException.StatusCode : 500;
            var code = exception is ValoraException knownException ? knownException.Code : "internal_error";
            var title = status switch
            {
                400 => "Invalid request",
                404 => "Not found",
                409 => "Conflict",
                _ => "An unexpected error occurred"
            };
            var detail = exception is ValoraException expected ? expected.Message : "The request could not be completed.";
            var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };
            problem.Extensions["code"] = code;
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem, cancellationToken: context.RequestAborted);
        }));
        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options.WithTitle("Valora API"));
        }

        app.MapValoraApiEndpoints();
        return app;
    }
}
