using HlktechFileStorage.Entity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using NewLife;
using NewLife.Log;

using Pek.EasyIO.Services;

namespace Pek.EasyIO.Auth;

/// <summary>API鉴权特性（支持令牌或签名两种方式）</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ApiAuthAttribute : Attribute, IAsyncActionFilter
{
    private readonly ApiSignatureValidator _validator = new();
    private readonly UploadTokenService _tokenService = new();

    /// <summary>是否允许上传令牌鉴权（默认 false，仅允许 API Key）</summary>
    public Boolean AllowUploadToken { get; set; } = false;

    /// <summary>执行动作过滤</summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        // 如果不要求鉴权,尝试获取项目信息但不验证签名
        if (!EasyIOSetting.Current.ApiAuthEnabled)
        {
            var disabledProjectCode = GetParameter(request, "X-Project-Code", "projectCode");
            if (!disabledProjectCode.IsNullOrEmpty())
            {
                var project = FileProject.FindByCode(disabledProjectCode);
                if (project != null && project.Enable)
                {
                    context.HttpContext.Items["ApiProject"] = project;
                    XTrace.WriteLine($"API鉴权已禁用，使用项目：{project.Name}({project.Code}) - {request.Method} {request.Path}");
                }
            }
            
            await next();
            return;
        }

        // 尝试令牌鉴权（如果允许且提供了令牌）
        var uploadToken = request.Headers["X-Upload-Token"].FirstOrDefault();
        if (AllowUploadToken && !uploadToken.IsNullOrEmpty())
        {
            var principal = _tokenService.ValidateToken(uploadToken);
            if (principal == null)
            {
                context.Result = new JsonResult(new
                {
                    code = 403,
                    message = "上传令牌无效或已过期"
                })
                {
                    StatusCode = 403
                };
                return;
            }

            // 从令牌提取项目ID
            var projectId = _tokenService.GetProjectId(principal);
            if (!projectId.HasValue)
            {
                context.Result = new JsonResult(new
                {
                    code = 403,
                    message = "令牌中缺少项目信息"
                })
                {
                    StatusCode = 403
                };
                return;
            }

            var tokenProject = FileProject.FindById(projectId.Value);
            if (tokenProject == null || !tokenProject.Enable)
            {
                context.Result = new JsonResult(new
                {
                    code = 403,
                    message = "令牌关联的项目不存在或已禁用"
                })
                {
                    StatusCode = 403
                };
                return;
            }

            // 令牌鉴权成功
            context.HttpContext.Items["ApiProject"] = tokenProject;
            context.HttpContext.Items["AuthMode"] = "UploadToken";
            context.HttpContext.Items["TokenPrincipal"] = principal;
            
            XTrace.WriteLine($"令牌鉴权成功：项目={tokenProject.Name}({tokenProject.Code}) - {request.Method} {request.Path}");
            await next();
            return;
        }

        // 尝试传统 API Key 签名鉴权
        var projectCode = GetParameter(request, "X-Project-Code", "projectCode");
        var timestamp = GetParameter(request, "X-Timestamp", "timestamp");
        var signature = GetParameter(request, "X-Signature", "signature");

        // 如果都没有提供鉴权信息，返回401
        if (projectCode.IsNullOrEmpty() && timestamp.IsNullOrEmpty() && signature.IsNullOrEmpty())
        {
            var msg = AllowUploadToken 
                ? "缺少API鉴权信息（需要 X-Upload-Token 或 X-Api-Key + X-Signature）" 
                : "缺少API鉴权信息";
            
            context.Result = new JsonResult(new
            {
                code = 401,
                message = msg
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
        context.HttpContext.Items["AuthMode"] = "ApiKey";

        XTrace.WriteLine($"API Key鉴权成功：项目={result.Project.Name}({result.Project.Code}) - {request.Method} {request.Path}");

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
