using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace HlktechFileStorage.Entity;

/// <summary>文件操作日志</summary>
public partial class FileOperationLogModel
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

    /// <summary>项目名称</summary>
    public String? ProjectName { get; set; }

    /// <summary>操作类型（Upload/Delete/Update/Move/Copy）</summary>
    public String OperationType { get; set; } = null!;

    /// <summary>操作详情（JSON）</summary>
    public String? OperationDetail { get; set; }

    /// <summary>文件路径</summary>
    public String? FilePath { get; set; }

    /// <summary>文件大小（字节）</summary>
    public Int64 FileSize { get; set; }

    /// <summary>文件哈希</summary>
    public String? FileHash { get; set; }

    /// <summary>操作用户ID</summary>
    public Int64 UserId { get; set; }

    /// <summary>操作用户名</summary>
    public String? UserName { get; set; }

    /// <summary>客户端IP</summary>
    public String? ClientIp { get; set; }

    /// <summary>用户代理</summary>
    public String? UserAgent { get; set; }

    /// <summary>是否成功</summary>
    public Boolean Success { get; set; }

    /// <summary>错误信息</summary>
    public String? ErrorMessage { get; set; }

    /// <summary>操作耗时（毫秒）</summary>
    public Int32 Duration { get; set; }

    /// <summary>操作时间</summary>
    public DateTime CreateTime { get; set; }

    /// <summary>追踪ID</summary>
    public String? TraceId { get; set; }

    /// <summary>备注</summary>
    public String? Remark { get; set; }
    #endregion

    #region 拷贝
    /// <summary>拷贝模型对象</summary>
    /// <param name="model">模型</param>
    public void Copy(IFileOperationLog model)
    {
        Id = model.Id;
        FileId = model.FileId;
        FileName = model.FileName;
        ProjectId = model.ProjectId;
        ProjectName = model.ProjectName;
        OperationType = model.OperationType;
        OperationDetail = model.OperationDetail;
        FilePath = model.FilePath;
        FileSize = model.FileSize;
        FileHash = model.FileHash;
        UserId = model.UserId;
        UserName = model.UserName;
        ClientIp = model.ClientIp;
        UserAgent = model.UserAgent;
        Success = model.Success;
        ErrorMessage = model.ErrorMessage;
        Duration = model.Duration;
        CreateTime = model.CreateTime;
        TraceId = model.TraceId;
        Remark = model.Remark;
    }
    #endregion
}
