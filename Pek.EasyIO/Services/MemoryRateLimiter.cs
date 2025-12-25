using System.Collections.Concurrent;

using NewLife;

namespace Pek.EasyIO.Services;

/// <summary>基于内存的限流器</summary>
public class MemoryRateLimiter : IRateLimiter
{
    private readonly ConcurrentDictionary<String, RateLimitCounter> _counters = new();
    private readonly Int32 _ipLimitPerMinute;

    /// <summary>实例化内存限流器</summary>
    public MemoryRateLimiter()
    {
        _ipLimitPerMinute = EasyIOSetting.Current.RateLimitPerIp;
    }

    /// <summary>检查IP限流</summary>
    public Boolean CheckIpRateLimit(String ip)
    {
        if (_ipLimitPerMinute <= 0) return true;
        
        var key = $"ip:{ip}";
        return CheckLimit(key, _ipLimitPerMinute);
    }

    private Boolean CheckLimit(String key, Int32 limit)
    {
        var now = DateTime.Now;
        var counter = _counters.GetOrAdd(key, _ => new RateLimitCounter());

        lock (counter)
        {
            // 清理过期计数
            if ((now - counter.WindowStart).TotalMinutes >= 1)
            {
                counter.Count = 0;
                counter.WindowStart = now;
            }

            if (counter.Count >= limit)
                return false;

            counter.Count++;
            return true;
        }
    }

    private class RateLimitCounter
    {
        public Int32 Count { get; set; }
        public DateTime WindowStart { get; set; } = DateTime.Now;
    }
}
