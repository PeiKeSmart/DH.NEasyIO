using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace HlktechFileStorage.Entity;

/// <summary>文件条目</summary>
public partial interface IFileEntry
{
    #region 属性
    /// <summary>编号</summary>
    Int64 Id { get; set; }

    /// <summary>文件名</summary>
    String Name { get; set; }

    /// <summary>原始文件名</summary>
    String? OriginalName { get; set; }

    /// <summary>扩展名（.jpg）</summary>
    String? Extension { get; set; }

    /// <summary>MIME类型（image/jpeg）</summary>
    String? ContentType { get; set; }

    /// <summary>文件大小（字节）</summary>
    Int64 Size { get; set; }

    /// <summary>文件哈希（MD5/SHA256）</summary>
    String? Hash { get; set; }

    /// <summary>存储类型（Local/OSS/S3）</summary>
    String? StorageType { get; set; }

    /// <summary>相对于项目存储目录的路径</summary>
    String RelativePath { get; set; }

    /// <summary>存储桶名称</summary>
    String? BucketName { get; set; }

    /// <summary>访问级别（1=Public,2=Private,3=Internal）</summary>
    Int32 AccessLevel { get; set; }

    /// <summary>已下载次数</summary>
    Int32 DownloadCount { get; set; }

    /// <summary>所属项目ID</summary>
    Int64 ProjectId { get; set; }

    /// <summary>项目名称（冗余）</summary>
    String? ProjectName { get; set; }

    /// <summary>分类（Document/Image/Video等）</summary>
    String? Category { get; set; }

    /// <summary>标签（JSON数组或逗号分隔）</summary>
    String? Tags { get; set; }

    /// <summary>业务类型</summary>
    String? BusinessType { get; set; }

    /// <summary>业务ID</summary>
    String? BusinessId { get; set; }

    /// <summary>所有者用户ID</summary>
    Int64 OwnerId { get; set; }

    /// <summary>所有者名称</summary>
    String? OwnerName { get; set; }

    /// <summary>图片宽度</summary>
    Int32 Width { get; set; }

    /// <summary>图片高度</summary>
    Int32 Height { get; set; }

    /// <summary>视频/音频时长（秒）</summary>
    Int32 Duration { get; set; }

    /// <summary>其他元数据（JSON）</summary>
    String? Metadata { get; set; }

    /// <summary>是否已删除</summary>
    Boolean IsDeleted { get; set; }

    /// <summary>删除时间</summary>
    DateTime DeletedTime { get; set; }

    /// <summary>是否已病毒扫描</summary>
    Boolean IsScanned { get; set; }

    /// <summary>是否加密存储</summary>
    Boolean IsEncrypted { get; set; }

    /// <summary>加密密钥ID</summary>
    String? EncryptionKey { get; set; }

    /// <summary>创建者ID</summary>
    Int64 CreateUserId { get; set; }

    /// <summary>创建者</summary>
    String? CreateUser { get; set; }

    /// <summary>创建时间</summary>
    DateTime CreateTime { get; set; }

    /// <summary>创建IP</summary>
    String? CreateIP { get; set; }

    /// <summary>更新者ID</summary>
    Int64 UpdateUserId { get; set; }

    /// <summary>更新者</summary>
    String? UpdateUser { get; set; }

    /// <summary>更新时间</summary>
    DateTime UpdateTime { get; set; }

    /// <summary>更新IP</summary>
    String? UpdateIP { get; set; }

    /// <summary>备注说明</summary>
    String? Remark { get; set; }

    /// <summary>IP限流-每分钟最大请求次数(0=不限制)</summary>
    Int32 IpRateLimitPerMinute { get; set; }
    #endregion
}
