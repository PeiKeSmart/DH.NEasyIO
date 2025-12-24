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

        // 4. 读取请求体或文件哈希（用于签名）
        String bodyHash = "";
        
        // 对于 multipart/form-data（文件上传），使用上传文件的 MD5 哈希
        if (request.ContentType != null && request.ContentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            // 尝试从 form 中获取文件并计算哈希
            if (request.HasFormContentType && request.Form.Files.Count > 0)
            {
                var file = request.Form.Files[0];  // 获取第一个文件
                if (file != null && file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    using var md5 = MD5.Create();
                    var hash = await md5.ComputeHashAsync(stream);
                    bodyHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    XTrace.WriteLine($"文件上传签名验证，使用文件哈希：{bodyHash}, 文件名：{file.FileName}, 大小：{file.Length:N0} 字节");
                }
            }
        }
        else if (request.ContentLength > 0)
        {
            // 对于其他请求（JSON 等），使用完整请求体
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            bodyHash = body ?? "";
        }

        // 5. 生成签名字符串
        var signString = BuildSignString(request.Method, request.Path, request.QueryString.Value, bodyHash, timestamp);

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
    /// <param name="bodyHashOrContent">请求体内容或文件哈希（文件上传时为 MD5 哈希，其他请求为完整内容）</param>
    /// <param name="timestamp">时间戳</param>
    /// <returns></returns>
    public String BuildSignString(String method, String path, String query, String bodyHashOrContent, String timestamp)
    {
        // 签名格式：Method\nPath\nQuery\nBodyHashOrContent\nTimestamp
        // 注意：对于文件上传，bodyHashOrContent 是文件的 MD5 哈希（32字符），而非完整文件内容
        var parts = new[]
        {
            method?.ToUpper() ?? "",
            path ?? "",
            SortQueryString(query),
            bodyHashOrContent ?? "",
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
