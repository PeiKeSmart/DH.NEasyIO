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

/// <summary>下载日志</summary>
[Serializable]
[DataObject]
[Description("下载日志")]
[BindIndex("IX_DownloadLog_FileId_CreateTime", false, "FileId,CreateTime")]
[BindIndex("IX_DownloadLog_ClientIp_CreateTime", false, "ClientIp,CreateTime")]
[BindIndex("IX_DownloadLog_ProjectId_CreateTime", false, "ProjectId,CreateTime")]
[BindIndex("IX_DownloadLog_UserId_CreateTime", false, "UserId,CreateTime")]
[BindIndex("IX_DownloadLog_CreateTime", false, "CreateTime")]
[BindTable("DownloadLog", Description = "下载日志", ConnName = "EasyFile", DbType = DatabaseType.None)]
public partial class DownloadLog : IDownloadLog, IEntity<IDownloadLog>
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

    private String? _AccessType;
    /// <summary>访问方式（Direct/Token/Share）</summary>
    [DisplayName("访问方式（Direct_Token_Share）")]
    [Description("访问方式（Direct/Token/Share）")]
    [DataObjectField(false, false, true, 20)]
    [BindColumn("AccessType", "访问方式（Direct/Token/Share）", "")]
    public String? AccessType { get => _AccessType; set { if (OnPropertyChanging("AccessType", value)) { _AccessType = value; OnPropertyChanged("AccessType"); } } }

    private String? _ShareCode;
    /// <summary>分享码（如果是分享下载）</summary>
    [DisplayName("分享码（如果是分享下载）")]
    [Description("分享码（如果是分享下载）")]
    [DataObjectField(false, false, true, 32)]
    [BindColumn("ShareCode", "分享码（如果是分享下载）", "")]
    public String? ShareCode { get => _ShareCode; set { if (OnPropertyChanging("ShareCode", value)) { _ShareCode = value; OnPropertyChanged("ShareCode"); } } }

    private Int64 _UserId;
    /// <summary>用户ID（登录下载）</summary>
    [DisplayName("用户ID（登录下载）")]
    [Description("用户ID（登录下载）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UserId", "用户ID（登录下载）", "")]
    public Int64 UserId { get => _UserId; set { if (OnPropertyChanging("UserId", value)) { _UserId = value; OnPropertyChanged("UserId"); } } }

    private String? _UserName;
    /// <summary>用户名</summary>
    [DisplayName("用户名")]
    [Description("用户名")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("UserName", "用户名", "")]
    public String? UserName { get => _UserName; set { if (OnPropertyChanging("UserName", value)) { _UserName = value; OnPropertyChanged("UserName"); } } }

    private String? _ClientIp;
    /// <summary>客户端IP</summary>
    [DisplayName("客户端IP")]
    [Description("客户端IP")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("ClientIp", "客户端IP", "")]
    public String? ClientIp { get => _ClientIp; set { if (OnPropertyChanging("ClientIp", value)) { _ClientIp = value; OnPropertyChanged("ClientIp"); } } }

    private String? _UserAgent;
    /// <summary>用户代理</summary>
    [DisplayName("用户代理")]
    [Description("用户代理")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("UserAgent", "用户代理", "")]
    public String? UserAgent { get => _UserAgent; set { if (OnPropertyChanging("UserAgent", value)) { _UserAgent = value; OnPropertyChanged("UserAgent"); } } }

    private String? _Referer;
    /// <summary>来源地址</summary>
    [DisplayName("来源地址")]
    [Description("来源地址")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("Referer", "来源地址", "")]
    public String? Referer { get => _Referer; set { if (OnPropertyChanging("Referer", value)) { _Referer = value; OnPropertyChanged("Referer"); } } }

    private Boolean _Success;
    /// <summary>是否成功</summary>
    [DisplayName("是否成功")]
    [Description("是否成功")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Success", "是否成功", "")]
    public Boolean Success { get => _Success; set { if (OnPropertyChanging("Success", value)) { _Success = value; OnPropertyChanged("Success"); } } }

    private String? _FailReason;
    /// <summary>失败原因</summary>
    [DisplayName("失败原因")]
    [Description("失败原因")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("FailReason", "失败原因", "")]
    public String? FailReason { get => _FailReason; set { if (OnPropertyChanging("FailReason", value)) { _FailReason = value; OnPropertyChanged("FailReason"); } } }

    private Int32 _ResponseCode;
    /// <summary>响应状态码</summary>
    [DisplayName("响应状态码")]
    [Description("响应状态码")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ResponseCode", "响应状态码", "")]
    public Int32 ResponseCode { get => _ResponseCode; set { if (OnPropertyChanging("ResponseCode", value)) { _ResponseCode = value; OnPropertyChanged("ResponseCode"); } } }

    private Int64 _BytesTransferred;
    /// <summary>传输字节数</summary>
    [DisplayName("传输字节数")]
    [Description("传输字节数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("BytesTransferred", "传输字节数", "")]
    public Int64 BytesTransferred { get => _BytesTransferred; set { if (OnPropertyChanging("BytesTransferred", value)) { _BytesTransferred = value; OnPropertyChanged("BytesTransferred"); } } }

    private Int32 _DownloadTime;
    /// <summary>下载耗时（毫秒）</summary>
    [DisplayName("下载耗时（毫秒）")]
    [Description("下载耗时（毫秒）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("DownloadTime", "下载耗时（毫秒）", "")]
    public Int32 DownloadTime { get => _DownloadTime; set { if (OnPropertyChanging("DownloadTime", value)) { _DownloadTime = value; OnPropertyChanged("DownloadTime"); } } }

    private Int64 _Speed;
    /// <summary>下载速度（字节/秒）</summary>
    [DisplayName("下载速度（字节_秒）")]
    [Description("下载速度（字节/秒）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Speed", "下载速度（字节/秒）", "")]
    public Int64 Speed { get => _Speed; set { if (OnPropertyChanging("Speed", value)) { _Speed = value; OnPropertyChanged("Speed"); } } }

    private DateTime _CreateTime;
    /// <summary>下载时间</summary>
    [DisplayName("下载时间")]
    [Description("下载时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateTime", "下载时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private String? _TraceId;
    /// <summary>追踪ID</summary>
    [DisplayName("追踪ID")]
    [Description("追踪ID")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("TraceId", "追踪ID", "")]
    public String? TraceId { get => _TraceId; set { if (OnPropertyChanging("TraceId", value)) { _TraceId = value; OnPropertyChanged("TraceId"); } } }
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
            "AccessType" => _AccessType,
            "ShareCode" => _ShareCode,
            "UserId" => _UserId,
            "UserName" => _UserName,
            "ClientIp" => _ClientIp,
            "UserAgent" => _UserAgent,
            "Referer" => _Referer,
            "Success" => _Success,
            "FailReason" => _FailReason,
            "ResponseCode" => _ResponseCode,
            "BytesTransferred" => _BytesTransferred,
            "DownloadTime" => _DownloadTime,
            "Speed" => _Speed,
            "CreateTime" => _CreateTime,
            "TraceId" => _TraceId,
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
                case "AccessType": _AccessType = Convert.ToString(value); break;
                case "ShareCode": _ShareCode = Convert.ToString(value); break;
                case "UserId": _UserId = value.ToLong(); break;
                case "UserName": _UserName = Convert.ToString(value); break;
                case "ClientIp": _ClientIp = Convert.ToString(value); break;
                case "UserAgent": _UserAgent = Convert.ToString(value); break;
                case "Referer": _Referer = Convert.ToString(value); break;
                case "Success": _Success = value.ToBoolean(); break;
                case "FailReason": _FailReason = Convert.ToString(value); break;
                case "ResponseCode": _ResponseCode = value.ToInt(); break;
                case "BytesTransferred": _BytesTransferred = value.ToLong(); break;
                case "DownloadTime": _DownloadTime = value.ToInt(); break;
                case "Speed": _Speed = value.ToLong(); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "TraceId": _TraceId = Convert.ToString(value); break;
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
    public static DownloadLog? FindById(Int64 id)
    {
        if (id < 0) return null;

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Id == id);

        // 单对象缓存
        return Meta.SingleCache[id];

        //return Find(_.Id == id);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="projectId">项目ID</param>
    /// <param name="userId">用户ID（登录下载）</param>
    /// <param name="clientIp">客户端IP</param>
    /// <param name="success">是否成功</param>
    /// <param name="start">下载时间开始</param>
    /// <param name="end">下载时间结束</param>
    /// <param name="key">关键字</param>
    /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
    /// <returns>实体列表</returns>
    public static IList<DownloadLog> Search(Int64 fileId, Int64 projectId, Int64 userId, String? clientIp, Boolean? success, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();

        if (fileId >= 0) exp &= _.FileId == fileId;
        if (projectId >= 0) exp &= _.ProjectId == projectId;
        if (userId >= 0) exp &= _.UserId == userId;
        if (!clientIp.IsNullOrEmpty()) exp &= _.ClientIp == clientIp;
        if (success != null) exp &= _.Success == success;
        exp &= _.CreateTime.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= SearchWhereByKeys(key);

        return FindAll(exp, page);
    }
    #endregion

    #region 字段名
    /// <summary>取得下载日志字段信息的快捷方式</summary>
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

        /// <summary>访问方式（Direct/Token/Share）</summary>
        public static readonly Field AccessType = FindByName("AccessType");

        /// <summary>分享码（如果是分享下载）</summary>
        public static readonly Field ShareCode = FindByName("ShareCode");

        /// <summary>用户ID（登录下载）</summary>
        public static readonly Field UserId = FindByName("UserId");

        /// <summary>用户名</summary>
        public static readonly Field UserName = FindByName("UserName");

        /// <summary>客户端IP</summary>
        public static readonly Field ClientIp = FindByName("ClientIp");

        /// <summary>用户代理</summary>
        public static readonly Field UserAgent = FindByName("UserAgent");

        /// <summary>来源地址</summary>
        public static readonly Field Referer = FindByName("Referer");

        /// <summary>是否成功</summary>
        public static readonly Field Success = FindByName("Success");

        /// <summary>失败原因</summary>
        public static readonly Field FailReason = FindByName("FailReason");

        /// <summary>响应状态码</summary>
        public static readonly Field ResponseCode = FindByName("ResponseCode");

        /// <summary>传输字节数</summary>
        public static readonly Field BytesTransferred = FindByName("BytesTransferred");

        /// <summary>下载耗时（毫秒）</summary>
        public static readonly Field DownloadTime = FindByName("DownloadTime");

        /// <summary>下载速度（字节/秒）</summary>
        public static readonly Field Speed = FindByName("Speed");

        /// <summary>下载时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>追踪ID</summary>
        public static readonly Field TraceId = FindByName("TraceId");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得下载日志字段名称的快捷方式</summary>
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

        /// <summary>访问方式（Direct/Token/Share）</summary>
        public const String AccessType = "AccessType";

        /// <summary>分享码（如果是分享下载）</summary>
        public const String ShareCode = "ShareCode";

        /// <summary>用户ID（登录下载）</summary>
        public const String UserId = "UserId";

        /// <summary>用户名</summary>
        public const String UserName = "UserName";

        /// <summary>客户端IP</summary>
        public const String ClientIp = "ClientIp";

        /// <summary>用户代理</summary>
        public const String UserAgent = "UserAgent";

        /// <summary>来源地址</summary>
        public const String Referer = "Referer";

        /// <summary>是否成功</summary>
        public const String Success = "Success";

        /// <summary>失败原因</summary>
        public const String FailReason = "FailReason";

        /// <summary>响应状态码</summary>
        public const String ResponseCode = "ResponseCode";

        /// <summary>传输字节数</summary>
        public const String BytesTransferred = "BytesTransferred";

        /// <summary>下载耗时（毫秒）</summary>
        public const String DownloadTime = "DownloadTime";

        /// <summary>下载速度（字节/秒）</summary>
        public const String Speed = "Speed";

        /// <summary>下载时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>追踪ID</summary>
        public const String TraceId = "TraceId";
    }
    #endregion
}
