using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Cache;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace HlktechFileStorage.Entity;

/// <summary>文件分享</summary>
[Serializable]
[DataObject]
[Description("文件分享")]
[BindIndex("IU_FileShare_ShareCode", true, "ShareCode")]
[BindIndex("IX_FileShare_FileId_IsActive", false, "FileId,IsActive")]
[BindIndex("IX_FileShare_ShareUserId_CreateTime", false, "ShareUserId,CreateTime")]
[BindIndex("IX_FileShare_ExpiresAt_IsActive", false, "ExpiresAt,IsActive")]
[BindIndex("IX_FileShare_Status", false, "Status")]
[BindTable("FileShare", Description = "文件分享", ConnName = "EasyFile", DbType = DatabaseType.None)]
public partial class FileShare : IFileShare, IEntity<IFileShare>
{
    #region 属性
    private Int64 _Id;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("Id", "编号", "")]
    public Int64 Id { get => _Id; set { if (OnPropertyChanging("Id", value)) { _Id = value; OnPropertyChanged("Id"); } } }

    private Int64 _FileId;
    /// <summary>文件ID</summary>
    [DisplayName("文件ID")]
    [Description("文件ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("FileId", "文件ID", "")]
    public Int64 FileId { get => _FileId; set { if (OnPropertyChanging("FileId", value)) { _FileId = value; OnPropertyChanged("FileId"); } } }

    private String? _FileName;
    /// <summary>文件名（冗余）</summary>
    [DisplayName("文件名（冗余）")]
    [Description("文件名（冗余）")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("FileName", "文件名（冗余）", "")]
    public String? FileName { get => _FileName; set { if (OnPropertyChanging("FileName", value)) { _FileName = value; OnPropertyChanged("FileName"); } } }

    private Int64 _ProjectId;
    /// <summary>项目ID</summary>
    [DisplayName("项目ID")]
    [Description("项目ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ProjectId", "项目ID", "")]
    public Int64 ProjectId { get => _ProjectId; set { if (OnPropertyChanging("ProjectId", value)) { _ProjectId = value; OnPropertyChanged("ProjectId"); } } }

    private String _ShareCode = null!;
    /// <summary>分享码（唯一短码）</summary>
    [DisplayName("分享码（唯一短码）")]
    [Description("分享码（唯一短码）")]
    [DataObjectField(false, false, false, 32)]
    [BindColumn("ShareCode", "分享码（唯一短码）", "")]
    public String ShareCode { get => _ShareCode; set { if (OnPropertyChanging("ShareCode", value)) { _ShareCode = value; OnPropertyChanged("ShareCode"); } } }

    private String? _SharePassword;
    /// <summary>提取码（可选）</summary>
    [DisplayName("提取码（可选）")]
    [Description("提取码（可选）")]
    [DataObjectField(false, false, true, 20)]
    [BindColumn("SharePassword", "提取码（可选）", "")]
    public String? SharePassword { get => _SharePassword; set { if (OnPropertyChanging("SharePassword", value)) { _SharePassword = value; OnPropertyChanged("SharePassword"); } } }

    private DateTime _ExpiresAt;
    /// <summary>过期时间（null=永久）</summary>
    [DisplayName("过期时间（null=永久）")]
    [Description("过期时间（null=永久）")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("ExpiresAt", "过期时间（null=永久）", "")]
    public DateTime ExpiresAt { get => _ExpiresAt; set { if (OnPropertyChanging("ExpiresAt", value)) { _ExpiresAt = value; OnPropertyChanged("ExpiresAt"); } } }

    private Int32 _MaxDownloads;
    /// <summary>最大下载次数（0=不限制）</summary>
    [DisplayName("最大下载次数（0=不限制）")]
    [Description("最大下载次数（0=不限制）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MaxDownloads", "最大下载次数（0=不限制）", "")]
    public Int32 MaxDownloads { get => _MaxDownloads; set { if (OnPropertyChanging("MaxDownloads", value)) { _MaxDownloads = value; OnPropertyChanged("MaxDownloads"); } } }

    private Int32 _DownloadCount;
    /// <summary>已下载次数</summary>
    [DisplayName("已下载次数")]
    [Description("已下载次数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("DownloadCount", "已下载次数", "")]
    public Int32 DownloadCount { get => _DownloadCount; set { if (OnPropertyChanging("DownloadCount", value)) { _DownloadCount = value; OnPropertyChanged("DownloadCount"); } } }

    private Int32 _MaxViews;
    /// <summary>最大访问次数（0=不限制）</summary>
    [DisplayName("最大访问次数（0=不限制）")]
    [Description("最大访问次数（0=不限制）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MaxViews", "最大访问次数（0=不限制）", "")]
    public Int32 MaxViews { get => _MaxViews; set { if (OnPropertyChanging("MaxViews", value)) { _MaxViews = value; OnPropertyChanged("MaxViews"); } } }

    private Int32 _ViewCount;
    /// <summary>已访问次数</summary>
    [DisplayName("已访问次数")]
    [Description("已访问次数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ViewCount", "已访问次数", "")]
    public Int32 ViewCount { get => _ViewCount; set { if (OnPropertyChanging("ViewCount", value)) { _ViewCount = value; OnPropertyChanged("ViewCount"); } } }

    private String? _AllowedIps;
    /// <summary>允许的IP列表（逗号分隔，支持通配）</summary>
    [DisplayName("允许的IP列表（逗号分隔")]
    [Description("允许的IP列表（逗号分隔，支持通配）")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("AllowedIps", "允许的IP列表（逗号分隔，支持通配）", "")]
    public String? AllowedIps { get => _AllowedIps; set { if (OnPropertyChanging("AllowedIps", value)) { _AllowedIps = value; OnPropertyChanged("AllowedIps"); } } }

    private String? _DeniedIps;
    /// <summary>禁止的IP列表</summary>
    [DisplayName("禁止的IP列表")]
    [Description("禁止的IP列表")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("DeniedIps", "禁止的IP列表", "")]
    public String? DeniedIps { get => _DeniedIps; set { if (OnPropertyChanging("DeniedIps", value)) { _DeniedIps = value; OnPropertyChanged("DeniedIps"); } } }

    private Boolean _RequirePassword;
    /// <summary>是否需要提取码</summary>
    [DisplayName("是否需要提取码")]
    [Description("是否需要提取码")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("RequirePassword", "是否需要提取码", "")]
    public Boolean RequirePassword { get => _RequirePassword; set { if (OnPropertyChanging("RequirePassword", value)) { _RequirePassword = value; OnPropertyChanged("RequirePassword"); } } }

    private Boolean _AllowPreview;
    /// <summary>是否允许预览</summary>
    [DisplayName("是否允许预览")]
    [Description("是否允许预览")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("AllowPreview", "是否允许预览", "")]
    public Boolean AllowPreview { get => _AllowPreview; set { if (OnPropertyChanging("AllowPreview", value)) { _AllowPreview = value; OnPropertyChanged("AllowPreview"); } } }

    private Boolean _AllowDownload;
    /// <summary>是否允许下载</summary>
    [DisplayName("是否允许下载")]
    [Description("是否允许下载")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("AllowDownload", "是否允许下载", "")]
    public Boolean AllowDownload { get => _AllowDownload; set { if (OnPropertyChanging("AllowDownload", value)) { _AllowDownload = value; OnPropertyChanged("AllowDownload"); } } }

    private Int64 _ShareUserId;
    /// <summary>分享人ID</summary>
    [DisplayName("分享人ID")]
    [Description("分享人ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ShareUserId", "分享人ID", "")]
    public Int64 ShareUserId { get => _ShareUserId; set { if (OnPropertyChanging("ShareUserId", value)) { _ShareUserId = value; OnPropertyChanged("ShareUserId"); } } }

    private String? _ShareUserName;
    /// <summary>分享人姓名</summary>
    [DisplayName("分享人姓名")]
    [Description("分享人姓名")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("ShareUserName", "分享人姓名", "")]
    public String? ShareUserName { get => _ShareUserName; set { if (OnPropertyChanging("ShareUserName", value)) { _ShareUserName = value; OnPropertyChanged("ShareUserName"); } } }

    private String? _ShareMessage;
    /// <summary>分享留言</summary>
    [DisplayName("分享留言")]
    [Description("分享留言")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("ShareMessage", "分享留言", "")]
    public String? ShareMessage { get => _ShareMessage; set { if (OnPropertyChanging("ShareMessage", value)) { _ShareMessage = value; OnPropertyChanged("ShareMessage"); } } }

    private Boolean _IsActive;
    /// <summary>是否激活</summary>
    [DisplayName("是否激活")]
    [Description("是否激活")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("IsActive", "是否激活", "")]
    public Boolean IsActive { get => _IsActive; set { if (OnPropertyChanging("IsActive", value)) { _IsActive = value; OnPropertyChanged("IsActive"); } } }

    private Int32 _Status;
    /// <summary>状态（0=已失效,1=正常,2=已禁用）</summary>
    [DisplayName("状态（0=已失效")]
    [Description("状态（0=已失效,1=正常,2=已禁用）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Status", "状态（0=已失效,1=正常,2=已禁用）", "")]
    public Int32 Status { get => _Status; set { if (OnPropertyChanging("Status", value)) { _Status = value; OnPropertyChanged("Status"); } } }

    private DateTime _RevokedAt;
    /// <summary>撤销时间</summary>
    [DisplayName("撤销时间")]
    [Description("撤销时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("RevokedAt", "撤销时间", "")]
    public DateTime RevokedAt { get => _RevokedAt; set { if (OnPropertyChanging("RevokedAt", value)) { _RevokedAt = value; OnPropertyChanged("RevokedAt"); } } }

    private String? _RevokedReason;
    /// <summary>撤销原因</summary>
    [DisplayName("撤销原因")]
    [Description("撤销原因")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("RevokedReason", "撤销原因", "")]
    public String? RevokedReason { get => _RevokedReason; set { if (OnPropertyChanging("RevokedReason", value)) { _RevokedReason = value; OnPropertyChanged("RevokedReason"); } } }

    private DateTime _CreateTime;
    /// <summary>创建时间</summary>
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateTime", "创建时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private String? _CreateIP;
    /// <summary>创建IP</summary>
    [DisplayName("创建IP")]
    [Description("创建IP")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateIP", "创建IP", "")]
    public String? CreateIP { get => _CreateIP; set { if (OnPropertyChanging("CreateIP", value)) { _CreateIP = value; OnPropertyChanged("CreateIP"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [DisplayName("更新时间")]
    [Description("更新时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("UpdateTime", "更新时间", "")]
    public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging("UpdateTime", value)) { _UpdateTime = value; OnPropertyChanged("UpdateTime"); } } }

    private DateTime _LastAccessTime;
    /// <summary>最后访问时间</summary>
    [DisplayName("最后访问时间")]
    [Description("最后访问时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("LastAccessTime", "最后访问时间", "")]
    public DateTime LastAccessTime { get => _LastAccessTime; set { if (OnPropertyChanging("LastAccessTime", value)) { _LastAccessTime = value; OnPropertyChanged("LastAccessTime"); } } }

    private String? _Remark;
    /// <summary>备注</summary>
    [DisplayName("备注")]
    [Description("备注")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("Remark", "备注", "")]
    public String? Remark { get => _Remark; set { if (OnPropertyChanging("Remark", value)) { _Remark = value; OnPropertyChanged("Remark"); } } }
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

    #region 获取/设置 字段值
    /// <summary>获取/设置 字段值</summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    public override Object? this[String name]
    {
        get => name switch
        {
            "Id" => _Id,
            "FileId" => _FileId,
            "FileName" => _FileName,
            "ProjectId" => _ProjectId,
            "ShareCode" => _ShareCode,
            "SharePassword" => _SharePassword,
            "ExpiresAt" => _ExpiresAt,
            "MaxDownloads" => _MaxDownloads,
            "DownloadCount" => _DownloadCount,
            "MaxViews" => _MaxViews,
            "ViewCount" => _ViewCount,
            "AllowedIps" => _AllowedIps,
            "DeniedIps" => _DeniedIps,
            "RequirePassword" => _RequirePassword,
            "AllowPreview" => _AllowPreview,
            "AllowDownload" => _AllowDownload,
            "ShareUserId" => _ShareUserId,
            "ShareUserName" => _ShareUserName,
            "ShareMessage" => _ShareMessage,
            "IsActive" => _IsActive,
            "Status" => _Status,
            "RevokedAt" => _RevokedAt,
            "RevokedReason" => _RevokedReason,
            "CreateTime" => _CreateTime,
            "CreateIP" => _CreateIP,
            "UpdateTime" => _UpdateTime,
            "LastAccessTime" => _LastAccessTime,
            "Remark" => _Remark,
            _ => base[name]
        };
        set
        {
            switch (name)
            {
                case "Id": _Id = value.ToLong(); break;
                case "FileId": _FileId = value.ToLong(); break;
                case "FileName": _FileName = Convert.ToString(value); break;
                case "ProjectId": _ProjectId = value.ToLong(); break;
                case "ShareCode": _ShareCode = Convert.ToString(value); break;
                case "SharePassword": _SharePassword = Convert.ToString(value); break;
                case "ExpiresAt": _ExpiresAt = value.ToDateTime(); break;
                case "MaxDownloads": _MaxDownloads = value.ToInt(); break;
                case "DownloadCount": _DownloadCount = value.ToInt(); break;
                case "MaxViews": _MaxViews = value.ToInt(); break;
                case "ViewCount": _ViewCount = value.ToInt(); break;
                case "AllowedIps": _AllowedIps = Convert.ToString(value); break;
                case "DeniedIps": _DeniedIps = Convert.ToString(value); break;
                case "RequirePassword": _RequirePassword = value.ToBoolean(); break;
                case "AllowPreview": _AllowPreview = value.ToBoolean(); break;
                case "AllowDownload": _AllowDownload = value.ToBoolean(); break;
                case "ShareUserId": _ShareUserId = value.ToLong(); break;
                case "ShareUserName": _ShareUserName = Convert.ToString(value); break;
                case "ShareMessage": _ShareMessage = Convert.ToString(value); break;
                case "IsActive": _IsActive = value.ToBoolean(); break;
                case "Status": _Status = value.ToInt(); break;
                case "RevokedAt": _RevokedAt = value.ToDateTime(); break;
                case "RevokedReason": _RevokedReason = Convert.ToString(value); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "CreateIP": _CreateIP = Convert.ToString(value); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                case "LastAccessTime": _LastAccessTime = value.ToDateTime(); break;
                case "Remark": _Remark = Convert.ToString(value); break;
                default: base[name] = value; break;
            }
        }
    }
    #endregion

    #region 关联映射
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id">编号</param>
    /// <returns>实体对象</returns>
    public static FileShare? FindById(Int64 id)
    {
        if (id < 0) return null;

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Id == id);

        // 单对象缓存
        return Meta.SingleCache[id];

        //return Find(_.Id == id);
    }

    /// <summary>根据分享码（唯一短码）查找</summary>
    /// <param name="shareCode">分享码（唯一短码）</param>
    /// <returns>实体对象</returns>
    public static FileShare? FindByShareCode(String shareCode)
    {
        if (shareCode.IsNullOrEmpty()) return null;

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.ShareCode.EqualIgnoreCase(shareCode));

        return Find(_.ShareCode == shareCode);
    }

    /// <summary>根据状态（0=已失效查找</summary>
    /// <param name="status">状态（0=已失效</param>
    /// <returns>实体列表</returns>
    public static IList<FileShare> FindAllByStatus(Int32 status)
    {
        if (status < 0) return [];

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.Status == status);

        return FindAll(_.Status == status);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="shareCode">分享码（唯一短码）</param>
    /// <param name="shareUserId">分享人ID</param>
    /// <param name="isActive">是否激活</param>
    /// <param name="status">状态（0=已失效,1=正常,2=已禁用）</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="requirePassword">是否需要提取码</param>
    /// <param name="allowPreview">是否允许预览</param>
    /// <param name="allowDownload">是否允许下载</param>
    /// <param name="start">过期时间（null=永久）开始</param>
    /// <param name="end">过期时间（null=永久）结束</param>
    /// <param name="key">关键字</param>
    /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
    /// <returns>实体列表</returns>
    public static IList<FileShare> Search(Int64 fileId, String shareCode, Int64 shareUserId, Boolean? isActive, Int32 status, DateTime createTime, Boolean? requirePassword, Boolean? allowPreview, Boolean? allowDownload, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();

        if (fileId >= 0) exp &= _.FileId == fileId;
        if (!shareCode.IsNullOrEmpty()) exp &= _.ShareCode == shareCode;
        if (shareUserId >= 0) exp &= _.ShareUserId == shareUserId;
        if (isActive != null) exp &= _.IsActive == isActive;
        if (status >= 0) exp &= _.Status == status;
        if (requirePassword != null) exp &= _.RequirePassword == requirePassword;
        if (allowPreview != null) exp &= _.AllowPreview == allowPreview;
        if (allowDownload != null) exp &= _.AllowDownload == allowDownload;
        exp &= _.ExpiresAt.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= SearchWhereByKeys(key);

        return FindAll(exp, page);
    }
    #endregion

    #region 字段名
    /// <summary>取得文件分享字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field Id = FindByName("Id");

        /// <summary>文件ID</summary>
        public static readonly Field FileId = FindByName("FileId");

        /// <summary>文件名（冗余）</summary>
        public static readonly Field FileName = FindByName("FileName");

        /// <summary>项目ID</summary>
        public static readonly Field ProjectId = FindByName("ProjectId");

        /// <summary>分享码（唯一短码）</summary>
        public static readonly Field ShareCode = FindByName("ShareCode");

        /// <summary>提取码（可选）</summary>
        public static readonly Field SharePassword = FindByName("SharePassword");

        /// <summary>过期时间（null=永久）</summary>
        public static readonly Field ExpiresAt = FindByName("ExpiresAt");

        /// <summary>最大下载次数（0=不限制）</summary>
        public static readonly Field MaxDownloads = FindByName("MaxDownloads");

        /// <summary>已下载次数</summary>
        public static readonly Field DownloadCount = FindByName("DownloadCount");

        /// <summary>最大访问次数（0=不限制）</summary>
        public static readonly Field MaxViews = FindByName("MaxViews");

        /// <summary>已访问次数</summary>
        public static readonly Field ViewCount = FindByName("ViewCount");

        /// <summary>允许的IP列表（逗号分隔，支持通配）</summary>
        public static readonly Field AllowedIps = FindByName("AllowedIps");

        /// <summary>禁止的IP列表</summary>
        public static readonly Field DeniedIps = FindByName("DeniedIps");

        /// <summary>是否需要提取码</summary>
        public static readonly Field RequirePassword = FindByName("RequirePassword");

        /// <summary>是否允许预览</summary>
        public static readonly Field AllowPreview = FindByName("AllowPreview");

        /// <summary>是否允许下载</summary>
        public static readonly Field AllowDownload = FindByName("AllowDownload");

        /// <summary>分享人ID</summary>
        public static readonly Field ShareUserId = FindByName("ShareUserId");

        /// <summary>分享人姓名</summary>
        public static readonly Field ShareUserName = FindByName("ShareUserName");

        /// <summary>分享留言</summary>
        public static readonly Field ShareMessage = FindByName("ShareMessage");

        /// <summary>是否激活</summary>
        public static readonly Field IsActive = FindByName("IsActive");

        /// <summary>状态（0=已失效,1=正常,2=已禁用）</summary>
        public static readonly Field Status = FindByName("Status");

        /// <summary>撤销时间</summary>
        public static readonly Field RevokedAt = FindByName("RevokedAt");

        /// <summary>撤销原因</summary>
        public static readonly Field RevokedReason = FindByName("RevokedReason");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>创建IP</summary>
        public static readonly Field CreateIP = FindByName("CreateIP");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        /// <summary>最后访问时间</summary>
        public static readonly Field LastAccessTime = FindByName("LastAccessTime");

        /// <summary>备注</summary>
        public static readonly Field Remark = FindByName("Remark");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得文件分享字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String Id = "Id";

        /// <summary>文件ID</summary>
        public const String FileId = "FileId";

        /// <summary>文件名（冗余）</summary>
        public const String FileName = "FileName";

        /// <summary>项目ID</summary>
        public const String ProjectId = "ProjectId";

        /// <summary>分享码（唯一短码）</summary>
        public const String ShareCode = "ShareCode";

        /// <summary>提取码（可选）</summary>
        public const String SharePassword = "SharePassword";

        /// <summary>过期时间（null=永久）</summary>
        public const String ExpiresAt = "ExpiresAt";

        /// <summary>最大下载次数（0=不限制）</summary>
        public const String MaxDownloads = "MaxDownloads";

        /// <summary>已下载次数</summary>
        public const String DownloadCount = "DownloadCount";

        /// <summary>最大访问次数（0=不限制）</summary>
        public const String MaxViews = "MaxViews";

        /// <summary>已访问次数</summary>
        public const String ViewCount = "ViewCount";

        /// <summary>允许的IP列表（逗号分隔，支持通配）</summary>
        public const String AllowedIps = "AllowedIps";

        /// <summary>禁止的IP列表</summary>
        public const String DeniedIps = "DeniedIps";

        /// <summary>是否需要提取码</summary>
        public const String RequirePassword = "RequirePassword";

        /// <summary>是否允许预览</summary>
        public const String AllowPreview = "AllowPreview";

        /// <summary>是否允许下载</summary>
        public const String AllowDownload = "AllowDownload";

        /// <summary>分享人ID</summary>
        public const String ShareUserId = "ShareUserId";

        /// <summary>分享人姓名</summary>
        public const String ShareUserName = "ShareUserName";

        /// <summary>分享留言</summary>
        public const String ShareMessage = "ShareMessage";

        /// <summary>是否激活</summary>
        public const String IsActive = "IsActive";

        /// <summary>状态（0=已失效,1=正常,2=已禁用）</summary>
        public const String Status = "Status";

        /// <summary>撤销时间</summary>
        public const String RevokedAt = "RevokedAt";

        /// <summary>撤销原因</summary>
        public const String RevokedReason = "RevokedReason";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>创建IP</summary>
        public const String CreateIP = "CreateIP";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";

        /// <summary>最后访问时间</summary>
        public const String LastAccessTime = "LastAccessTime";

        /// <summary>备注</summary>
        public const String Remark = "Remark";
    }
    #endregion
}
