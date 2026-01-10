using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace HlktechFileStorage.Entity;

/// <summary>文件操作日志</summary>
public partial interface IFileOperationLog
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

    /// <summary>项目名称</summary>
    String? ProjectName { get; set; }

    /// <summary>操作类型（Upload/Delete/Update/Move/Copy）</summary>
    String OperationType { get; set; }

    /// <summary>操作详情（JSON）</summary>
    String? OperationDetail { get; set; }

    /// <summary>文件路径</summary>
    String? FilePath { get; set; }

    /// <summary>文件大小（字节）</summary>
    Int64 FileSize { get; set; }

    /// <summary>文件哈希</summary>
    String? FileHash { get; set; }

    /// <summary>操作用户ID</summary>
    Int64 UserId { get; set; }

    /// <summary>操作用户名</summary>
    String? UserName { get; set; }

    /// <summary>客户端IP</summary>
    String? ClientIp { get; set; }

    /// <summary>用户代理</summary>
    String? UserAgent { get; set; }

    /// <summary>是否成功</summary>
    Boolean Success { get; set; }

    /// <summary>错误信息</summary>
    String? ErrorMessage { get; set; }

    /// <summary>操作耗时（毫秒）</summary>
    Int32 Duration { get; set; }

    /// <summary>操作时间</summary>
    DateTime CreateTime { get; set; }

    /// <summary>追踪ID</summary>
    String? TraceId { get; set; }

    /// <summary>备注</summary>
    String? Remark { get; set; }
    #endregion
}
