using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using HlktechFileStorage.Entity;

using NewLife;

namespace Pek.EasyIO.Services;

/// <summary>上传令牌服务（七牛云风格，基于 HMAC-SHA256 签名）</summary>
public class UploadTokenService
{
    /// <summary>生成上传令牌</summary>
    /// <param name="projectId">项目ID</param>
    /// <param name="fileHash">文件哈希（MD5，32位小写）</param>
    /// <param name="fileName">文件名</param>
    /// <param name="fileSize">文件大小</param>
    /// <param name="externalUserId">外部用户ID</param>
    /// <param name="expiresInMinutes">过期时间（分钟）</param>
    /// <returns>令牌字符串，格式：projectId:fileHash:userId:expires:signature</returns>
    public String GenerateToken(
        Int64 projectId,
        String fileHash,
        String fileName,
        Int64 fileSize,
        String externalUserId,
        Int32 expiresInMinutes = 60)
    {
        // 查询项目密钥
        var project = FileProject.FindById(projectId);
        if (project == null)
            throw new Exception($"项目不存在：{projectId}");

        if (project.ApiSecret.IsNullOrEmpty())
            throw new Exception($"项目 [{project.Name}] 未配置 ApiSecret，无法生成令牌");

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes).ToUnixTimeSeconds();

        // 构造待签名载荷：projectId:fileHash:userId:expires
        var payload = $"{projectId}:{fileHash.ToLower()}:{externalUserId}:{expiresAt}";

        // 使用项目密钥进行 HMAC-SHA256 签名
        var signature = ComputeHmacSha256(payload, project.ApiSecret);

        // 最终令牌：payload:signature
        return $"{payload}:{signature}";
    }

    /// <summary>验证并解析上传令牌</summary>
    /// <param name="token">令牌字符串</param>
    /// <returns>成功返回声明主体，失败返回 null</returns>
    public ClaimsPrincipal ValidateToken(String token)
    {
        if (token.IsNullOrEmpty())
            return null;

        try
        {
            // 解析令牌：projectId:fileHash:userId:expires:signature
            var parts = token.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5)
                return null;

            var projectIdStr = parts[0];
            var fileHash = parts[1];
            var externalUserId = parts[2];
            var expiresStr = parts[3];
            var signature = parts[4];

            // 验证项目ID
            if (!Int64.TryParse(projectIdStr, out var projectId))
                return null;

            // 验证过期时间
            if (!Int64.TryParse(expiresStr, out var expiresAt))
                return null;

            var expiresTime = DateTimeOffset.FromUnixTimeSeconds(expiresAt);
            if (expiresTime <= DateTimeOffset.UtcNow)
                return null; // 令牌已过期

            // 查询项目密钥
            var project = FileProject.FindById(projectId);
            if (project == null || project.ApiSecret.IsNullOrEmpty())
                return null;

            // 重新计算签名
            var payload = $"{projectId}:{fileHash}:{externalUserId}:{expiresAt}";
            var expectedSignature = ComputeHmacSha256(payload, project.ApiSecret);

            // 验证签名（防止篡改）
            if (signature != expectedSignature)
                return null;

            // 构造 ClaimsPrincipal（兼容现有代码）
            var claims = new[]
            {
                new Claim("projectId", projectId.ToString()),
                new Claim("fileHash", fileHash),
                new Claim("externalUserId", externalUserId),
                new Claim("expires", expiresAt.ToString()),
                new Claim("tokenType", "upload")
            };

            var identity = new ClaimsIdentity(claims, "UploadToken");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>从令牌中提取项目ID</summary>
    public Int64? GetProjectId(ClaimsPrincipal principal)
    {
        var claim = principal?.FindFirst("projectId");
        return claim != null && Int64.TryParse(claim.Value, out var projectId) ? projectId : null;
    }

    /// <summary>从令牌中提取文件哈希</summary>
    public String GetFileHash(ClaimsPrincipal principal)
    {
        return principal?.FindFirst("fileHash")?.Value;
    }

    /// <summary>从令牌中提取外部用户ID</summary>
    public String GetExternalUserId(ClaimsPrincipal principal)
    {
        return principal?.FindFirst("externalUserId")?.Value;
    }

    /// <summary>计算 HMAC-SHA256 签名（复用现有签名机制）</summary>
    /// <param name="data">待签名数据</param>
    /// <param name="secret">密钥</param>
    /// <returns>签名结果（小写十六进制）</returns>
    private static String ComputeHmacSha256(String data, String secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
