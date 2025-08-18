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
            await antiforgery.ValidateRequestAsync(ctx.HttpContext);
            await next();
        }
    }
}
