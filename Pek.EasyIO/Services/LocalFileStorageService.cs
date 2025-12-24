using Microsoft.AspNetCore.Http;

using NewLife;
using NewLife.Log;

using HlktechFileStorage.Entity;

namespace Pek.EasyIO.Services;

/// <summary>本地文件存储服务</summary>
public class LocalFileStorageService : IFileStorageService
{
    /// <summary>保存文件</summary>
    public async Task<StorageResult> SaveFileAsync(Stream stream, String fileName, FileProject project)
    {
        // 生成相对路径：{年}/{月}/{日}/{GUID}_{原文件名}
        var now = DateTime.Now;
        var relativePath = $"{now:yyyy}/{now:MM}/{now:dd}/{Guid.NewGuid():N}_{fileName}";
        
        // 获取项目存储根目录
        var storageRoot = project.StoragePath;
        if (storageRoot.IsNullOrEmpty())
            throw new Exception($"项目 [{project.Name}] 未配置存储目录，请在项目设置中指定 StoragePath");

        // 检查根目录是否存在，不存在则创建
        if (!Directory.Exists(storageRoot))
        {
            XTrace.WriteLine($"项目 [{project.Name}] 的存储根目录不存在，正在创建：{storageRoot}");
            Directory.CreateDirectory(storageRoot);
        }
            
        // 完整存储路径
        var fullPath = Path.Combine(storageRoot, relativePath).GetFullPath();
        
        // 确保目录存在
        fullPath.EnsureDirectory(true);
        
        // 保存文件
        using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fs);
        
        XTrace.WriteLine($"文件已保存到：{fullPath}");
        
        return new StorageResult
        {
            FileName = fileName,
            StorageType = "Local",
            StoragePath = fullPath,
            RelativePath = relativePath,
            BucketName = null
        };
    }

    /// <summary>保存文件（IFormFile）</summary>
    public async Task<StorageResult> SaveFileAsync(IFormFile file, FileProject project)
    {
        using var stream = file.OpenReadStream();
        return await SaveFileAsync(stream, file.FileName, project);
    }

    /// <summary>获取文件流</summary>
    public Task<Stream> GetFileStreamAsync(FileEntry file)
    {
        if (!System.IO.File.Exists(file.RelativePath))
            throw new FileNotFoundException("文件不存在", file.RelativePath);
        
        Stream stream = System.IO.File.OpenRead(file.RelativePath);
        return Task.FromResult(stream);
    }

    /// <summary>删除文件</summary>
    public Task<Boolean> DeleteFileAsync(FileEntry file)
    {
        try
        {
            if (System.IO.File.Exists(file.RelativePath))
            {
                System.IO.File.Delete(file.RelativePath);
                XTrace.WriteLine($"文件已删除：{file.RelativePath}");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            return Task.FromResult(false);
        }
    }
}
