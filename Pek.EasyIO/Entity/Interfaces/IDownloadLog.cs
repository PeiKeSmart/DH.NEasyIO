using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace HlktechFileStorage.Entity;

/// <summary>下载日志</summary>
public partial interface IDownloadLog
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

    /// <summary>访问方式（Direct/Token/Share）</summary>
    String? AccessType { get; set; }

    /// <summary>分享码（如果是分享下载）</summary>
    String? ShareCode { get; set; }

    /// <summary>用户ID（登录下载）</summary>
    Int64 UserId { get; set; }

    /// <summary>用户名</summary>
    String? UserName { get; set; }

    /// <summary>客户端IP</summary>
    String? ClientIp { get; set; }

    /// <summary>用户代理</summary>
    String? UserAgent { get; set; }

    /// <summary>来源地址</summary>
    String? Referer { get; set; }

    /// <summary>是否成功</summary>
    Boolean Success { get; set; }

    /// <summary>失败原因</summary>
    String? FailReason { get; set; }

    /// <summary>响应状态码</summary>
    Int32 ResponseCode { get; set; }

    /// <summary>传输字节数</summary>
    Int64 BytesTransferred { get; set; }

    /// <summary>下载耗时（毫秒）</summary>
    Int32 DownloadTime { get; set; }

    /// <summary>下载速度（字节/秒）</summary>
    Int64 Speed { get; set; }

    /// <summary>下载时间</summary>
    DateTime CreateTime { get; set; }

    /// <summary>追踪ID</summary>
    String? TraceId { get; set; }
    #endregion
}
