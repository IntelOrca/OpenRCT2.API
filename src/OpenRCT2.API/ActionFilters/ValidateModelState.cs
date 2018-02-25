using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OpenRCT2.API.ActionFilters
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Controller is Controller controller)
            {
                var modelState = controller.ModelState;
                if (modelState?.IsValid == false)
                {
                    filterContext.Result = controller.BadRequest(modelState);
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
