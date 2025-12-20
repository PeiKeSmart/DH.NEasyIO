using System.ComponentModel;

using NewLife.Configuration;

namespace Pek.EasyIO;

/// <summary>文件存储配置</summary>
[Config("EasyIO")]
public class EasyIOSetting : Config<EasyIOSetting>
{
    /// <summary>路径</summary>
    [Description("路径")]
    public String Path { get; set; } = "../files";

    /// <summary>是否启用Api鉴权</summary>
    [Description("是否启用Api鉴权")]
    public Boolean ApiAuthEnabled { get; set; } = true;

    /// <summary>每IP每分钟最大请求数</summary>
    [Description("每IP每分钟最大请求数")]
    public Int32 RateLimitPerIp { get; set; } = 10;

    /// <summary>每文件每分钟最大下载次数</summary>
    [Description("每文件每分钟最大下载次数")]
    public Int32 RateLimitPerFile { get; set; } = 20;
}
