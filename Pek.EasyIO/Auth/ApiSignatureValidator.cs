using System.Security.Cryptography;
using System.Text;

using HlktechFileStorage.Entity;

using Microsoft.AspNetCore.Http;

using NewLife;
using NewLife.Log;

namespace Pek.EasyIO.Auth;

/// <summary>API签名验证器</summary>
public class ApiSignatureValidator
{
    /// <summary>签名过期时间（秒）</summary>
    public Int32 SignatureExpireSeconds { get; set; } = 300;

    /// <summary>验证API签名</summary>
    /// <param name="request">HTTP请求</param>
    /// <param name="projectCode">项目编码</param>
    /// <param name="timestamp">时间戳</param>
    /// <param name="signature">签名</param>
    /// <returns></returns>
    public async Task<ApiAuthResult> ValidateAsync(HttpRequest request, String projectCode, String timestamp, String signature)
    {
        var result = new ApiAuthResult();

        // 1. 验证必填参数
        if (projectCode.IsNullOrEmpty())
        {
            result.Success = false;
            result.Message = "缺少项目编码";
            return result;
        }

        if (timestamp.IsNullOrEmpty())
        {
            result.Success = false;
            result.Message = "缺少时间戳";
            return result;
        }

        if (signature.IsNullOrEmpty())
        {
            result.Success = false;
            result.Message = "缺少签名";
            return result;
        }

        // 2. 查找项目配置
        var project = FileProject.FindByCode(projectCode);
        if (project == null)
        {
            result.Success = false;
            result.Message = "项目不存在";
            return result;
        }

        if (!project.Enable)
        {
            result.Success = false;
            result.Message = "项目已禁用";
            return result;
        }

        if (project.ApiSecret.IsNullOrEmpty())
        {
            result.Success = false;
            result.Message = "项目未配置API密钥";
            return result;
        }

        // 3. 验证时间戳
        if (!Int64.TryParse(timestamp, out var ts))
        {
            result.Success = false;
            result.Message = "时间戳格式错误";
            return result;
        }

        var requestTime = DateTimeOffset.FromUnixTimeSeconds(ts).LocalDateTime;
        var now = DateTime.Now;
        var diff = Math.Abs((now - requestTime).TotalSeconds);

        if (diff > SignatureExpireSeconds)
        {
            result.Success = false;
            result.Message = $"请求已过期（时间差：{diff:F0}秒）";
            return result;
        }

        // 4. 读取请求体（用于签名）
        String body = null;
        if (request.ContentLength > 0)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        // 5. 生成签名字符串
        var signString = BuildSignString(request.Method, request.Path, request.QueryString.Value, body, timestamp);

        // 6. 计算期望的签名
        var expectedSignature = ComputeSignature(signString, project.ApiSecret);

        // 7. 比对签名
        if (!signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Message = "签名验证失败";
            XTrace.WriteLine($"签名验证失败：项目={projectCode}, 期望={expectedSignature}, 实际={signature}");
            XTrace.WriteLine($"签名字符串：{signString}");
            return result;
        }

        // 8. 验证成功
        result.Success = true;
        result.Project = project;
        return result;
    }

    /// <summary>构建签名字符串</summary>
    /// <param name="method">HTTP方法</param>
    /// <param name="path">路径</param>
    /// <param name="query">查询参数</param>
    /// <param name="body">请求体</param>
    /// <param name="timestamp">时间戳</param>
    /// <returns></returns>
    public String BuildSignString(String method, String path, String query, String body, String timestamp)
    {
        // 签名格式：Method\nPath\nQuery\nBody\nTimestamp
        var parts = new[]
        {
            method?.ToUpper() ?? "",
            path ?? "",
            SortQueryString(query),
            body ?? "",
            timestamp ?? ""
        };

        return String.Join("\n", parts);
    }

    /// <summary>计算HMAC-SHA256签名</summary>
    /// <param name="data">待签名数据</param>
    /// <param name="secret">密钥</param>
    /// <returns></returns>
    public String ComputeSignature(String data, String secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    /// <summary>排序查询参数（用于签名一致性）</summary>
    private String SortQueryString(String query)
    {
        if (query.IsNullOrEmpty()) return "";

        query = query.TrimStart('?');
        var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        Array.Sort(pairs);
        return String.Join("&", pairs);
    }
}

/// <summary>API鉴权结果</summary>
public class ApiAuthResult
{
    /// <summary>是否成功</summary>
    public Boolean Success { get; set; }

    /// <summary>消息</summary>
    public String Message { get; set; }

    /// <summary>项目信息</summary>
    public FileProject Project { get; set; }
}
