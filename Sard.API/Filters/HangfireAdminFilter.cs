public class HangfireAdminFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        var env = httpContext.RequestServices.GetService<IWebHostEnvironment>();
        if (env?.IsDevelopment() ?? false)
            return true;

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Admin");
    }
}