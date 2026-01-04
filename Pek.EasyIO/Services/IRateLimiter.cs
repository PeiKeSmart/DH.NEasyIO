namespace Pek.EasyIO.Services;

/// <summary>限流器接口</summary>
public interface IRateLimiter
{
    /// <summary>检查IP限流</summary>
    /// <param name="ip">IP地址</param>
    /// <returns></returns>
    Boolean CheckIpRateLimit(String ip);

    /// <summary>检查文件级别IP限流</summary>
    /// <param name="ip">IP地址</param>
    /// <param name="fileId">文件ID</param>
    /// <param name="limitPerMinute">每分钟限制次数</param>
    /// <returns></returns>
    Boolean CheckFileIpRateLimit(String ip, Int64 fileId, Int32 limitPerMinute);
}
