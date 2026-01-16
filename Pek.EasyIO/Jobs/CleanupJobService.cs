using System.ComponentModel;

using HlktechFileStorage.Entity;

using NewLife;
using NewLife.Cube.Jobs;
using NewLife.Log;

namespace Pek.EasyIO.Jobs;

/// <summary>清理临时文件作业参数</summary>
public class CleanupJobArgument
{
    /// <summary>过期时间（小时，默认24小时）</summary>
    public Int32 ExpireHours { get; set; } = 24;
}

/// <summary>清理临时文件服务</summary>
[DisplayName("清理过期分片临时文件")]
[Description("每天自动清理超过24小时未完成的分片上传临时文件，释放磁盘空间")]
[CronJob("CleanupJob", "0 0 2 * * ? *", Enable = true)] // 每天凌晨2点执行
public class CleanupJobService : CubeJobBase<CleanupJobArgument>
{
    private readonly ITracer _tracer;

    /// <summary>实例化清理临时文件服务</summary>
    /// <param name="tracer">追踪器</param>
    public CleanupJobService(ITracer tracer)
    {
        _tracer = tracer;
    }

    /// <summary>执行作业</summary>
    /// <param name="argument">作业参数</param>
    /// <returns></returns>
    protected override async Task<String> OnExecute(CleanupJobArgument argument)
    {
        using var span = _tracer?.NewSpan("CleanupJob", argument);

        var expireHours = argument?.ExpireHours ?? 24;
        var expireTime = DateTime.Now.AddHours(-expireHours);
        var totalCleanedCount = 0;
        var totalSize = 0L;
        var projectCount = 0;
        var errorCount = 0;

        try
        {
            // 获取所有启用的项目
            var projects = FileProject.FindAll(FileProject._.Enable == true);
            if (projects == null || projects.Count == 0)
            {
                XTrace.WriteLine("未找到启用的项目，跳过清理");
                return "未找到启用的项目";
            }

            XTrace.WriteLine($"开始清理过期分片文件，共 {projects.Count} 个项目，过期时间：{expireHours} 小时");

            // 遍历所有项目
            foreach (var project in projects)
            {
                if (project.StoragePath.IsNullOrEmpty())
                {
                    XTrace.WriteLine($"项目 [{project.Name}] 未配置存储路径，跳过");
                    continue;
                }

                projectCount++;

                try
                {
                    var chunksRootDir = Path.Combine(project.StoragePath, "Temp", "Chunks");
                    if (!Directory.Exists(chunksRootDir))
                    {
                        XTrace.WriteLine($"项目 [{project.Name}] 无临时文件目录，跳过");
                        continue;
                    }

                    var projectCleanedCount = 0;
                    var projectSize = 0L;

                    // 遍历所有文件哈希目录
                    var hashDirs = Directory.GetDirectories(chunksRootDir);
                    foreach (var hashDir in hashDirs)
                    {
                        try
                        {
                            var dirInfo = new DirectoryInfo(hashDir);
                            
                            // 检查最后修改时间
                            if (dirInfo.LastWriteTime < expireTime)
                            {
                                // 计算目录大小
                                var dirSize = dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
                                projectSize += dirSize;

                                // 删除整个目录
                                Directory.Delete(hashDir, true);
                                projectCleanedCount++;

                                XTrace.WriteLine($"清理过期分片：{Path.GetFileName(hashDir)} ({dirSize.ToGMK()})");
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            XTrace.WriteLine($"清理目录失败（继续）：{hashDir} - {ex.Message}");
                        }
                    }

                    if (projectCleanedCount > 0)
                    {
                        totalCleanedCount += projectCleanedCount;
                        totalSize += projectSize;
                        XTrace.WriteLine($"项目 [{project.Name}] 清理完成：删除 {projectCleanedCount} 个目录，释放 {projectSize.ToGMK()} 空间");
                    }
                    else
                    {
                        XTrace.WriteLine($"项目 [{project.Name}] 无需清理");
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    XTrace.WriteException(ex);
                    span?.SetError(ex, null);
                }
            }

            var summary = $"清理完成：处理 {projectCount} 个项目，删除 {totalCleanedCount} 个过期分片目录，释放 {totalSize.ToGMK()} 空间";
            if (errorCount > 0)
                summary += $"，失败 {errorCount} 项";

            XTrace.WriteLine(summary);
            span?.AppendTag($"cleanedCount={totalCleanedCount}, totalSize={totalSize}, errorCount={errorCount}");

            return summary;
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            span?.SetError(ex, null);
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }
}
