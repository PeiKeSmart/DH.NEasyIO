using System.Reflection;

using NewLife.Log;

using Pek.EasyIO.Services;

namespace Pek.EasyIO;

/// <summary>EasyIO服务</summary>
public static class EasyIOService
{
    /// <summary>添加EasyIO</summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEasyIO(this IServiceCollection services)
    {
        using var span = DefaultTracer.Instance?.NewSpan(nameof(AddEasyIO));

        //XTrace.WriteLine("{0} Start 配置EasyIO {0}", new String('=', 32));
        Assembly.GetExecutingAssembly().WriteVersion();

        // 注册限流器
        services.AddSingleton<IRateLimiter, MemoryRateLimiter>();

        //XTrace.WriteLine("{0} End   配置EasyIO {0}", new String('=', 32));

        return services;
    }

    /// <summary>使用EasyIO</summary>
    /// <param name="app"></param>
    /// <param name="useHomeIndex"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseEasyIO(this IApplicationBuilder app, Boolean useHomeIndex)
    {
        using var span = DefaultTracer.Instance?.NewSpan(nameof(UseEasyIO));

        //XTrace.WriteLine("{0} Start 初始化EasyIO {0}", new String('=', 32));

        var env = app.ApplicationServices.GetService<IWebHostEnvironment>();

        app.RegisterService("EasyIO", null, env.EnvironmentName);

        //XTrace.WriteLine("{0} End   初始化EasyIO {0}", new String('=', 32));

        return app;
    }
}