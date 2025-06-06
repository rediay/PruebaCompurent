using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MusicRadioInc.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public CustomAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 1. Verificar si el usuario está autenticado (existe en sesión)
            var userLoginId = context.HttpContext.Session.GetString("UserLoginId");
            if (string.IsNullOrEmpty(userLoginId))
            {
                // No autenticado, redirigir al login
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Login", action = "Index" })
                );
                return;
            }

            // 2. Si hay roles especificados en el atributo, verificar el rol del usuario
            if (_allowedRoles != null && _allowedRoles.Length > 0)
            {
                var userRole = context.HttpContext.Session.GetString("UserRole");

                if (string.IsNullOrEmpty(userRole) || !_allowedRoles.Contains(userRole))
                {
                    // No tiene el rol requerido, redirigir a una página de "Acceso Denegado"
                    context.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Home", action = "AccessDenied" })
                    );
                    return;
                }
            }

            // Si llega aquí, el usuario está autenticado y tiene los roles correctos (o no se especificaron roles)
            // Continuar con la ejecución de la acción
        }
    }
}
