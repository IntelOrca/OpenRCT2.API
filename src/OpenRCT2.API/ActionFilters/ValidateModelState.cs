using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenRCT2.API.Implementations;

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
                    var firstError = new SerializableError(modelState).FirstOrDefault();
                    var message = "Invalid request";
                    if (firstError.Value is string[] messages && messages.Length != 0)
                    {
                        message = messages[0];
                    }
                    filterContext.Result = controller.BadRequest(JResponse.Error(message));
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
