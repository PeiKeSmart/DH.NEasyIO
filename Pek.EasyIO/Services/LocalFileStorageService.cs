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
        
        // 完整存储路径
        var basePath = EasyIOSetting.Current.Path;
        var fullPath = basePath.CombinePath(relativePath).GetFullPath();
        
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
        if (!System.IO.File.Exists(file.StoragePath))
            throw new FileNotFoundException("文件不存在", file.StoragePath);
        
        Stream stream = System.IO.File.OpenRead(file.StoragePath);
        return Task.FromResult(stream);
    }

    /// <summary>删除文件</summary>
    public Task<Boolean> DeleteFileAsync(FileEntry file)
    {
        try
        {
            if (System.IO.File.Exists(file.StoragePath))
            {
                System.IO.File.Delete(file.StoragePath);
                XTrace.WriteLine($"文件已删除：{file.StoragePath}");
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
