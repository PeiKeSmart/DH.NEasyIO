using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using NewLife;
using NewLife.Log;
using NewLife.Remoting;

using Pek.NCube.BaseControllers;

using IActionFilter = Microsoft.AspNetCore.Mvc.Filters.IActionFilter;

namespace Pek.EasyIO.Controllers;

/// <summary>控制器基类</summary>
public class ApiControllerBase : ApiControllerBaseX, IActionFilter
{
    #region 属性
    /// <summary>令牌</summary>
    public String Token { get; set; }
    #endregion

    #region 构造
    /// <summary>动作执行前</summary>
    /// <param name="context"></param>
    [NonAction]
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // 访问令牌
        var request = context.HttpContext.Request;
        var token = request.Query["Token"] + "";
        if (token.IsNullOrEmpty()) token = (request.Headers["Authorization"] + "").TrimStart("Bearer ");
        if (token.IsNullOrEmpty()) token = request.Headers["X-Token"] + "";
        if (token.IsNullOrEmpty()) token = request.Cookies["Token"] + "";
        Token = token;

        try
        {
            //todo 令牌验证
            //if (token.IsNullOrEmpty() && context.ActionDescriptor is ControllerActionDescriptor act && !act.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute)))
            //    throw new ApiException(403, "认证失败");
        }
        catch (Exception ex)
        {
            context.Result = Json(0, null, ex);
        }
    }

    /// <summary>动作执行后</summary>
    /// <param name="context"></param>
    [NonAction]
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var ex = context.Exception?.GetTrue();
        var traceId = DefaultSpan.Current?.TraceId;

        if (context.Result != null)
        {
            if (context.Result is ObjectResult obj)
            {
                var rs = new { code = obj.StatusCode ?? 0, data = obj.Value, traceId };
                context.Result = new ContentResult
                {
                    Content = OnJsonSerialize(rs),
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }
            else if (context.Result is EmptyResult)
            {
                context.Result = new JsonResult(new { code = 0, data = new { }, traceId });
            }
        }
        else if (context.Exception != null && !context.ExceptionHandled)
        {
            if (ex is ApiException aex)
                context.Result = new JsonResult(new { code = aex.Code, data = aex.Message, traceId });
            else
                context.Result = new JsonResult(new { code = 500, data = ex.Message, traceId });

            context.ExceptionHandled = true;

            // 输出异常日志
            if (XTrace.Debug) XTrace.WriteException(ex);
        }
    }
    #endregion
}
