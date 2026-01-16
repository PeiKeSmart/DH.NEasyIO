using System.ComponentModel;

using NewLife.Cube.Jobs;
using NewLife.Log;

namespace Pek.EasyIO.Jobs;

/// <summary>
/// 刷新大屏页面作业参数
/// </summary>
public class CleanupJobArgument
{

}

/// <summary>刷新大屏页面服务</summary>
[DisplayName("刷新大屏页面")]
[Description("刷新大屏页面")]
[CronJob("CleanupJob", "0 */10 * * * ? *", Enable = true)]
public class CleanupJobService : CubeJobBase<CleanupJobArgument>
{
    private readonly ITracer _tracer;

    /// <summary>实例化检查固件升级服务</summary>
    /// <param name="tracer"></param>
    public CleanupJobService(ITracer tracer)
    {
        _tracer = tracer;
    }

    /// <summary>执行作业</summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    protected override async Task<String> OnExecute(CleanupJobArgument argument)
    {
        using var span = _tracer?.NewSpan("CleanupJob", argument);


        

        return "OK";
    }
}
