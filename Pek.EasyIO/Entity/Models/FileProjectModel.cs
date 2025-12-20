using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace HlktechFileStorage.Entity;

/// <summary>文件项目</summary>
public partial class FileProjectModel
{
    #region 属性
    /// <summary>编号</summary>
    public Int64 Id { get; set; }

    /// <summary>项目编码（唯一）</summary>
    public String Code { get; set; } = null!;

    /// <summary>项目名称</summary>
    public String Name { get; set; } = null!;

    /// <summary>项目描述</summary>
    public String? Description { get; set; }

    /// <summary>API密钥</summary>
    public String? ApiSecret { get; set; }

    /// <summary>最大存储空间（字节，0=不限制）</summary>
    public Int64 MaxStorageSize { get; set; }

    /// <summary>已用存储空间</summary>
    public Int64 UsedStorageSize { get; set; }

    /// <summary>单文件最大大小（字节）</summary>
    public Int64 MaxFileSize { get; set; }

    /// <summary>允许的扩展名（.jpg,.png）</summary>
    public String? AllowedExtensions { get; set; }

    /// <summary>禁止的扩展名</summary>
    public String? ForbiddenExtensions { get; set; }

    /// <summary>默认访问级别。1公开 2私有 3内部</summary>
    public Int32 DefaultAccessLevel { get; set; }

    /// <summary>每IP每分钟限制次数</summary>
    public Int32 RateLimitPerIp { get; set; }

    /// <summary>每文件每分钟限制次数</summary>
    public Int32 RateLimitPerFile { get; set; }

    /// <summary>是否启用</summary>
    public Boolean Enable { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreateTime { get; set; }

    /// <summary>更新时间</summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>备注</summary>
    public String? Remark { get; set; }
    #endregion

    #region 拷贝
    /// <summary>拷贝模型对象</summary>
    /// <param name="model">模型</param>
    public void Copy(IFileProject model)
    {
        Id = model.Id;
        Code = model.Code;
        Name = model.Name;
        Description = model.Description;
        ApiSecret = model.ApiSecret;
        MaxStorageSize = model.MaxStorageSize;
        UsedStorageSize = model.UsedStorageSize;
        MaxFileSize = model.MaxFileSize;
        AllowedExtensions = model.AllowedExtensions;
        ForbiddenExtensions = model.ForbiddenExtensions;
        DefaultAccessLevel = model.DefaultAccessLevel;
        RateLimitPerIp = model.RateLimitPerIp;
        RateLimitPerFile = model.RateLimitPerFile;
        Enable = model.Enable;
        CreateTime = model.CreateTime;
        UpdateTime = model.UpdateTime;
        Remark = model.Remark;
    }
    #endregion
}
