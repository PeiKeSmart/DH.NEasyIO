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
[ApiAuth] // 启用API鉴权
public class IOController : ApiControllerBase
{
    private readonly IFileStorageService _storageService;

    /// <summary>实例化文件控制器</summary>
    public IOController() => _storageService = new LocalFileStorageService();

    private String GetPath(String id) => EasyIOSetting.Current.Path.CombinePath(id).GetFullPath();

    /// <summary>上传文件对象</summary>
    /// <param name="id">文件名称。可包含路径</param>
    /// <param name="category">文件分类（可选）</param>
    /// <param name="businessType">业务类型（可选）</param>
    /// <param name="businessId">业务ID（可选）</param>
    /// <param name="isPublic">是否公开（可选）</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<Object> Put(String id, String category = null,
        String businessType = null, String businessId = null, Boolean isPublic = false)
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

        // 保存文件到磁盘
        var fileName = GetPath(id);
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
            if (existing != null && existing.Status == 1 && !existing.IsDeleted)
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
                StoragePath = fileName,
                RelativePath = id,

                AccessLevel = isPublic ? 1 : project.DefaultAccessLevel,
                IsPublic = isPublic,

                ProjectId = project.Id,
                ProjectName = project.Name,
                Category = category,

                BusinessType = businessType,
                BusinessId = businessId,

                Status = 1,
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

    /// <summary>获取文件对象内容</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet]
    public IActionResult Get(String id)
    {
        if (id.IsNullOrEmpty()) throw new Exception("找不到记录！id=" + id);

        var fileName = GetPath(id);
        var fi = fileName.AsFile();
        if (!fi.Exists) throw new Exception("文件不存在");

        return File(fi.ReadBytes(), "application/octet-stream");
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

        var fileName = GetPath(id);
        var fi = fileName.AsFile();
        if (!fi.Exists) throw new Exception("文件不存在");

        //todo 实现计算Url
        throw new NotImplementedException();
    }

    /// <summary>删除文件对象</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpDelete]
    public Int32 Delete(String id)
    {
        if (id.IsNullOrEmpty()) throw new Exception("找不到记录！id=" + id);

        var path = GetPath(id);
        if (path.EndsWith('/') || path.EndsWith('\\'))
        {
            var di = path.AsDirectory();
            if (!di.Exists) return 0;

            di.Delete();

            return 1;
        }
        else
        {
            var fi = path.AsFile();
            if (!fi.Exists) return 0;

            fi.Delete();

            return 1;
        }
    }

    /// <summary>搜索文件</summary>
    /// <param name="pattern">匹配模式。如/202304/*.jpg</param>
    /// <param name="start">开始序号。0开始</param>
    /// <param name="count">最大个数</param>
    /// <returns></returns>
    [HttpGet]
    public virtual IList<Object> Search(String pattern, Int32 start, Int32 count)
    {
        //if (searchPattern.IsNullOrEmpty()) throw new ArgumentNullException(nameof(searchPattern));
        // 强制count默认值为100
        if (count <= 0) count = 100;
        if (start <= 0) start = 0;

        var dir = "";
        var pt = "*";
        if (!pattern.IsNullOrEmpty())
        {
            var p = pattern.LastIndexOfAny(new[] { '/', '\\' });
            if (p >= 0 && pattern[(p + 1)..].Contains('*'))
            {
                dir = pattern[..p];
                pt = pattern[(p + 1)..];
            }
            else
            {
                dir = pattern;
            }
        }

        var di = EasyIOSetting.Current.Path.CombinePath(dir).AsDirectory();
        if (!di.Exists) return null;

        var root = EasyIOSetting.Current.Path.EnsureEnd("/").GetFullPath();
        var rs = new List<Object>();

        // 子目录列表
        var dis = di.GetDirectories(pt);
        if (dis.Length > 0)
        {
            foreach (var item in dis.Skip(start).Take(count))
            {
                rs.Add(new
                {
                    name = item.FullName.TrimStart(root).Replace('\\', '/'),
                    time = item.LastWriteTime
                });
            }
            start += dis.Length;
            count -= rs.Count;
        }
        if (count == 0) return rs;

        // 文件列表
        var fis = di.GetFiles(pt);
        if (fis.Length > 0)
        {
            foreach (var item in fis.Skip(start).Take(count))
            {
                rs.Add(new
                {
                    name = item.FullName.TrimStart(root).Replace('\\', '/'),
                    time = item.LastWriteTime
                });
            }
        }

        return rs;
    }

    #region 辅助方法

    private FileProject GetOrCreateProject(String code)
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
