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

/// <summary>文件操作日志</summary>
[Serializable]
[DataObject]
[Description("文件操作日志")]
[BindIndex("IX_FileOperationLog_FileId_CreateTime", false, "FileId,CreateTime")]
[BindIndex("IX_FileOperationLog_OperationType_CreateTime", false, "OperationType,CreateTime")]
[BindIndex("IX_FileOperationLog_ProjectId_CreateTime", false, "ProjectId,CreateTime")]
[BindIndex("IX_FileOperationLog_UserId_CreateTime", false, "UserId,CreateTime")]
[BindIndex("IX_FileOperationLog_CreateTime", false, "CreateTime")]
[BindTable("FileOperationLog", Description = "文件操作日志", ConnName = "EasyFile", DbType = DatabaseType.None)]
public partial class FileOperationLog : IFileOperationLog, IEntity<IFileOperationLog>
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

    private String? _ProjectName;
    /// <summary>项目名称</summary>
    [DisplayName("项目名称")]
    [Description("项目名称")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("ProjectName", "项目名称", "")]
    public String? ProjectName { get => _ProjectName; set { if (OnPropertyChanging("ProjectName", value)) { _ProjectName = value; OnPropertyChanged("ProjectName"); } } }

    private String _OperationType = null!;
    /// <summary>操作类型（Upload/Delete/Update/Move/Copy）</summary>
    [DisplayName("操作类型（Upload_Delete_Update_Move_Copy）")]
    [Description("操作类型（Upload/Delete/Update/Move/Copy）")]
    [DataObjectField(false, false, false, 20)]
    [BindColumn("OperationType", "操作类型（Upload/Delete/Update/Move/Copy）", "")]
    public String OperationType { get => _OperationType; set { if (OnPropertyChanging("OperationType", value)) { _OperationType = value; OnPropertyChanged("OperationType"); } } }

    private String? _OperationDetail;
    /// <summary>操作详情（JSON）</summary>
    [DisplayName("操作详情（JSON）")]
    [Description("操作详情（JSON）")]
    [DataObjectField(false, false, true, 1000)]
    [BindColumn("OperationDetail", "操作详情（JSON）", "")]
    public String? OperationDetail { get => _OperationDetail; set { if (OnPropertyChanging("OperationDetail", value)) { _OperationDetail = value; OnPropertyChanged("OperationDetail"); } } }

    private String? _FilePath;
    /// <summary>文件路径</summary>
    [DisplayName("文件路径")]
    [Description("文件路径")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("FilePath", "文件路径", "")]
    public String? FilePath { get => _FilePath; set { if (OnPropertyChanging("FilePath", value)) { _FilePath = value; OnPropertyChanged("FilePath"); } } }

    private Int64 _FileSize;
    /// <summary>文件大小（字节）</summary>
    [DisplayName("文件大小（字节）")]
    [Description("文件大小（字节）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("FileSize", "文件大小（字节）", "")]
    public Int64 FileSize { get => _FileSize; set { if (OnPropertyChanging("FileSize", value)) { _FileSize = value; OnPropertyChanged("FileSize"); } } }

    private String? _FileHash;
    /// <summary>文件哈希</summary>
    [DisplayName("文件哈希")]
    [Description("文件哈希")]
    [DataObjectField(false, false, true, 64)]
    [BindColumn("FileHash", "文件哈希", "")]
    public String? FileHash { get => _FileHash; set { if (OnPropertyChanging("FileHash", value)) { _FileHash = value; OnPropertyChanged("FileHash"); } } }

    private Int64 _UserId;
    /// <summary>操作用户ID</summary>
    [DisplayName("操作用户ID")]
    [Description("操作用户ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UserId", "操作用户ID", "")]
    public Int64 UserId { get => _UserId; set { if (OnPropertyChanging("UserId", value)) { _UserId = value; OnPropertyChanged("UserId"); } } }

    private String? _UserName;
    /// <summary>操作用户名</summary>
    [DisplayName("操作用户名")]
    [Description("操作用户名")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("UserName", "操作用户名", "")]
    public String? UserName { get => _UserName; set { if (OnPropertyChanging("UserName", value)) { _UserName = value; OnPropertyChanged("UserName"); } } }

    private String? _ExternalUserId;
    /// <summary>外部用户关联ID（由请求方传递）</summary>
    [DisplayName("外部用户关联ID（由请求方传递）")]
    [Description("外部用户关联ID（由请求方传递）")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("ExternalUserId", "外部用户关联ID（由请求方传递）", "")]
    public String? ExternalUserId { get => _ExternalUserId; set { if (OnPropertyChanging("ExternalUserId", value)) { _ExternalUserId = value; OnPropertyChanged("ExternalUserId"); } } }

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

    private Boolean _Success;
    /// <summary>是否成功</summary>
    [DisplayName("是否成功")]
    [Description("是否成功")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Success", "是否成功", "")]
    public Boolean Success { get => _Success; set { if (OnPropertyChanging("Success", value)) { _Success = value; OnPropertyChanged("Success"); } } }

    private String? _ErrorMessage;
    /// <summary>错误信息</summary>
    [DisplayName("错误信息")]
    [Description("错误信息")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("ErrorMessage", "错误信息", "")]
    public String? ErrorMessage { get => _ErrorMessage; set { if (OnPropertyChanging("ErrorMessage", value)) { _ErrorMessage = value; OnPropertyChanged("ErrorMessage"); } } }

    private Int32 _Duration;
    /// <summary>操作耗时（毫秒）</summary>
    [DisplayName("操作耗时（毫秒）")]
    [Description("操作耗时（毫秒）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Duration", "操作耗时（毫秒）", "")]
    public Int32 Duration { get => _Duration; set { if (OnPropertyChanging("Duration", value)) { _Duration = value; OnPropertyChanged("Duration"); } } }

    private DateTime _CreateTime;
    /// <summary>操作时间</summary>
    [DisplayName("操作时间")]
    [Description("操作时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateTime", "操作时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private String? _TraceId;
    /// <summary>追踪ID</summary>
    [DisplayName("追踪ID")]
    [Description("追踪ID")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("TraceId", "追踪ID", "")]
    public String? TraceId { get => _TraceId; set { if (OnPropertyChanging("TraceId", value)) { _TraceId = value; OnPropertyChanged("TraceId"); } } }

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
        ExternalUserId = model.ExternalUserId;
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
            "ProjectName" => _ProjectName,
            "OperationType" => _OperationType,
            "OperationDetail" => _OperationDetail,
            "FilePath" => _FilePath,
            "FileSize" => _FileSize,
            "FileHash" => _FileHash,
            "UserId" => _UserId,
            "UserName" => _UserName,
            "ExternalUserId" => _ExternalUserId,
            "ClientIp" => _ClientIp,
            "UserAgent" => _UserAgent,
            "Success" => _Success,
            "ErrorMessage" => _ErrorMessage,
            "Duration" => _Duration,
            "CreateTime" => _CreateTime,
            "TraceId" => _TraceId,
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
                case "ProjectName": _ProjectName = Convert.ToString(value); break;
                case "OperationType": _OperationType = Convert.ToString(value); break;
                case "OperationDetail": _OperationDetail = Convert.ToString(value); break;
                case "FilePath": _FilePath = Convert.ToString(value); break;
                case "FileSize": _FileSize = value.ToLong(); break;
                case "FileHash": _FileHash = Convert.ToString(value); break;
                case "UserId": _UserId = value.ToLong(); break;
                case "UserName": _UserName = Convert.ToString(value); break;
                case "ExternalUserId": _ExternalUserId = Convert.ToString(value); break;
                case "ClientIp": _ClientIp = Convert.ToString(value); break;
                case "UserAgent": _UserAgent = Convert.ToString(value); break;
                case "Success": _Success = value.ToBoolean(); break;
                case "ErrorMessage": _ErrorMessage = Convert.ToString(value); break;
                case "Duration": _Duration = value.ToInt(); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "TraceId": _TraceId = Convert.ToString(value); break;
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
    public static FileOperationLog? FindById(Int64 id)
    {
        if (id < 0) return null;

        // 实体缓存
        if (Meta.Session.Count < MaxCacheCount) return Meta.Cache.Find(e => e.Id == id);

        // 单对象缓存
        return Meta.SingleCache[id];

        //return Find(_.Id == id);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="projectId">项目ID</param>
    /// <param name="operationType">操作类型（Upload/Delete/Update/Move/Copy）</param>
    /// <param name="userId">操作用户ID</param>
    /// <param name="success">是否成功</param>
    /// <param name="start">操作时间开始</param>
    /// <param name="end">操作时间结束</param>
    /// <param name="key">关键字</param>
    /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
    /// <returns>实体列表</returns>
    public static IList<FileOperationLog> Search(Int64 fileId, Int64 projectId, String operationType, Int64 userId, Boolean? success, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();

        if (fileId >= 0) exp &= _.FileId == fileId;
        if (projectId >= 0) exp &= _.ProjectId == projectId;
        if (!operationType.IsNullOrEmpty()) exp &= _.OperationType == operationType;
        if (userId >= 0) exp &= _.UserId == userId;
        if (success != null) exp &= _.Success == success;
        exp &= _.CreateTime.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= SearchWhereByKeys(key);

        return FindAll(exp, page);
    }
    #endregion

    #region 字段名
    /// <summary>取得文件操作日志字段信息的快捷方式</summary>
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

        /// <summary>项目名称</summary>
        public static readonly Field ProjectName = FindByName("ProjectName");

        /// <summary>操作类型（Upload/Delete/Update/Move/Copy）</summary>
        public static readonly Field OperationType = FindByName("OperationType");

        /// <summary>操作详情（JSON）</summary>
        public static readonly Field OperationDetail = FindByName("OperationDetail");

        /// <summary>文件路径</summary>
        public static readonly Field FilePath = FindByName("FilePath");

        /// <summary>文件大小（字节）</summary>
        public static readonly Field FileSize = FindByName("FileSize");

        /// <summary>文件哈希</summary>
        public static readonly Field FileHash = FindByName("FileHash");

        /// <summary>操作用户ID</summary>
        public static readonly Field UserId = FindByName("UserId");

        /// <summary>操作用户名</summary>
        public static readonly Field UserName = FindByName("UserName");

        /// <summary>外部用户关联ID（由请求方传递）</summary>
        public static readonly Field ExternalUserId = FindByName("ExternalUserId");

        /// <summary>客户端IP</summary>
        public static readonly Field ClientIp = FindByName("ClientIp");

        /// <summary>用户代理</summary>
        public static readonly Field UserAgent = FindByName("UserAgent");

        /// <summary>是否成功</summary>
        public static readonly Field Success = FindByName("Success");

        /// <summary>错误信息</summary>
        public static readonly Field ErrorMessage = FindByName("ErrorMessage");

        /// <summary>操作耗时（毫秒）</summary>
        public static readonly Field Duration = FindByName("Duration");

        /// <summary>操作时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>追踪ID</summary>
        public static readonly Field TraceId = FindByName("TraceId");

        /// <summary>备注</summary>
        public static readonly Field Remark = FindByName("Remark");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得文件操作日志字段名称的快捷方式</summary>
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

        /// <summary>项目名称</summary>
        public const String ProjectName = "ProjectName";

        /// <summary>操作类型（Upload/Delete/Update/Move/Copy）</summary>
        public const String OperationType = "OperationType";

        /// <summary>操作详情（JSON）</summary>
        public const String OperationDetail = "OperationDetail";

        /// <summary>文件路径</summary>
        public const String FilePath = "FilePath";

        /// <summary>文件大小（字节）</summary>
        public const String FileSize = "FileSize";

        /// <summary>文件哈希</summary>
        public const String FileHash = "FileHash";

        /// <summary>操作用户ID</summary>
        public const String UserId = "UserId";

        /// <summary>操作用户名</summary>
        public const String UserName = "UserName";

        /// <summary>外部用户关联ID（由请求方传递）</summary>
        public const String ExternalUserId = "ExternalUserId";

        /// <summary>客户端IP</summary>
        public const String ClientIp = "ClientIp";

        /// <summary>用户代理</summary>
        public const String UserAgent = "UserAgent";

        /// <summary>是否成功</summary>
        public const String Success = "Success";

        /// <summary>错误信息</summary>
        public const String ErrorMessage = "ErrorMessage";

        /// <summary>操作耗时（毫秒）</summary>
        public const String Duration = "Duration";

        /// <summary>操作时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>追踪ID</summary>
        public const String TraceId = "TraceId";

        /// <summary>备注</summary>
        public const String Remark = "Remark";
    }
    #endregion
}
