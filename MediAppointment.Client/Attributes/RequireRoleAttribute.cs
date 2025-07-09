using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MediAppointment.Client.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            
            // Check if user is authenticated
            var isAuthenticated = httpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl = httpContext.Request.Path });
                return;
            }

            // Check user role
            var userRole = httpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                context.Result = new ViewResult
                {
                    ViewName = "Unauthorized",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                        new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                        new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                    {
                        ["Title"] = "Không có quyền truy cập",
                        ["Message"] = $"Bạn cần quyền {string.Join(" hoặc ", _roles)} để truy cập trang này.",
                        ["RequiredRoles"] = _roles,
                        ["UserRole"] = userRole
                    }
                };
                return;
            }
        }
    }

    // Convenience attributes for specific roles
    public class RequirePatientAttribute : RequireRoleAttribute
    {
        public RequirePatientAttribute() : base(Constants.UserRoles.Patient) { }
    }

    public class RequireDoctorAttribute : RequireRoleAttribute
    {
        public RequireDoctorAttribute() : base(Constants.UserRoles.Doctor) { }
    }

    public class RequireAdminAttribute : RequireRoleAttribute
    {
        public RequireAdminAttribute() : base(Constants.UserRoles.Admin) { }
    }

    public class RequireManagerAttribute : RequireRoleAttribute
    {
        public RequireManagerAttribute() : base(Constants.UserRoles.Manager) { }
    }

    // Combined role attributes
    public class RequireAdminOrManagerAttribute : RequireRoleAttribute
    {
        public RequireAdminOrManagerAttribute() : base(Constants.UserRoles.Admin, Constants.UserRoles.Manager) { }
    }

    public class RequireDoctorOrAdminAttribute : RequireRoleAttribute
    {
        public RequireDoctorOrAdminAttribute() : base(Constants.UserRoles.Doctor, Constants.UserRoles.Admin) { }
    }
}
