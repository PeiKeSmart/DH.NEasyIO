using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace HlktechFileStorage.Entity;

/// <summary>下载日志</summary>
public partial class DownloadLogModel
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

    /// <summary>访问方式（Direct/Token/Share）</summary>
    public String? AccessType { get; set; }

    /// <summary>分享码（如果是分享下载）</summary>
    public String? ShareCode { get; set; }

    /// <summary>用户ID（登录下载）</summary>
    public Int64 UserId { get; set; }

    /// <summary>用户名</summary>
    public String? UserName { get; set; }

    /// <summary>客户端IP</summary>
    public String? ClientIp { get; set; }

    /// <summary>用户代理</summary>
    public String? UserAgent { get; set; }

    /// <summary>来源地址</summary>
    public String? Referer { get; set; }

    /// <summary>是否成功</summary>
    public Boolean Success { get; set; }

    /// <summary>失败原因</summary>
    public String? FailReason { get; set; }

    /// <summary>响应状态码</summary>
    public Int32 ResponseCode { get; set; }

    /// <summary>传输字节数</summary>
    public Int64 BytesTransferred { get; set; }

    /// <summary>下载耗时（毫秒）</summary>
    public Int32 DownloadTime { get; set; }

    /// <summary>下载速度（字节/秒）</summary>
    public Int64 Speed { get; set; }

    /// <summary>下载时间</summary>
    public DateTime CreateTime { get; set; }

    /// <summary>追踪ID</summary>
    public String? TraceId { get; set; }
    #endregion

    #region 拷贝
    /// <summary>拷贝模型对象</summary>
    /// <param name="model">模型</param>
    public void Copy(IDownloadLog model)
    {
        Id = model.Id;
        FileId = model.FileId;
        FileName = model.FileName;
        ProjectId = model.ProjectId;
        AccessType = model.AccessType;
        ShareCode = model.ShareCode;
        UserId = model.UserId;
        UserName = model.UserName;
        ClientIp = model.ClientIp;
        UserAgent = model.UserAgent;
        Referer = model.Referer;
        Success = model.Success;
        FailReason = model.FailReason;
        ResponseCode = model.ResponseCode;
        BytesTransferred = model.BytesTransferred;
        DownloadTime = model.DownloadTime;
        Speed = model.Speed;
        CreateTime = model.CreateTime;
        TraceId = model.TraceId;
    }
    #endregion
}
