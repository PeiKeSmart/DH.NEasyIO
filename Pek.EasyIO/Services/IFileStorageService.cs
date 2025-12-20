using HlktechFileStorage.Entity;

namespace Pek.EasyIO.Services;

/// <summary>文件存储服务接口</summary>
public interface IFileStorageService
{
    /// <summary>保存文件</summary>
    /// <param name="stream">文件流</param>
    /// <param name="fileName">文件名</param>
    /// <param name="project">项目配置</param>
    /// <returns></returns>
    Task<StorageResult> SaveFileAsync(Stream stream, String fileName, FileProject project);

    /// <summary>保存文件（IFormFile）</summary>
    /// <param name="file">表单文件</param>
    /// <param name="project">项目配置</param>
    /// <returns></returns>
    Task<StorageResult> SaveFileAsync(Microsoft.AspNetCore.Http.IFormFile file, FileProject project);

    /// <summary>获取文件流</summary>
    /// <param name="file">文件实体</param>
    /// <returns></returns>
    Task<Stream> GetFileStreamAsync(FileEntry file);

    /// <summary>删除文件</summary>
    /// <param name="file">文件实体</param>
    /// <returns></returns>
    Task<Boolean> DeleteFileAsync(FileEntry file);
}

/// <summary>存储结果</summary>
public class StorageResult
{
    /// <summary>文件名</summary>
    public String FileName { get; set; }

    /// <summary>存储类型</summary>
    public String StorageType { get; set; }

    /// <summary>存储完整路径</summary>
    public String StoragePath { get; set; }

    /// <summary>相对路径</summary>
    public String RelativePath { get; set; }

    /// <summary>存储桶名称</summary>
    public String BucketName { get; set; }
}
