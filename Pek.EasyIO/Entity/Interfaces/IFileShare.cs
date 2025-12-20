using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace HlktechFileStorage.Entity;

/// <summary>文件分享</summary>
public partial interface IFileShare
{
    #region 属性
    /// <summary>编号</summary>
    Int64 Id { get; set; }

    /// <summary>文件ID</summary>
    Int64 FileId { get; set; }

    /// <summary>文件名（冗余）</summary>
    String? FileName { get; set; }

    /// <summary>项目ID</summary>
    Int64 ProjectId { get; set; }

    /// <summary>分享码（唯一短码）</summary>
    String ShareCode { get; set; }

    /// <summary>提取码（可选）</summary>
    String? SharePassword { get; set; }

    /// <summary>过期时间（null=永久）</summary>
    DateTime ExpiresAt { get; set; }

    /// <summary>最大下载次数（0=不限制）</summary>
    Int32 MaxDownloads { get; set; }

    /// <summary>已下载次数</summary>
    Int32 DownloadCount { get; set; }

    /// <summary>最大访问次数（0=不限制）</summary>
    Int32 MaxViews { get; set; }

    /// <summary>已访问次数</summary>
    Int32 ViewCount { get; set; }

    /// <summary>允许的IP列表（逗号分隔，支持通配）</summary>
    String? AllowedIps { get; set; }

    /// <summary>禁止的IP列表</summary>
    String? DeniedIps { get; set; }

    /// <summary>是否需要提取码</summary>
    Boolean RequirePassword { get; set; }

    /// <summary>是否允许预览</summary>
    Boolean AllowPreview { get; set; }

    /// <summary>是否允许下载</summary>
    Boolean AllowDownload { get; set; }

    /// <summary>分享人ID</summary>
    Int64 ShareUserId { get; set; }

    /// <summary>分享人姓名</summary>
    String? ShareUserName { get; set; }

    /// <summary>分享留言</summary>
    String? ShareMessage { get; set; }

    /// <summary>是否激活</summary>
    Boolean IsActive { get; set; }

    /// <summary>状态（0=已失效,1=正常,2=已禁用）</summary>
    Int32 Status { get; set; }

    /// <summary>撤销时间</summary>
    DateTime RevokedAt { get; set; }

    /// <summary>撤销原因</summary>
    String? RevokedReason { get; set; }

    /// <summary>创建时间</summary>
    DateTime CreateTime { get; set; }

    /// <summary>创建IP</summary>
    String? CreateIP { get; set; }

    /// <summary>更新时间</summary>
    DateTime UpdateTime { get; set; }

    /// <summary>最后访问时间</summary>
    DateTime LastAccessTime { get; set; }

    /// <summary>备注</summary>
    String? Remark { get; set; }
    #endregion
}
