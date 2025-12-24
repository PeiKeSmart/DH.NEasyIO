using System.Security.Cryptography;

using HlktechFileStorage.Entity;

using Microsoft.AspNetCore.Mvc;

using NewLife;
using NewLife.Log;

using Pek.EasyIO.Auth;
using Pek.EasyIO.Services;
using Pek.Models;
using Pek.MVC;
using Pek.Swagger;

namespace Pek.EasyIO.Controllers;

/// <summary>文件控制器</summary>
[Produces("application/json")]
[CustomRoute(ApiVersions.V1)]
[ApiAuth()] // 启用API鉴权
public class IOController : ApiControllerBase
{
    private readonly IFileStorageService _storageService;

    /// <summary>实例化文件控制器</summary>
    public IOController() => _storageService = new LocalFileStorageService();

    /// <summary>获取项目文件的存储路径</summary>
    /// <param name="project">文件项目</param>
    /// <param name="relativePath">相对路径</param>
    /// <returns></returns>
    private String GetProjectFilePath(FileProject project, String relativePath)
    {
        // 强制要求项目必须配置存储目录
        var storageRoot = project.StoragePath;
        if (storageRoot.IsNullOrEmpty())
            throw new Exception($"项目 [{project.Name}] 未配置存储目录，请在项目设置中指定 StoragePath");

        // 检查目录是否存在
        if (!Directory.Exists(storageRoot))
            throw new DirectoryNotFoundException($"项目 [{project.Name}] 的存储目录不存在：{storageRoot}");

        return Path.Combine(storageRoot, relativePath).GetFullPath();
    }

