using CourseAPI.Services.Validation;

namespace CourseAPI.Endpoints.Filters
{
    public sealed class BadRequestOnInvalidRequest(ILogger<BadRequestOnInvalidRequest> logger) : IEndpointFilter
    {
        // Gjør InvalidRequestException fra service-laget om til 400 Bad Request i ProblemDetails-format, og logger en advarsel.
        // Hvorfor et filter og ikke middleware: det skal bare gjelde endepunktene som tar imot en request-DTO.
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            try
            {
                return await next(context);
            }
            catch (InvalidRequestException exception)
            {
                var request = context.HttpContext.Request;
                logger.LogWarning("Ugyldige data avvist i {Method} {Path}. Felt med feil: {Fields}",
                    request.Method, request.Path, exception.Errors.Keys);

                return Results.ValidationProblem(exception.Errors, title: "Valideringen feilet");
            }
        }
    }
}