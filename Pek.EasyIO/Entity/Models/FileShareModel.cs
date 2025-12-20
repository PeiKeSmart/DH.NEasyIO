using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace HlktechFileStorage.Entity;

/// <summary>文件分享</summary>
public partial class FileShareModel
{
    #region 属性
    /// <summary>编号</summary>
    public Int64 Id { get; set; }

    /// <summary>文件ID</summary>
    public Int64 FileId { get; set; }

    /// <summary>文件名（冗余）</summary>
    public String? FileName { get; set; }

    /// <summary>项目ID</summary>
    public Int64 ProjectId { get; set; }

    /// <summary>分享码（唯一短码）</summary>
    public String ShareCode { get; set; } = null!;

    /// <summary>提取码（可选）</summary>
    public String? SharePassword { get; set; }

    /// <summary>过期时间（null=永久）</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>最大下载次数（0=不限制）</summary>
    public Int32 MaxDownloads { get; set; }

    /// <summary>已下载次数</summary>
    public Int32 DownloadCount { get; set; }

    /// <summary>最大访问次数（0=不限制）</summary>
    public Int32 MaxViews { get; set; }

    /// <summary>已访问次数</summary>
    public Int32 ViewCount { get; set; }

    /// <summary>允许的IP列表（逗号分隔，支持通配）</summary>
    public String? AllowedIps { get; set; }

    /// <summary>禁止的IP列表</summary>
    public String? DeniedIps { get; set; }

    /// <summary>是否需要提取码</summary>
    public Boolean RequirePassword { get; set; }

    /// <summary>是否允许预览</summary>
    public Boolean AllowPreview { get; set; }

    /// <summary>是否允许下载</summary>
    public Boolean AllowDownload { get; set; }

    /// <summary>分享人ID</summary>
    public Int64 ShareUserId { get; set; }

    /// <summary>分享人姓名</summary>
    public String? ShareUserName { get; set; }

    /// <summary>分享留言</summary>
    public String? ShareMessage { get; set; }

    /// <summary>是否激活</summary>
    public Boolean IsActive { get; set; }

    /// <summary>状态（0=已失效,1=正常,2=已禁用）</summary>
    public Int32 Status { get; set; }

    /// <summary>撤销时间</summary>
    public DateTime RevokedAt { get; set; }

    /// <summary>撤销原因</summary>
    public String? RevokedReason { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreateTime { get; set; }

    /// <summary>创建IP</summary>
    public String? CreateIP { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>最后访问时间</summary>
    public DateTime LastAccessTime { get; set; }

    /// <summary>备注</summary>
    public String? Remark { get; set; }
    #endregion

    #region 拷贝
    /// <summary>拷贝模型对象</summary>
    /// <param name="model">模型</param>
    public void Copy(IFileShare model)
    {
        Id = model.Id;
        FileId = model.FileId;
        FileName = model.FileName;
        ProjectId = model.ProjectId;
        ShareCode = model.ShareCode;
        SharePassword = model.SharePassword;
        ExpiresAt = model.ExpiresAt;
        MaxDownloads = model.MaxDownloads;
        DownloadCount = model.DownloadCount;
        MaxViews = model.MaxViews;
        ViewCount = model.ViewCount;
        AllowedIps = model.AllowedIps;
        DeniedIps = model.DeniedIps;
        RequirePassword = model.RequirePassword;
        AllowPreview = model.AllowPreview;
        AllowDownload = model.AllowDownload;
        ShareUserId = model.ShareUserId;
        ShareUserName = model.ShareUserName;
        ShareMessage = model.ShareMessage;
        IsActive = model.IsActive;
        Status = model.Status;
        RevokedAt = model.RevokedAt;
        RevokedReason = model.RevokedReason;
        CreateTime = model.CreateTime;
        CreateIP = model.CreateIP;
        UpdateTime = model.UpdateTime;
        LastAccessTime = model.LastAccessTime;
        Remark = model.Remark;
    }
    #endregion
}
