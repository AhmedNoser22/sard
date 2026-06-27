using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sard.API.Filters
{
    public class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                var validator = serviceProvider.GetService(validatorType) as IValidator;

                if (validator is null) continue;

                var result = await validator.ValidateAsync(new ValidationContext<object>(argument));

                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(e => e.ErrorMessage);
                    context.Result = new BadRequestObjectResult(new { Errors = errors });
                    return;
                }
            }

            await next();
        }
    }
}
