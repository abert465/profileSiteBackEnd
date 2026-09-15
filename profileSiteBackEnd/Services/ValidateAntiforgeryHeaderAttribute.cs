using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Antiforgery;

namespace profileSiteBackEnd.Services
{
    public sealed class ValidateAntiforgeryHeaderAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            var req = ctx.HttpContext.Request;
            if(HttpMethods.IsGet(req.Method) || HttpMethods.IsHead(req.Method) || HttpMethods.IsOptions(req.Method))
            {
                await next();
                return;
            }
            var antiforgery = ctx.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
            try
            {
                await antiforgery.ValidateRequestAsync(ctx.HttpContext);
            }
            catch (AntiforgeryValidationException ex)
            {
                // ValidateRequestAsync throws; without this the request surfaces as
                // an unhandled 500 rather than the 400 a missing token deserves.
                ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger<ValidateAntiforgeryHeaderAttribute>()
                    .LogWarning(ex, "Rejected {Method} {Path}: antiforgery validation failed.",
                        req.Method, req.Path);

                ctx.Result = new BadRequestObjectResult(new { error = "Invalid or missing CSRF token." });
                return;
            }

            await next();
        }
    }
}
