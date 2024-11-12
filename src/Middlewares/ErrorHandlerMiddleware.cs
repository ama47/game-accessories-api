using src.Utils;
namespace src.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        protected readonly RequestDelegate _next;
        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (CustomException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";
                var response = new { ex.StatusCode, ex.Message };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
