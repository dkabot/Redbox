using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Redbox.NetCore.Middleware.Middleware
{
    internal class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelAttribute> _logger;

        public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
        {
            _logger = logger;
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
                return;
            var actionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            var list = context.ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            _logger.LogError("{controllerName}.{actionName} request failed validation, errors: {@errors}",
                actionDescriptor.ControllerName, actionDescriptor.ActionName, list);
            context.Result = new BadRequestObjectResult(list);
        }
    }
}