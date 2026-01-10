using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace HlktechFileStorage.Entity;

/// <summary>文件条目</summary>
public partial class FileEntryModel
{
    #region 属性
    /// <summary>编号</summary>
    public Int64 Id { get; set; }

    /// <summary>文件名</summary>
    public String Name { get; set; } = null!;

    /// <summary>原始文件名</summary>
    public String? OriginalName { get; set; }

    /// <summary>扩展名（.jpg）</summary>
    public String? Extension { get; set; }

    /// <summary>MIME类型（image/jpeg）</summary>
    public String? ContentType { get; set; }

    /// <summary>文件大小（字节）</summary>
    public Int64 Size { get; set; }

    /// <summary>文件哈希（MD5/SHA256）</summary>
    public String? Hash { get; set; }

    /// <summary>存储类型（Local/OSS/S3）</summary>
    public String? StorageType { get; set; }

    /// <summary>相对于项目存储目录的路径</summary>
    public String RelativePath { get; set; } = null!;

    /// <summary>存储桶名称</summary>
    public String? BucketName { get; set; }

    /// <summary>访问级别（1=Public,2=Private,3=Internal）</summary>
    public Int32 AccessLevel { get; set; }

    /// <summary>已下载次数</summary>
    public Int32 DownloadCount { get; set; }

    /// <summary>所属项目ID</summary>
    public Int64 ProjectId { get; set; }

    /// <summary>项目名称（冗余）</summary>
    public String? ProjectName { get; set; }

    /// <summary>分类（Document/Image/Video等）</summary>
    public String? Category { get; set; }

    /// <summary>标签（JSON数组或逗号分隔）</summary>
    public String? Tags { get; set; }

    /// <summary>业务类型</summary>
    public String? BusinessType { get; set; }

    /// <summary>业务ID</summary>
    public String? BusinessId { get; set; }

    /// <summary>所有者用户ID</summary>
    public Int64 OwnerId { get; set; }

    /// <summary>所有者名称</summary>
    public String? OwnerName { get; set; }

    /// <summary>图片宽度</summary>
    public Int32 Width { get; set; }

    /// <summary>图片高度</summary>
    public Int32 Height { get; set; }

    /// <summary>视频/音频时长（秒）</summary>
    public Int32 Duration { get; set; }

    /// <summary>其他元数据（JSON）</summary>
    public String? Metadata { get; set; }

    /// <summary>是否已病毒扫描</summary>
    public Boolean IsScanned { get; set; }

    /// <summary>是否加密存储</summary>
    public Boolean IsEncrypted { get; set; }

    /// <summary>加密密钥ID</summary>
    public String? EncryptionKey { get; set; }

    /// <summary>创建者ID</summary>
    public Int64 CreateUserId { get; set; }

    /// <summary>创建者</summary>
    public String? CreateUser { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreateTime { get; set; }

    /// <summary>创建IP</summary>
    public String? CreateIP { get; set; }

    /// <summary>更新者ID</summary>
    public Int64 UpdateUserId { get; set; }

    /// <summary>更新者</summary>
    public String? UpdateUser { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>更新IP</summary>
    public String? UpdateIP { get; set; }

    /// <summary>备注说明</summary>
    public String? Remark { get; set; }

    /// <summary>IP限流-每分钟最大请求次数(0=不限制)</summary>
    public Int32 IpRateLimitPerMinute { get; set; }
    #endregion

    #region 拷贝
    /// <summary>拷贝模型对象</summary>
    /// <param name="model">模型</param>
    public void Copy(IFileEntry model)
    {
        Id = model.Id;
        Name = model.Name;
        OriginalName = model.OriginalName;
        Extension = model.Extension;
        ContentType = model.ContentType;
        Size = model.Size;
        Hash = model.Hash;
        StorageType = model.StorageType;
        RelativePath = model.RelativePath;
        BucketName = model.BucketName;
        AccessLevel = model.AccessLevel;
        DownloadCount = model.DownloadCount;
        ProjectId = model.ProjectId;
        ProjectName = model.ProjectName;
        Category = model.Category;
        Tags = model.Tags;
        BusinessType = model.BusinessType;
        BusinessId = model.BusinessId;
        OwnerId = model.OwnerId;
        OwnerName = model.OwnerName;
        Width = model.Width;
        Height = model.Height;
        Duration = model.Duration;
        Metadata = model.Metadata;
        IsScanned = model.IsScanned;
        IsEncrypted = model.IsEncrypted;
        EncryptionKey = model.EncryptionKey;
        CreateUserId = model.CreateUserId;
        CreateUser = model.CreateUser;
        CreateTime = model.CreateTime;
        CreateIP = model.CreateIP;
        UpdateUserId = model.UpdateUserId;
        UpdateUser = model.UpdateUser;
        UpdateTime = model.UpdateTime;
        UpdateIP = model.UpdateIP;
        Remark = model.Remark;
        IpRateLimitPerMinute = model.IpRateLimitPerMinute;
    }
    #endregion
}
