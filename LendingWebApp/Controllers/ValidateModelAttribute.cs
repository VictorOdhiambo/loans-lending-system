using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LoanApplicationService.Web.Controllers
{ 
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class ValidateModelAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext context)
            {
                if (!context.ModelState.IsValid)
                {
                    var controller = context.Controller as Controller;

                    // Try to retrieve the original model passed to the action
                    var model = context.ActionArguments.Values.FirstOrDefault();

                    if (controller != null && model != null)
                    {
                        context.Result = controller.View(model);
                    }
                    else
                    {
                        // Fallback in case we can't resolve the controller or model
                        context.Result = new BadRequestResult();
                    }
                }
            }
        }
    }





    
