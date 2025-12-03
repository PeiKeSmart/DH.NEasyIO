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
}
