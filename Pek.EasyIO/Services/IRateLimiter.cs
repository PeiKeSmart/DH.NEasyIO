namespace Pek.EasyIO.Services;

/// <summary>限流器接口</summary>
public interface IRateLimiter
{
    /// <summary>检查IP限流</summary>
    /// <param name="ip">IP地址</param>
    /// <returns></returns>
    Boolean CheckIpRateLimit(String ip);

    /// <summary>检查文件限流</summary>
    /// <param name="fileId">文件ID</param>
    /// <returns></returns>
    Boolean CheckFileRateLimit(String fileId);
}
