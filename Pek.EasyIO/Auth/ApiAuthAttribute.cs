using HlktechFileStorage.Entity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using NewLife;
using NewLife.Log;

namespace Pek.EasyIO.Auth;

/// <summary>API鉴权特性</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ApiAuthAttribute : Attribute, IAsyncActionFilter
{
    private readonly ApiSignatureValidator _validator = new();

    /// <summary>执行动作过滤</summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        // 从请求头或查询参数获取鉴权信息
        var projectCode = GetParameter(request, "X-Project-Code", "projectCode");

        // 如果不要求鉴权，尝试获取项目信息但不验证签名
        if (!EasyIOSetting.Current.ApiAuthEnabled)
        {
            if (!projectCode.IsNullOrEmpty())
            {
                var project = FileProject.FindByCode(projectCode);
                if (project != null && project.Enable)
                {
                    context.HttpContext.Items["ApiProject"] = project;
                    XTrace.WriteLine($"API鉴权已禁用，使用项目：{project.Name}({project.Code}) - {request.Method} {request.Path}");
                }
            }
            
            await next();
            return;
        }

        var timestamp = GetParameter(request, "X-Timestamp", "timestamp");
        var signature = GetParameter(request, "X-Signature", "signature");

        // 如果都没有提供鉴权信息，返回401
        if (projectCode.IsNullOrEmpty() && timestamp.IsNullOrEmpty() && signature.IsNullOrEmpty())
        {
            context.Result = new JsonResult(new
            {
                code = 401,
                message = "缺少API鉴权信息"
            })
            {
                StatusCode = 401
            };
            return;
        }

        // 验证签名
        var result = await _validator.ValidateAsync(request, projectCode, timestamp, signature);

        if (!result.Success)
        {
            XTrace.WriteLine($"API鉴权失败：{result.Message} - {request.Method} {request.Path}");
            
            context.Result = new JsonResult(new
            {
                code = 403,
                message = $"鉴权失败：{result.Message}"
            })
            {
                StatusCode = 403
            };
            return;
        }

        // 将项目信息存入 HttpContext.Items，供后续使用
        context.HttpContext.Items["ApiProject"] = result.Project;

        XTrace.WriteLine($"API鉴权成功：项目={result.Project.Name}({result.Project.Code}) - {request.Method} {request.Path}");

        await next();
    }

    private String GetParameter(Microsoft.AspNetCore.Http.HttpRequest request, String headerName, String queryName)
    {
        // 优先从请求头获取
        var value = request.Headers[headerName].FirstOrDefault();
        if (!value.IsNullOrEmpty()) return value;

        // 其次从查询参数获取
        value = request.Query[queryName].FirstOrDefault();
        return value;
    }
}

/// <summary>API控制器扩展方法</summary>
public static class ApiControllerExtensions
{
    /// <summary>获取当前请求的项目信息</summary>
    public static FileProject GetCurrentProject(this ControllerBase controller)
    {
        if (controller.HttpContext.Items.TryGetValue("ApiProject", out var project))
            return project as FileProject;

        return null;
    }
}
