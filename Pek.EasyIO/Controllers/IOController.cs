using Microsoft.AspNetCore.Mvc;

using NewLife;

using Pek.EasyIO.Options;
using Pek.NCube.BaseControllers;

namespace Pek.EasyIO.Controllers;

/// <summary>文件控制器</summary>
public class IOController : ApiControllerBaseX
{
    private readonly FileStorageOptions _storageOptions;

    /// <summary>实例化</summary>
    /// <param name="storageOptions"></param>
    public IOController(FileStorageOptions storageOptions) => _storageOptions = storageOptions;

    private String GetPath(String id)
    {
        if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));

        var set = _storageOptions;
        if (set.Path.IsNullOrEmpty()) throw new Exception("未配置存储信息");

        return set.Path.CombinePath(id).GetFullPath();
    }

    public IActionResult Index()
    {
        return View();
    }
}
