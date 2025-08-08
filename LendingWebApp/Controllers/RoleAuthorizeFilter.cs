using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LoanApplicationService.Web.Controllers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RoleAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Check if user is authenticated
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            // Check if user has any of the required roles
            bool hasRequiredRole = false;
            foreach (var role in _roles)
            {
                if (user.IsInRole(role))
                {
                    hasRequiredRole = true;
                    break;
                }
            }

            if (!hasRequiredRole)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            // Additional role crossover prevention
            PreventRoleCrossover(context, user);
        }

        private void PreventRoleCrossover(AuthorizationFilterContext context, ClaimsPrincipal user)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString()?.ToLower();
            var actionName = context.RouteData.Values["action"]?.ToString()?.ToLower();

            // Customer-specific restrictions
            if (user.IsInRole("Customer"))
            {
                // Customers should not access admin-specific controllers
                var adminControllers = new[] { "users", "notificationtemplate", "loancharge" };
                if (adminControllers.Contains(controllerName))
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                    return;
                }

                // Customers should not access admin-specific actions
                var adminActions = new[] { "index", "create", "edit", "delete", "approve", "reject", "disburse" };
                if (controllerName == "loanapplication" && adminActions.Contains(actionName))
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                    return;
                }
            }

            // Admin/SuperAdmin restrictions
            if (user.IsInRole("Admin") || user.IsInRole("SuperAdmin"))
            {
                // Admins should not access customer-specific actions
                var customerActions = new[] { "customerreject", "customeraccept" };
                if (controllerName == "loanapplication" && customerActions.Contains(actionName))
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                    return;
                }
            }
        }
    }
}
