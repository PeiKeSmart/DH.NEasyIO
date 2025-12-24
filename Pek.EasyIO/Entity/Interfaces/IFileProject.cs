using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace HlktechFileStorage.Entity;

/// <summary>文件项目</summary>
public partial interface IFileProject
{
    #region 属性
    /// <summary>编号</summary>
    Int64 Id { get; set; }

    /// <summary>项目编码（唯一）</summary>
    String Code { get; set; }

    /// <summary>项目名称</summary>
    String Name { get; set; }

    /// <summary>项目描述</summary>
    String? Description { get; set; }

    /// <summary>存储根目录（相对或绝对路径）</summary>
    String? StoragePath { get; set; }

    /// <summary>API密钥</summary>
    String? ApiSecret { get; set; }

    /// <summary>最大存储空间（字节，0=不限制）</summary>
    Int64 MaxStorageSize { get; set; }

    /// <summary>已用存储空间</summary>
    Int64 UsedStorageSize { get; set; }

    /// <summary>单文件最大大小（字节）</summary>
    Int64 MaxFileSize { get; set; }

    /// <summary>允许的扩展名（.jpg,.png）</summary>
    String? AllowedExtensions { get; set; }

    /// <summary>禁止的扩展名</summary>
    String? ForbiddenExtensions { get; set; }

    /// <summary>默认访问级别。1公开 2私有 3内部</summary>
    Int32 DefaultAccessLevel { get; set; }

    /// <summary>每IP每分钟限制次数</summary>
    Int32 RateLimitPerIp { get; set; }

    /// <summary>每文件每分钟限制次数</summary>
    Int32 RateLimitPerFile { get; set; }

    /// <summary>是否启用</summary>
    Boolean Enable { get; set; }

    /// <summary>创建时间</summary>
    DateTime CreateTime { get; set; }

    /// <summary>更新时间</summary>
    DateTime UpdateTime { get; set; }

    /// <summary>备注</summary>
    String? Remark { get; set; }
    #endregion
}
