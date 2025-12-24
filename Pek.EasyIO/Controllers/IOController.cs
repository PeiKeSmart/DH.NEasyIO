using System.Security.Cryptography;

using HlktechFileStorage.Entity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using NewLife;
using NewLife.Log;

using Pek.EasyIO.Auth;
using Pek.EasyIO.Services;
using Pek.Helpers;
using Pek.Models;
using Pek.MVC;
using Pek.Swagger;

namespace Pek.EasyIO.Controllers;

/// <summary>文件控制器</summary>
[Produces("application/json")]
[CustomRoute(ApiVersions.V1)]
public class IOController : ApiControllerBase
{
    private readonly IFileStorageService _storageService;
    private readonly IRateLimiter _rateLimiter;
    private readonly IMemoryCache _cache;

    /// <summary>实例化文件控制器</summary>
    public IOController(IRateLimiter rateLimiter, IMemoryCache cache)
    {
        _storageService = new LocalFileStorageService();
        _rateLimiter = rateLimiter;
        _cache = cache;
    }

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

        // 自动创建目录（如果不存在）
        if (!Directory.Exists(storageRoot))
            Directory.CreateDirectory(storageRoot);

        return Path.Combine(storageRoot, relativePath).GetFullPath();
    }

    /// <summary>上传文件对象</summary>
    /// <param name="file">上传的文件</param>
    /// <param name="category">文件分类（可选）</param>
    /// <param name="businessType">业务类型（可选）</param>
    /// <param name="businessId">业务ID（可选）</param>
    /// <param name="isPublic">是否公开（可选）</param>
    /// <param name="remark">备注说明（必填）</param>
    /// <returns></returns>
    [ApiAuth]  // 上传需要API鉴权
    [HttpPut]
    public async Task<Object> Put(IFormFile file, [FromForm] String remark,
        [FromForm] String category = null, [FromForm] String businessType = null, 
        [FromForm] String businessId = null, [FromForm] Boolean isPublic = false)
    {
        var result = new DGResult();

        // 验证是否上传了文件
        if (file == null || file.Length == 0)
        {
            result.ErrCode = 10000;
            result.Message = "未上传文件或文件为空";
            return result;
        }

        // 获取原始文件名（只取文件名部分，忽略可能包含的路径）
        var originalFileName = Path.GetFileName(file.FileName);
        if (originalFileName.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = GetResource("文件名不能为空");
            return result;
        }

        // 验证备注说明是否为空
        if (remark.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = "备注说明不能为空";
            return result;
        }

        // 从鉴权信息中获取项目（已通过ApiAuthAttribute验证）
        var project = this.GetCurrentProject();
        if (project == null)
            throw new Exception("无法获取项目信息");

        // 验证文件扩展名
        var ext = Path.GetExtension(originalFileName);
        if (!ValidateExtension(ext, project))
            throw new Exception($"不支持的文件类型：{ext}");

        // 生成唯一的存储文件名（时间戳+原文件名+GUID短码+扩展名）避免同名冲突
        var originalNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
        // 清理文件名中的特殊字符，只保留字母数字中文和常见符号
        originalNameWithoutExt = System.Text.RegularExpressions.Regex.Replace(originalNameWithoutExt, @"[^\w\u4e00-\u9fa5\-_]", "_");
        // 限制原文件名长度，避免路径过长
        if (originalNameWithoutExt.Length > 50)
            originalNameWithoutExt = originalNameWithoutExt.Substring(0, 50);
        
        var now = DateTime.Now;
        var guidShort = Guid.NewGuid().ToString("N").Substring(0, 8);
        var storageName = $"{now:yyyyMMddHHmmss}_{originalNameWithoutExt}_{guidShort}{ext}";
        
        // 清理 category 参数，防止路径穿越攻击（../、..\等）
        var safeCategory = category;
        if (!category.IsNullOrEmpty())
        {
            // 移除路径分隔符和特殊字符，只保留字母数字中文横线下划线
            safeCategory = System.Text.RegularExpressions.Regex.Replace(category, @"[^\w\u4e00-\u9fa5\-]", "_");
            // 移除连续的下划线
            safeCategory = System.Text.RegularExpressions.Regex.Replace(safeCategory, @"_{2,}", "_");
            safeCategory = safeCategory.Trim('_');
        }
        
        // 按日期分片存储，避免单目录文件过多导致性能问题
        // 路径结构：[category/]YYYY/MM/DD/storageName
        // 示例：Document/2025/12/24/20251224093045_report_a1b2c3d4.pdf
        var datePath = $"{now:yyyy}/{now:MM}/{now:dd}";
        var category_path = safeCategory.IsNullOrEmpty() ? "" : safeCategory + "/";
        var relativePath = category_path + datePath + "/" + storageName;
        
        // 保存文件到项目存储目录
        var fileName = GetProjectFilePath(project, relativePath);
        fileName.EnsureDirectory(true);

        String hash;
        Int64 fileSize;

        using (var uploadStream = file.OpenReadStream())
        using (var fs = new FileStream(fileName, FileMode.Create))
        {
            // 计算哈希的同时保存文件
            using var md5 = MD5.Create();
            var buffer = new Byte[8192];
            Int32 bytesRead;
            fileSize = 0;

            while ((bytesRead = await uploadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fs.WriteAsync(buffer, 0, bytesRead);
                md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                fileSize += bytesRead;
            }

            md5.TransformFinalBlock(buffer, 0, 0);
            hash = BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
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
                    name = existing.Name,
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
                Name = storageName,  // 存储的唯一文件名
                OriginalName = originalFileName,  // 用户上传时的原始文件名
                Extension = ext,
                ContentType = GetContentType(ext),
                Size = fileSize,
                Hash = hash,

                StorageType = "Local",
                RelativePath = relativePath,  // 实际存储的相对路径

                AccessLevel = isPublic ? 1 : project.DefaultAccessLevel,
                IsPublic = isPublic,

                ProjectId = project.Id,
                ProjectName = project.Name,
                Category = safeCategory,  // 使用清理后的安全分类名

                BusinessType = businessType,
                BusinessId = businessId,

                CreateIP = GetClientIp(),
                CreateTime = DateTime.Now,
                Remark = remark
            };

            entry.Insert();

            // 更新项目存储统计
            project.UsedStorageSize += fileSize;
            project.Update();

            XTrace.WriteLine($"文件上传成功：{entry.Id} - {entry.Name} ({entry.Size.ToGMK()})");

            return new
            {
                id = entry.Id,
                name = entry.Name,
                originalName = entry.OriginalName,
                length = entry.Size,
                hash = entry.Hash,
                time = fi.LastWriteTime,
                isDirectory = false,
                projectId = entry.ProjectId,
                category = entry.Category,
                isPublic = entry.IsPublic,
                remark = entry.Remark
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
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Int64 id, Boolean inline = false)
    {
        if (id <= 0) throw new Exception("无效的文件ID");

        // 0. 限流检查（早期退出）
        var clientIp = DHWeb.GetUserHost(HttpContext) ?? "unknown";
        if (!_rateLimiter.CheckIpRateLimit(clientIp))
            return StatusCode(429, new { error = "请求过于频繁，请稍后再试" });
        if (!_rateLimiter.CheckFileRateLimit(id.ToString()))
            return StatusCode(429, new { error = "该文件下载过于频繁，请稍后再试" });

        // 1. 尝试从缓存获取文件元数据（缓存 5 分钟）
        var cacheKey = $"file_meta_{id}";
        var cached = _cache.Get<(FileEntry entry, FileProject project, String filePath, DateTime lastModified)>(cacheKey);
        
        FileEntry entry;
        FileProject fileProject;
        String filePath;
        DateTime lastModified;
        
        if (cached != default)
        {
            (entry, fileProject, filePath, lastModified) = cached;
            // 验证缓存数据仍然有效
            if (entry.IsDeleted)
            {
                _cache.Remove(cacheKey);
                throw new Exception("文件已被删除");
            }
        }
        else
        {
            // 缓存未命中，查询数据库
            entry = FileEntry.FindById(id);
            if (entry == null) throw new Exception($"文件不存在：{id}");
            if (entry.IsDeleted) throw new Exception("文件已被删除");

            fileProject = FileProject.FindById(entry.ProjectId);
            if (fileProject == null) throw new Exception("文件所属项目不存在");

            filePath = GetProjectFilePath(fileProject, entry.RelativePath);
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) throw new Exception("物理文件不存在");
            
            lastModified = fileInfo.LastWriteTimeUtc;

            // 存入缓存（5分钟过期）
            _cache.Set(cacheKey, (entry, fileProject, filePath, lastModified), TimeSpan.FromMinutes(5));
        }

        // 2. 权限验证
        var project = this.GetCurrentProject();
        if (project != null && entry.ProjectId != project.Id)
            throw new Exception("无权访问此文件");

        // 3. 访问级别验证（合并逻辑）
        var effectiveAccessLevel = Math.Max(fileProject.DefaultAccessLevel, entry.AccessLevel);
        if (project == null && effectiveAccessLevel >= 2)
            throw new Exception(effectiveAccessLevel == 2 ? "私有文件需要项目鉴权" : "内部文件仅限同项目访问");
        if (effectiveAccessLevel == 3 && project?.Id != entry.ProjectId)
            throw new Exception("内部文件仅限同项目访问");

        // 4. 业务规则验证
        if (entry.ExpiresAt != DateTime.MinValue && entry.ExpiresAt < DateTime.Now)
            throw new Exception("文件已过期");

        // 5. HTTP 缓存验证（优化字符串操作）
        var etag = $"\"{entry.Hash}-{lastModified.Ticks}\"";
        var requestETag = Request.Headers["If-None-Match"].ToString();
        
        if (requestETag == etag)
            return StatusCode(304);
        
        var requestModifiedSince = Request.Headers["If-Modified-Since"].ToString();
        if (!requestModifiedSince.IsNullOrEmpty() && 
            DateTime.TryParse(requestModifiedSince, out var modifiedSince) &&
            lastModified <= modifiedSince.ToUniversalTime())
            return StatusCode(304);

        // 6. 设置响应头（优化字符串操作）
        var contentType = entry.ContentType ?? "application/octet-stream";
        var downloadFileName = entry.OriginalName.IsNullOrEmpty() ? entry.Name : entry.OriginalName;
        
        Response.Headers.Append("Content-Disposition", $"{(inline ? "inline" : "attachment")}; filename=\"{Uri.EscapeDataString(downloadFileName)}\"");
        Response.Headers.Append("ETag", etag);
        Response.Headers.Append("Last-Modified", lastModified.ToString("R"));
        Response.Headers.Append("Cache-Control", effectiveAccessLevel == 1 ? "public, max-age=3600" : "private, no-cache");

        // 7. 异步更新下载计数（每次都计数，SaveAsync 批量写入减少数据库压力）
        entry.DownloadCount++;
        entry.SaveAsync(3000);  // 3秒内累积批量写入，配合 AdditionalFields 生成原子 SQL

        XTrace.WriteLine($"文件下载：{entry.Id} - {entry.Name} ({entry.Size.ToGMK()}) by {clientIp}");

        // 8. 返回文件（使用 PhysicalFile 获得最佳性能）
        try
        {
            var result = PhysicalFile(filePath, contentType, downloadFileName, enableRangeProcessing: true);
            
            // 下载成功，记录日志（采样：前100次 + 之后每10次，SaveAsync 批量写入）
            var shouldLogSample = entry.DownloadCount < 100 || (entry.DownloadCount % 10) == 0;
            if (shouldLogSample)
            {
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
                log.SaveAsync(3000);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            // 记录失败日志（SaveAsync 自动异步批量写入）
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
            errorLog.SaveAsync(3000);

            throw;
        }
    }

    /// <summary>删除文件对象</summary>
    /// <param name="id">文件数据库ID</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [ApiAuth]  // 删除需要API鉴权
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

    /// <summary>获取客户端IP地址（支持代理、负载均衡等场景）</summary>
    private String GetClientIp()
    {
        return DHWeb.GetUserHost(HttpContext) ?? "unknown";
    }

    #endregion
}
