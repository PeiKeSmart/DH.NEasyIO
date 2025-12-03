using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Pek.NCube.BaseControllers;

using IActionFilter = Microsoft.AspNetCore.Mvc.Filters.IActionFilter;

namespace Pek.EasyIO.Controllers;

/// <summary>控制器基类</summary>
public class ApiControllerBase : ApiControllerBaseX, IActionFilter
{
    /// <summary>动作执行前</summary>
    /// <param name="context"></param>
    [NonAction]
    public virtual void OnActionExecuting(ActionExecutingContext context)
    {

    }

    /// <summary>动作执行后</summary>
    /// <param name="context"></param>
    [NonAction]
    public virtual void OnActionExecuted(ActionExecutedContext context)
    {

    }
}
