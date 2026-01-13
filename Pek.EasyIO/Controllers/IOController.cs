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
    /// <param name="accessLevel">访问级别（可选，0=使用项目默认值,1=Public,2=Private,3=Internal）</param>
    /// <param name="directory">指定存储目录（可选）。指定后文件存储在该目录下，为空则按年月日结构存储</param>
    /// <param name="remark">备注说明（必填）</param>
    /// <returns></returns>
    [ApiAuth]  // 上传需要API鉴权
    [HttpPut]
    public async Task<Object> Put(IFormFile file, [FromForm] String remark,
        [FromForm] String category = null, [FromForm] String businessType = null,
        [FromForm] String businessId = null, [FromForm] Int32 accessLevel = 0,
        [FromForm] String directory = null)
    {
        var result = new DGResult();

        // 验证外部用户ID（必填）
        var externalUserId = Request.Headers["X-External-UserId"].ToString();
        if (externalUserId.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = "缺少必填请求头：X-External-UserId";
            return result;
        }

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

        // 清理 directory 参数，防止路径穿越攻击
        var safeDirectory = directory;
        if (!directory.IsNullOrEmpty())
        {
            // 移除路径分隔符和特殊字符，只保留字母数字中文横线下划线和斜杠
            safeDirectory = System.Text.RegularExpressions.Regex.Replace(directory, @"[^\w\u4e00-\u9fa5\-/]", "_");
            // 移除连续的下划线和斜杠
            safeDirectory = System.Text.RegularExpressions.Regex.Replace(safeDirectory, @"_{2,}", "_");
            safeDirectory = System.Text.RegularExpressions.Regex.Replace(safeDirectory, @"/{2,}", "/");
            safeDirectory = safeDirectory.Trim('_').Trim('/');
        }

        // 根据是否指定 directory 决定存储路径结构
        String relativePath;
        if (!safeDirectory.IsNullOrEmpty())
        {
            // 指定了目录，直接使用该目录存储
            // 路径结构：directory/storageName
            // 示例：Project_A/20251224093045_report_a1b2c3d4.pdf
            relativePath = safeDirectory + "/" + storageName;
        }
        else
        {
            // 未指定目录，按日期分片存储，避免单目录文件过多导致性能问题
            // 路径结构：[category/]YYYY/MM/DD/storageName
            // 示例：Document/2025/12/24/20251224093045_report_a1b2c3d4.pdf
            var datePath = $"{now:yyyy}/{now:MM}/{now:dd}";
            var category_path = safeCategory.IsNullOrEmpty() ? "" : safeCategory + "/";
            relativePath = category_path + datePath + "/" + storageName;
        }

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

        var startTime = DateTime.Now;
        FileEntry entry = null;
        var success = false;
        var errorMessage = "";

        try
        {
            // 创建文件记录（不再进行去重检查，每次上传都创建独立记录和物理文件）
            // 这样确保：不同业务引用的文件互相独立，删除时不会影响其他引用
            entry = new FileEntry
            {
                Name = storageName,  // 存储的唯一文件名
                OriginalName = originalFileName,  // 用户上传时的原始文件名
                Extension = ext,
                ContentType = GetContentType(ext),
                Size = fileSize,
                Hash = hash,

                StorageType = "Local",
                RelativePath = relativePath,  // 实际存储的相对路径

                AccessLevel = accessLevel > 0 ? accessLevel : project.DefaultAccessLevel,

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

            success = true;
            XTrace.WriteLine($"文件上传成功：{entry.Id} - {entry.Name} ({entry.Size.ToGMK()})");

            result.Code = StateCode.Ok;
            result.Message = "文件上传成功";
            result.Data = new
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
                accessLevel = entry.AccessLevel,
                remark = entry.Remark
            };
        }
        catch (Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
            XTrace.WriteException(ex);
            result.Code = StateCode.Error;
            result.ErrCode = 50000;
            result.Message = ex.Message;
        }
        finally
        {
            // 记录操作日志
            if (entry != null)
            {
                var duration = (Int32)(DateTime.Now - startTime).TotalMilliseconds;
                FileOperationLog.Log(entry, "Upload", success, errorMessage, null, duration, externalUserId);
            }
        }

        return result;
    }

    /// <summary>下载文件（通过数据库ID）</summary>
    /// <param name="id">文件数据库ID</param>
    /// <param name="inline">是否内联显示（预览）。true=预览，false=下载</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Int64 id, [FromQuery] Boolean inline = false)
    {
        if (id <= 0) throw new Exception("无效的文件ID");

        var clientIp = DHWeb.GetUserHost(HttpContext) ?? "unknown";

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
        }
        else
        {
            // 缓存未命中,查询数据库
            entry = FileEntry.FindById(id);
            if (entry == null) throw new Exception($"文件不存在:{id}");

            fileProject = FileProject.FindById(entry.ProjectId);
            if (fileProject == null) throw new Exception("文件所属项目不存在");

            filePath = GetProjectFilePath(fileProject, entry.RelativePath);
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) throw new Exception("物理文件不存在");

            lastModified = fileInfo.LastWriteTimeUtc;

            // 存入缓存（5分钟过期）
            _cache.Set(cacheKey, (entry, fileProject, filePath, lastModified), TimeSpan.FromMinutes(5));
        }

        // 1.5. IP限流检查（文件级别优先，否则使用全局配置）
        if (entry.IpRateLimitPerMinute > 0)
        {
            // 文件级别限流
            if (!_rateLimiter.CheckFileIpRateLimit(clientIp, entry.Id, entry.IpRateLimitPerMinute))
                return StatusCode(429, new { error = "该文件访问过于频繁，请稍后再试" });
        }
        else
        {
            // 全局限流
            if (!_rateLimiter.CheckIpRateLimit(clientIp))
                return StatusCode(429, new { error = "请求过于频繁，请稍后再试" });
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

        // 4. HTTP 缓存验证（优化字符串操作）
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

    /// <summary>替换文件内容（保留ID和元数据）</summary>
    /// <param name="id">文件数据库ID</param>
    /// <param name="file">新上传的文件</param>
    /// <param name="remark">备注说明（可选，不填则保留原备注）</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [ApiAuth]  // 替换需要API鉴权
    [HttpPost("{id}/replace")]
    public async Task<Object> Replace(Int64 id, IFormFile file, [FromForm] String remark = null)
    {
        var result = new DGResult();

        // 验证外部用户ID（必填）
        var externalUserId = Request.Headers["X-External-UserId"].ToString();
        if (externalUserId.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = "缺少必填请求头：X-External-UserId";
            return result;
        }

        if (id <= 0)
        {
            result.ErrCode = 10000;
            result.Message = "无效的文件ID";
            return result;
        }

        if (file == null || file.Length == 0)
        {
            result.ErrCode = 10000;
            result.Message = "未上传文件或文件为空";
            return result;
        }

        FileEntry entry = null;
        try
        {
            // 查询原有文件记录
            entry = FileEntry.FindById(id);
            if (entry == null)
            {
                result.ErrCode = 10001;
                result.Message = "文件记录不存在";
                return result;
            }

            var project = FileProject.FindById(entry.ProjectId);
            if (project == null)
            {
                result.ErrCode = 10003;
                result.Message = "文件所属项目不存在";
                return result;
            }

            // 权限验证：只能替换本项目文件
            var currentProject = this.GetCurrentProject();
            if (currentProject?.Id != entry.ProjectId)
            {
                result.ErrCode = 10004;
                result.Message = "无权替换其他项目的文件";
                return result;
            }

            // 获取新文件信息
            var originalFileName = Path.GetFileName(file.FileName);
            if (originalFileName.IsNullOrEmpty())
            {
                result.ErrCode = 10000;
                result.Message = "文件名不能为空";
                return result;
            }

            var ext = Path.GetExtension(originalFileName);
            if (!ValidateExtension(ext, project))
            {
                result.ErrCode = 10005;
                result.Message = $"不支持的文件类型：{ext}";
                return result;
            }

            // 生成新的存储文件名（保持与原文件相同的目录结构）
            var originalNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            originalNameWithoutExt = System.Text.RegularExpressions.Regex.Replace(originalNameWithoutExt, @"[^\w\u4e00-\u9fa5\-_]", "_");
            if (originalNameWithoutExt.Length > 50)
                originalNameWithoutExt = originalNameWithoutExt.Substring(0, 50);

            var now = DateTime.Now;
            var guidShort = Guid.NewGuid().ToString("N").Substring(0, 8);
            var storageName = $"{now:yyyyMMddHHmmss}_{originalNameWithoutExt}_{guidShort}{ext}";

            // 保持原目录结构，只替换文件名
            var oldRelativePath = entry.RelativePath;
            var directory = Path.GetDirectoryName(oldRelativePath)?.Replace("\\", "/");
            var relativePath = directory.IsNullOrEmpty() ? storageName : directory + "/" + storageName;

            // 保存新文件
            var newFilePath = GetProjectFilePath(project, relativePath);
            newFilePath.EnsureDirectory(true);

            String hash;
            Int64 fileSize;

            using (var uploadStream = file.OpenReadStream())
            using (var fs = new FileStream(newFilePath, FileMode.Create))
            {
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
                System.IO.File.Delete(newFilePath);
                result.ErrCode = 10006;
                result.Message = $"文件大小超过限制（{project.MaxFileSize.ToGMK()}）";
                return result;
            }

            // 删除旧物理文件
            var oldFilePath = GetProjectFilePath(project, oldRelativePath);
            if (System.IO.File.Exists(oldFilePath))
            {
                try
                {
                    System.IO.File.Delete(oldFilePath);
                }
                catch (Exception ex)
                {
                    XTrace.WriteLine($"删除旧文件失败（继续执行）：{oldFilePath} - {ex.Message}");
                }
            }

            // 更新项目存储统计
            var sizeDiff = fileSize - entry.Size;
            project.UsedStorageSize += sizeDiff;
            project.Update();

            // 更新文件记录（保留ID、创建时间、下载次数等元数据）
            entry.Name = storageName;
            entry.OriginalName = originalFileName;
            entry.Extension = ext;
            entry.ContentType = GetContentType(ext);
            entry.Size = fileSize;
            entry.Hash = hash;
            entry.RelativePath = relativePath;
            entry.UpdateTime = now;
            entry.UpdateIP = GetClientIp();
            if (!remark.IsNullOrEmpty())
                entry.Remark = remark;

            entry.Update();

            // 清除缓存
            _cache.Remove($"file_meta_{id}");

            // 记录操作日志
            FileOperationLog.Log(entry, "Replace", true, null, null, 0, externalUserId);

            XTrace.WriteLine($"文件替换成功：{entry.Id} - {entry.Name} ({entry.Size.ToGMK()})，原大小：{entry.Size - sizeDiff}");

            result.Code = StateCode.Ok;
            result.Message = "文件替换成功";
            result.Data = new
            {
                id = entry.Id,
                name = entry.Name,
                originalName = entry.OriginalName,
                length = entry.Size,
                hash = entry.Hash,
                time = entry.UpdateTime,
                downloadCount = entry.DownloadCount,
                replaced = true
            };
            return result;
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);

            // 记录失败日志
            if (entry != null)
            {
                FileOperationLog.Log(entry, "Replace", false, ex.Message, null, 0, externalUserId);
            }

            result.Code = StateCode.Error;
            result.ErrCode = 50000;
            result.Message = $"文件替换失败：{ex.Message}";
            return result;
        }
    }

    /// <summary>重命名文件（同步修改物理文件名和原始文件名）</summary>
    /// <param name="id">文件数据库ID</param>
    /// <param name="newOriginalName">新的原始文件名（含扩展名）</param>
    /// <returns></returns>
    [ApiAuth]  // 重命名需要API鉴权
    [HttpPatch("{id}/rename")]
    public Object Rename(Int64 id, [FromForm] String newOriginalName)
    {
        var result = new DGResult();

        // 验证外部用户ID（必填）
        var externalUserId = Request.Headers["X-External-UserId"].ToString();
        if (externalUserId.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = "缺少必填请求头：X-External-UserId";
            return result;
        }

        if (id <= 0)
        {
            result.ErrCode = 10000;
            result.Message = "无效的文件ID";
            return result;
        }

        // 验证新文件名
        if (newOriginalName.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = "新文件名不能为空";
            return result;
        }

        // 清理文件名（只取文件名部分，忽略可能包含的路径）
        newOriginalName = Path.GetFileName(newOriginalName);
        if (newOriginalName.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = "文件名格式无效";
            return result;
        }

        var entry = FileEntry.FindById(id);
        if (entry == null)
        {
            result.ErrCode = 10001;
            result.Message = "文件记录不存在";
            return result;
        }

        var project = FileProject.FindById(entry.ProjectId);
        if (project == null)
        {
            result.ErrCode = 10003;
            result.Message = "文件所属项目不存在";
            return result;
        }

        // 权限验证：只能重命名本项目文件
        var currentProject = this.GetCurrentProject();
        if (currentProject?.Id != entry.ProjectId)
        {
            result.ErrCode = 10004;
            result.Message = "无权重命名其他项目的文件";
            return result;
        }

        // 验证新文件名的扩展名是否允许
        var newExt = Path.GetExtension(newOriginalName);
        if (!ValidateExtension(newExt, project))
        {
            result.ErrCode = 10005;
            result.Message = $"不支持的文件类型：{newExt}";
            return result;
        }

        var startTime = DateTime.Now;
        var success = false;
        var errorMessage = "";
        var oldOriginalName = entry.OriginalName;
        var oldName = entry.Name;
        var oldRelativePath = entry.RelativePath;

        // 获取旧物理文件路径
        var oldFilePath = GetProjectFilePath(project, entry.RelativePath);
        if (!System.IO.File.Exists(oldFilePath))
        {
            result.ErrCode = 10006;
            result.Message = "物理文件不存在，无法重命名";
            return result;
        }

        try
        {
            // 生成新的存储文件名（保留原有时间戳和GUID，只替换文件名部分）
            var nameWithoutExt = Path.GetFileNameWithoutExtension(newOriginalName);
            // 清理文件名中的特殊字符
            nameWithoutExt = System.Text.RegularExpressions.Regex.Replace(nameWithoutExt, @"[^\w\u4e00-\u9fa5\-_]", "_");
            // 限制文件名长度
            if (nameWithoutExt.Length > 50)
                nameWithoutExt = nameWithoutExt.Substring(0, 50);

            var now = DateTime.Now;
            var guidShort = Guid.NewGuid().ToString("N").Substring(0, 8);
            var newStorageName = $"{now:yyyyMMddHHmmss}_{nameWithoutExt}_{guidShort}{newExt}";

            // 计算新的相对路径（保持目录结构，只替换文件名）
            var directory = Path.GetDirectoryName(entry.RelativePath);
            var newRelativePath = directory.IsNullOrEmpty() 
                ? newStorageName 
                : Path.Combine(directory, newStorageName).Replace("\\", "/");

            // 获取新物理文件路径
            var newFilePath = GetProjectFilePath(project, newRelativePath);

            // 检查目标文件是否已存在
            if (System.IO.File.Exists(newFilePath))
            {
                result.ErrCode = 10007;
                result.Message = "目标文件名已存在";
                return result;
            }

            // 确保目标目录存在
            newFilePath.EnsureDirectory(true);

            // 重命名物理文件
            System.IO.File.Move(oldFilePath, newFilePath);
            XTrace.WriteLine($"物理文件重命名成功：{oldFilePath} -> {newFilePath}");

            // 更新数据库记录
            entry.Name = newStorageName;
            entry.OriginalName = newOriginalName;
            entry.RelativePath = newRelativePath;
            entry.Extension = newExt;
            entry.ContentType = GetContentType(newExt);
            entry.Update();

            // 清除缓存
            _cache.Remove($"file_meta_{id}");

            success = true;
            XTrace.WriteLine($"文件重命名成功：{entry.Id} - 原名：{oldOriginalName}（{oldName}），新名：{newOriginalName}（{newStorageName}）");

            result.Code = StateCode.Ok;
            result.Message = "文件重命名成功";
            result.Data = new
            {
                id = entry.Id,
                oldName,
                newName = entry.Name,
                oldOriginalName,
                newOriginalName = entry.OriginalName,
                oldRelativePath,
                newRelativePath = entry.RelativePath,
                extension = entry.Extension,
                renamed = true
            };
        }
        catch (Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
            XTrace.WriteException(ex);

            // 尝试回滚：如果物理文件已重命名但数据库更新失败，尝试恢复物理文件
            var newFilePath = GetProjectFilePath(project, entry.RelativePath);
            if (!System.IO.File.Exists(oldFilePath) && System.IO.File.Exists(newFilePath))
            {
                try
                {
                    System.IO.File.Move(newFilePath, oldFilePath);
                    XTrace.WriteLine($"重命名失败，已回滚物理文件：{newFilePath} -> {oldFilePath}");
                }
                catch (Exception rollbackEx)
                {
                    XTrace.WriteException(rollbackEx);
                    errorMessage += $"；回滚失败：{rollbackEx.Message}";
                }
            }

            result.Code = StateCode.Error;
            result.ErrCode = 50000;
            result.Message = $"重命名文件失败：{ex.Message}";
        }
        finally
        {
            // 记录操作日志
            var duration = (Int32)(DateTime.Now - startTime).TotalMilliseconds;
            FileOperationLog.Log(entry, "Rename", success, errorMessage, null, duration, externalUserId);
        }

        return result;
    }

    /// <summary>移动文件到指定目录</summary>
    /// <param name="id">文件数据库ID</param>
    /// <param name="targetDirectory">目标目录路径（相对路径，如：Images/Products 或 Documents/2024）</param>
    /// <param name="category">新的分类（可选，如：Image、Document等）</param>
    /// <returns></returns>
    [ApiAuth]  // 移动需要API鉴权
    [HttpPatch("{id}/move")]
    public Object Move(Int64 id, [FromForm] String targetDirectory, [FromForm] String category = null)
    {
        var result = new DGResult();

        // 验证外部用户ID（必填）
        var externalUserId = Request.Headers["X-External-UserId"].ToString();
        if (externalUserId.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = "缺少必填请求头：X-External-UserId";
            return result;
        }

        if (id <= 0)
        {
            result.ErrCode = 10000;
            result.Message = "无效的文件ID";
            return result;
        }

        // 验证目标目录
        if (targetDirectory.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = "目标目录不能为空";
            return result;
        }

        var entry = FileEntry.FindById(id);
        if (entry == null)
        {
            result.ErrCode = 10001;
            result.Message = "文件记录不存在";
            return result;
        }

        var project = FileProject.FindById(entry.ProjectId);
        if (project == null)
        {
            result.ErrCode = 10003;
            result.Message = "文件所属项目不存在";
            return result;
        }

        // 权限验证：只能移动本项目文件
        var currentProject = this.GetCurrentProject();
        if (currentProject?.Id != entry.ProjectId)
        {
            result.ErrCode = 10004;
            result.Message = "无权移动其他项目的文件";
            return result;
        }

        var startTime = DateTime.Now;
        var success = false;
        var errorMessage = "";
        var oldRelativePath = entry.RelativePath;
        var oldCategory = entry.Category;
        String newRelativePath = null;

        // 获取旧物理文件路径
        var oldFilePath = GetProjectFilePath(project, entry.RelativePath);
        if (!System.IO.File.Exists(oldFilePath))
        {
            result.ErrCode = 10006;
            result.Message = "物理文件不存在，无法移动";
            return result;
        }

        try
        {
            // 清理目标目录参数，防止路径穿越攻击
            var safeTargetDirectory = targetDirectory;
            // 移除路径分隔符和特殊字符，只保留字母数字中文横线下划线和斜杠
            safeTargetDirectory = System.Text.RegularExpressions.Regex.Replace(safeTargetDirectory, @"[^\w\u4e00-\u9fa5\-/]", "_");
            // 移除连续的下划线和斜杠
            safeTargetDirectory = System.Text.RegularExpressions.Regex.Replace(safeTargetDirectory, @"_{2,}", "_");
            safeTargetDirectory = System.Text.RegularExpressions.Regex.Replace(safeTargetDirectory, @"/{2,}", "/");
            safeTargetDirectory = safeTargetDirectory.Trim('_').Trim('/');

            if (safeTargetDirectory.IsNullOrEmpty())
            {
                result.ErrCode = 10000;
                result.Message = "目标目录格式无效";
                return result;
            }

            // 清理分类参数
            var safeCategory = category;
            if (!category.IsNullOrEmpty())
            {
                safeCategory = System.Text.RegularExpressions.Regex.Replace(category, @"[^\w\u4e00-\u9fa5\-]", "_");
                safeCategory = System.Text.RegularExpressions.Regex.Replace(safeCategory, @"_{2,}", "_");
                safeCategory = safeCategory.Trim('_');
            }

            // 计算新的相对路径（保持文件名不变，只改变目录）
            var fileName = Path.GetFileName(entry.RelativePath);
            newRelativePath = Path.Combine(safeTargetDirectory, fileName).Replace("\\", "/");

            // 获取新物理文件路径
            var newFilePath = GetProjectFilePath(project, newRelativePath);

            // 检查目标文件是否已存在
            if (System.IO.File.Exists(newFilePath))
            {
                result.ErrCode = 10007;
                result.Message = "目标位置已存在同名文件";
                return result;
            }

            // 确保目标目录存在
            newFilePath.EnsureDirectory(true);

            // 移动物理文件
            System.IO.File.Move(oldFilePath, newFilePath);
            XTrace.WriteLine($"物理文件移动成功：{oldFilePath} -> {newFilePath}");

            // 尝试删除旧目录（如果为空）
            try
            {
                var oldDirectory = Path.GetDirectoryName(oldFilePath);
                if (!oldDirectory.IsNullOrEmpty() && Directory.Exists(oldDirectory))
                {
                    var remainingFiles = Directory.GetFileSystemEntries(oldDirectory);
                    if (remainingFiles.Length == 0)
                    {
                        Directory.Delete(oldDirectory);
                        XTrace.WriteLine($"已删除空目录：{oldDirectory}");
                    }
                }
            }
            catch (Exception ex)
            {
                XTrace.WriteLine($"删除空目录失败（可忽略）：{ex.Message}");
            }

            // 更新数据库记录
            entry.RelativePath = newRelativePath;
            if (!safeCategory.IsNullOrEmpty())
                entry.Category = safeCategory;
            entry.Update();

            // 清除缓存
            _cache.Remove($"file_meta_{id}");

            success = true;
            XTrace.WriteLine($"文件移动成功：{entry.Id} - 从 {oldRelativePath}（{oldCategory}）移动到 {newRelativePath}（{entry.Category}）");

            result.Code = StateCode.Ok;
            result.Message = "文件移动成功";
            result.Data = new
            {
                id = entry.Id,
                name = entry.Name,
                originalName = entry.OriginalName,
                oldRelativePath,
                newRelativePath = entry.RelativePath,
                oldCategory,
                newCategory = entry.Category,
                moved = true
            };
        }
        catch (Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
            XTrace.WriteException(ex);

            // 尝试回滚：如果物理文件已移动但数据库更新失败，尝试恢复物理文件
            var newFilePath = GetProjectFilePath(project, newRelativePath ?? entry.RelativePath);
            if (!System.IO.File.Exists(oldFilePath) && System.IO.File.Exists(newFilePath))
            {
                try
                {
                    System.IO.File.Move(newFilePath, oldFilePath);
                    XTrace.WriteLine($"移动失败，已回滚物理文件：{newFilePath} -> {oldFilePath}");
                }
                catch (Exception rollbackEx)
                {
                    XTrace.WriteException(rollbackEx);
                    errorMessage += $"；回滚失败：{rollbackEx.Message}";
                }
            }

            result.Code = StateCode.Error;
            result.ErrCode = 50000;
            result.Message = $"移动文件失败：{ex.Message}";
        }
        finally
        {
            // 记录操作日志
            var duration = (Int32)(DateTime.Now - startTime).TotalMilliseconds;
            FileOperationLog.Log(entry, "Move", success, errorMessage, null, duration, externalUserId);
        }

        return result;
    }

    /// <summary>删除文件对象</summary>
    /// <param name="id">文件数据库ID</param>
    /// <returns></returns>
    [ApiAuth]  // 删除需要API鉴权
    [HttpDelete]
    public Object Delete(Int64 id)
    {
        var result = new DGResult();

        // 验证外部用户ID（必填）
        var externalUserId = Request.Headers["X-External-UserId"].ToString();
        if (externalUserId.IsNullOrEmpty())
        {
            result.ErrCode = 10000;
            result.Message = "缺少必填请求头：X-External-UserId";
            return result;
        }

        if (id <= 0)
        {
            result.ErrCode = 10000;
            result.Message = "无效的文件ID";
            return result;
        }

        var entry = FileEntry.FindById(id);
        if (entry == null)
        {
            result.ErrCode = 10001;
            result.Message = "文件记录不存在";
            return result;
        }

        var project = FileProject.FindById(entry.ProjectId);
        if (project == null)
        {
            result.ErrCode = 10003;
            result.Message = "文件所属项目不存在";
            return result;
        }

        // 权限验证：只能删除本项目文件
        var currentProject = this.GetCurrentProject();
        if (currentProject?.Id != entry.ProjectId)
        {
            result.ErrCode = 10004;
            result.Message = "无权删除其他项目的文件";
            return result;
        }

        // 获取物理文件路径
        var filePath = GetProjectFilePath(project, entry.RelativePath);
        XTrace.WriteLine($"获取物理文件路径：{filePath}");

        var startTime = DateTime.Now;
        var success = false;
        var errorMessage = "";

        try
        {
            // 先删除物理文件（如果存在）
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                XTrace.WriteLine($"物理文件已删除：{filePath}");
            }
            else
            {
                XTrace.WriteLine($"物理文件不存在（可能已被手动删除）：{filePath}");
            }

            // 再删除数据库记录
            entry.Delete();

            // 更新项目存储统计
            if (project.UsedStorageSize >= entry.Size)
                project.UsedStorageSize -= entry.Size;
            else
                project.UsedStorageSize = 0; // 防止负数
            project.Update();

            // 清除缓存
            _cache.Remove($"file_meta_{id}");

            success = true;
            XTrace.WriteLine($"文件删除成功：{entry.Id} - {entry.Name} ({entry.Size.ToGMK()})");

            result.Code = StateCode.Ok;
            result.Message = "文件删除成功";
            result.Data = new { deletedId = entry.Id };
        }
        catch (Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
            XTrace.WriteException(ex);
            result.Code = StateCode.Error;
            result.ErrCode = 50000;
            result.Message = $"删除文件失败：{ex.Message}";
        }
        finally
        {
            // 记录操作日志
            var duration = (Int32)(DateTime.Now - startTime).TotalMilliseconds;
            FileOperationLog.Log(entry, "Delete", success, errorMessage, null, duration, externalUserId);
        }

        return result;
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

    /// <summary>诊断文件信息（用于排查删除等问题）</summary>
    /// <param name="id">文件数据库ID</param>
    /// <returns></returns>
    [ApiAuth]
    [HttpGet("{id}/diagnose")]
    public Object Diagnose(Int64 id)
    {
        var result = new DGResult();

        if (id <= 0)
        {
            result.ErrCode = 10000;
            result.Message = "无效的文件ID";
            return result;
        }

        try
        {
            var entry = FileEntry.FindById(id);
            if (entry == null)
            {
                result.ErrCode = 10001;
                result.Message = "文件记录不存在";
                return result;
            }

            var project = FileProject.FindById(entry.ProjectId);
            if (project == null)
            {
                result.ErrCode = 10003;
                result.Message = "文件所属项目不存在";
                return result;
            }

            var filePath = GetProjectFilePath(project, entry.RelativePath);
            var fileExists = System.IO.File.Exists(filePath);

            FileInfo fileInfo = null;
            String filePermissions = null;
            String deleteTestResult = null;

            if (fileExists)
            {
                fileInfo = new FileInfo(filePath);

                // 测试文件权限
                try
                {
                    using var fs = System.IO.File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, System.IO.FileShare.None);
                    filePermissions = "可读写";
                }
                catch (UnauthorizedAccessException)
                {
                    filePermissions = "无写权限";
                }
                catch (IOException)
                {
                    filePermissions = "文件被占用";
                }
                catch (Exception ex)
                {
                    filePermissions = $"未知错误：{ex.Message}";
                }

                // 测试删除（不实际删除）
                try
                {
                    var testPath = filePath + ".delete_test";
                    System.IO.File.Copy(filePath, testPath, true);
                    System.IO.File.Delete(testPath);
                    deleteTestResult = "删除测试成功";
                }
                catch (Exception ex)
                {
                    deleteTestResult = $"删除测试失败：{ex.Message}";
                }
            }

            result.Code = StateCode.Ok;
            result.Message = "诊断信息获取成功";
            result.Data = new
            {
                // 数据库信息
                database = new
                {
                    id = entry.Id,
                    name = entry.Name,
                    originalName = entry.OriginalName,
                    size = entry.Size,
                    hash = entry.Hash,
                    relativePath = entry.RelativePath,
                    projectId = entry.ProjectId,
                    projectName = entry.ProjectName,
                    createTime = entry.CreateTime
                },
                // 物理文件信息
                physical = new
                {
                    fullPath = filePath,
                    exists = fileExists,
                    actualSize = fileInfo?.Length,
                    lastModified = fileInfo?.LastWriteTime,
                    isReadOnly = fileInfo?.IsReadOnly,
                    attributes = fileInfo?.Attributes.ToString(),
                    permissions = filePermissions,
                    deleteTest = deleteTestResult
                },
                // 项目信息
                project = new
                {
                    id = project.Id,
                    name = project.Name,
                    storagePath = project.StoragePath,
                    storagePathExists = Directory.Exists(project.StoragePath)
                }
            };
            return result;
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            result.Code = StateCode.Error;
            result.ErrCode = 50000;
            result.Message = $"诊断失败：{ex.Message}";
            return result;
        }
    }

    #endregion
}
