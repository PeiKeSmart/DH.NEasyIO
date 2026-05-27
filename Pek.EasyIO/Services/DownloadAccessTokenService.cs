using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using HlktechFileStorage.Entity;

using NewLife;

namespace Pek.EasyIO.Services;

/// <summary>下载访问令牌服务</summary>
public class DownloadAccessTokenService
{
    /// <summary>默认有效期（秒）</summary>
    public Int32 DefaultExpiresInSeconds { get; set; } = 300;

    /// <summary>最大有效期（秒）</summary>
    public Int32 MaxExpiresInSeconds { get; set; } = 3600;

    /// <summary>生成下载访问令牌</summary>
    /// <param name="projectId">项目编号</param>
    /// <param name="expiresInSeconds">有效期（秒）</param>
    /// <returns>访问令牌</returns>
    public String GenerateToken(Int64 projectId, Int32 expiresInSeconds = 300)
    {
        var project = FileProject.FindById(projectId);
        if (project == null)
            throw new Exception($"项目不存在：{projectId}");

        if (!project.Enable)
            throw new Exception($"项目已禁用：{project.Name}({project.Code})");

        if (project.ApiSecret.IsNullOrEmpty())
            throw new Exception($"项目 [{project.Name}] 未配置 ApiSecret，无法生成访问令牌");

        if (expiresInSeconds <= 0) expiresInSeconds = DefaultExpiresInSeconds;
        if (expiresInSeconds > MaxExpiresInSeconds) expiresInSeconds = MaxExpiresInSeconds;

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds).ToUnixTimeSeconds();
        var payload = BuildPayload(projectId, expiresAt);
        var signature = ComputeHmacSha256(payload, project.ApiSecret);
        return $"{payload}:{signature}";
    }

    /// <summary>验证下载访问令牌</summary>
    /// <param name="token">访问令牌</param>
    /// <returns>成功返回声明主体，失败返回 null</returns>
    public ClaimsPrincipal ValidateToken(String token)
    {
        if (token.IsNullOrEmpty()) return null;

        try
        {
            var parts = token.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4) return null;

            var projectIdStr = parts[0];
            var tokenType = parts[1];
            var expiresStr = parts[2];
            var signature = parts[3];

            if (!Int64.TryParse(projectIdStr, out var projectId)) return null;
            if (!Int64.TryParse(expiresStr, out var expiresAt)) return null;
            if (!tokenType.EqualIgnoreCase("download")) return null;

            var expiresTime = DateTimeOffset.FromUnixTimeSeconds(expiresAt);
            if (expiresTime <= DateTimeOffset.UtcNow) return null;

            var project = FileProject.FindById(projectId);
            if (project == null || !project.Enable || project.ApiSecret.IsNullOrEmpty()) return null;

            var payload = BuildPayload(projectId, expiresAt);
            var expectedSignature = ComputeHmacSha256(payload, project.ApiSecret);
            if (!signature.EqualIgnoreCase(expectedSignature)) return null;

            var claims = new[]
            {
                new Claim("projectId", projectId.ToString()),
                new Claim("expires", expiresAt.ToString()),
                new Claim("tokenType", tokenType)
            };

            var identity = new ClaimsIdentity(claims, "DownloadAccessToken");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>获取项目编号</summary>
    /// <param name="principal">声明主体</param>
    /// <returns>项目编号</returns>
    public Int64? GetProjectId(ClaimsPrincipal principal)
    {
        var claim = principal?.FindFirst("projectId");
        return claim != null && Int64.TryParse(claim.Value, out var projectId) ? projectId : null;
    }

    private static String BuildPayload(Int64 projectId, Int64 expiresAt) => $"{projectId}:download:{expiresAt}";

    private static String ComputeHmacSha256(String data, String secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}