    /// <summary>上传文件对象</summary>
    /// <param name="id">文件名称。可包含路径</param>
    /// <param name="category">文件分类（可选）</param>
    /// <param name="businessType">业务类型（可选）</param>
    /// <param name="businessId">业务ID（可选）</param>
    /// <param name="isPublic">是否公开（可选）</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<Object> Put([FromForm] String id, [FromForm] String category = null,
        [FromForm] String businessType = null, [FromForm] String businessId = null, [FromForm] Boolean isPublic = false)
    {
        var result = new DGResult();

        if (id.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = GetResource("参数不能为空");
            return result;
        }

        // 从鉴权信息中获取项目（已通过ApiAuthAttribute验证）
        var project = this.GetCurrentProject();
        if (project == null)
            throw new Exception("无法获取项目信息");

        // 验证文件扩展名
        var ext = Path.GetExtension(id);
        if (!ValidateExtension(ext, project))
            throw new Exception($"不支持的文件类型：{ext}");

        // 保存文件到项目存储目录
        var fileName = GetProjectFilePath(project, id);
        fileName.EnsureDirectory(true);

        var ms = Request.Body;
        String hash;
        Int64 fileSize;

        using (var fs = new FileStream(fileName, FileMode.OpenOrCreate))
        {
            // 计算哈希的同时保存文件
            using var md5 = MD5.Create();
            var buffer = new Byte[8192];
            Int32 bytesRead;
            fileSize = 0;

            while ((bytesRead = await ms.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fs.WriteAsync(buffer, 0, bytesRead);
                md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                fileSize += bytesRead;
            }

            md5.TransformFinalBlock(buffer, 0, 0);
            hash = BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
            fs.SetLength(fileSize);
        }

        // 验证文件大小
        if (project.MaxFileSize > 0 && fileSize > project.MaxFileSize)
        {
            System.IO.File.Delete(fileName);
            throw new Exception($"文件大小超过限制（{project.MaxFileSize.ToGMK()}）");
        }

        var fi = fileName.AsFile();

        try
        {
            // 检查是否已存在相同文件（去重）
            var existing = FileEntry.FindByHash(hash);
            if (existing != null && !existing.IsDeleted)
            {
                XTrace.WriteLine($"文件已存在，返回已有记录：{existing.Id}");

                return new
                {
                    id = existing.Id,
                    name = id,
                    originalName = existing.OriginalName,
                    length = existing.Size,
                    hash = existing.Hash,
                    time = existing.CreateTime,
                    isDirectory = false,
                    duplicate = true
                };
            }

            // 创建文件记录
            var entry = new FileEntry
            {
                Name = Path.GetFileName(id),
                OriginalName = id,
                Extension = ext,
                ContentType = GetContentType(ext),
                Size = fileSize,
                Hash = hash,

                StorageType = "Local",
                RelativePath = id,  // 相对于项目存储目录的路径

                AccessLevel = isPublic ? 1 : project.DefaultAccessLevel,
                IsPublic = isPublic,

                ProjectId = project.Id,
                ProjectName = project.Name,
                Category = category,

                BusinessType = businessType,
                BusinessId = businessId,

                CreateIP = GetClientIp(),
                CreateTime = DateTime.Now
            };

            entry.Insert();

            // 更新项目存储统计
            project.UsedStorageSize += fileSize;
            project.Update();

            XTrace.WriteLine($"文件上传成功：{entry.Id} - {entry.Name} ({entry.Size.ToGMK()})");

            return new
            {
                id = entry.Id,
                name = id,
                originalName = entry.OriginalName,
                length = entry.Size,
                hash = entry.Hash,
                time = fi.LastWriteTime,
                isDirectory = false,
                projectId = entry.ProjectId,
                category = entry.Category,
                isPublic = entry.IsPublic
            };
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            throw;
        }
    }

    /// <summary>下载文件（通过数据库ID）</summary>
    /// <param name="id">文件数据库ID</param>
    /// <param name="inline">是否内联显示（预览）。true=预览，false=下载</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet]
    public async Task<IActionResult> Get(Int64 id, Boolean inline = false)
    {
        if (id <= 0) throw new Exception("无效的文件ID");

        var startTime = DateTime.Now;

        // 1. 查询文件记录
        var entry = FileEntry.FindById(id);
        if (entry == null) throw new Exception($"文件不存在：{id}");

        if (entry.IsDeleted)
            throw new Exception("文件已被删除");

        // 2. 获取当前项目（可能为空，如果鉴权禁用）
        var project = this.GetCurrentProject();

        // 3. 验证项目权限
        if (project != null && entry.ProjectId != project.Id)
            throw new Exception("无权访问此文件");

        // 4. 查询文件所属项目（用于验证访问权限和获取存储目录）
        var fileProject = FileProject.FindById(entry.ProjectId);
        if (fileProject == null)
            throw new Exception("文件所属项目不存在");

        // 5. 验证访问级别（AccessLevel: 1=Public, 2=Private, 3=Internal）
        // 取更严格的访问控制：MAX(项目级别, 文件级别)
        var effectiveAccessLevel = Math.Max(fileProject.DefaultAccessLevel, entry.AccessLevel);
        
        if (effectiveAccessLevel == 2) // Private
        {
            if (project == null)
                throw new Exception("私有文件需要项目鉴权");
        }
        else if (effectiveAccessLevel == 3) // Internal
        {
            if (project == null || project.Id != entry.ProjectId)
                throw new Exception("内部文件仅限同项目访问");
        }

        // 6. 组合完整文件路径
        var filePath = GetProjectFilePath(fileProject, entry.RelativePath);
        if (!System.IO.File.Exists(filePath))
            throw new Exception("物理文件不存在");

        // 7. 检查下载次数限制
        if (entry.MaxDownloads > 0 && entry.DownloadCount >= entry.MaxDownloads)
            throw new Exception($"文件下载次数已达上限（{entry.MaxDownloads}）");

        // 8. 检查过期时间（DateTime.MinValue 表示永久有效）
        if (entry.ExpiresAt != DateTime.MinValue && entry.ExpiresAt < DateTime.Now)
            throw new Exception("文件已过期");

        var clientIp = GetClientIp();

        try
        {
            // 9. 更新下载计数
            entry.DownloadCount++;
            entry.Update();

            // 10. 记录下载日志
            var log = new DownloadLog
            {
                FileId = entry.Id,
                FileName = entry.Name,
                ProjectId = entry.ProjectId,
                AccessType = "Direct",
                ClientIp = clientIp,
                UserAgent = Request.Headers["User-Agent"].FirstOrDefault(),
                Referer = Request.Headers["Referer"].FirstOrDefault(),
                Success = true,
                ResponseCode = 200,
                BytesTransferred = entry.Size,
                CreateTime = DateTime.Now
            };

            // 11. 返回文件流
            var stream = System.IO.File.OpenRead(filePath);
            var contentType = entry.ContentType ?? "application/octet-stream";

            // 设置 Content-Disposition
            var disposition = inline ? "inline" : "attachment";
            Response.Headers.Append("Content-Disposition", $"{disposition}; filename=\"{Uri.EscapeDataString(entry.Name)}\"");

            XTrace.WriteLine($"文件下载：{entry.Id} - {entry.Name} ({entry.Size.ToGMK()}) by {clientIp}");

            // 异步记录日志（不阻塞响应）
            _ = Task.Run(() =>
            {
                try
                {
                    log.DownloadTime = (Int32)(DateTime.Now - startTime).TotalMilliseconds;
                    log.Speed = log.DownloadTime > 0 ? log.BytesTransferred * 1000 / log.DownloadTime : 0;
                    log.Insert();
                }
                catch (Exception ex)
                {
                    XTrace.WriteException(ex);
                }
            });

            return File(stream, contentType, entry.Name, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            // 记录失败日志
            var errorLog = new DownloadLog
            {
                FileId = entry.Id,
                FileName = entry.Name,
                ProjectId = entry.ProjectId,
                AccessType = "Direct",
                ClientIp = clientIp,
                UserAgent = Request.Headers["User-Agent"].FirstOrDefault(),
                Success = false,
                FailReason = ex.Message,
                ResponseCode = 500,
                CreateTime = DateTime.Now
            };

            _ = Task.Run(() =>
            {
                try { errorLog.Insert(); }
                catch { }
            });

            throw;
        }
    }

    /// <summary>获取文件对象的访问Url</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet]
    public String GetUrl(String id)
    {
        if (id.IsNullOrEmpty()) throw new Exception("找不到记录！id=" + id);

        // TODO: 此方法已废弃，应使用基于数据库 ID 的下载方式
        throw new NotImplementedException("此方法已废弃，请使用 Get(Int64 id) 方法");
    }

    /// <summary>删除文件对象</summary>
    /// <param name="id">文件数据库ID</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpDelete]
    public Int32 Delete(Int64 id)
    {
        if (id <= 0) throw new Exception("无效的文件ID：" + id);

        var entry = FileEntry.FindById(id);
        if (entry == null) throw new Exception("文件记录不存在");

        var project = FileProject.FindById(entry.ProjectId);
        if (project == null) throw new Exception("文件所属项目不存在");

        var filePath = GetProjectFilePath(project, entry.RelativePath);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        // 删除数据库记录
        entry.Delete();
        
        return 1;
    }

    /// <summary>搜索文件（已废弃，使用数据库查询代替）</summary>
    /// <param name="pattern">匹配模式。如/202304/*.jpg</param>
    /// <param name="start">开始序号。0开始</param>
    /// <param name="count">最大个数</param>
    /// <returns></returns>
    [HttpGet]
    [Obsolete("已废弃，请使用 FileEntry.Search 方法查询数据库")]
    public virtual IList<Object> Search(String pattern, Int32 start, Int32 count)
    {
        throw new NotSupportedException("此方法已废弃，请通过 FileEntry 实体查询数据库，每个项目必须配置存储目录");
    }

    #region 辅助方法

    /// <summary>验证文件扩展名是否允许</summary>
    private Boolean ValidateExtension(String ext, FileProject project)
    {
        if (ext.IsNullOrEmpty()) return true;

        ext = ext.ToLower();

        // 检查禁止列表
        if (!project.ForbiddenExtensions.IsNullOrEmpty())
        {
            var forbidden = project.ForbiddenExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (forbidden.Any(x => x.Trim().Equals(ext, StringComparison.OrdinalIgnoreCase)))
                return false;
        }

        // 检查允许列表
        if (!project.AllowedExtensions.IsNullOrEmpty())
        {
            var allowed = project.AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return allowed.Any(x => x.Trim().Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        return true;
    }

    private String GetContentType(String ext)
    {
        return ext?.ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".doc" or ".docx" => "application/msword",
            ".xls" or ".xlsx" => "application/vnd.ms-excel",
            ".ppt" or ".pptx" => "application/vnd.ms-powerpoint",
            _ => "application/octet-stream"
        };
    }

    private String GetClientIp()
    {
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (ip.IsNullOrEmpty())
            ip = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (ip.IsNullOrEmpty())
            ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        return ip ?? "unknown";
    }

    #endregion
}
