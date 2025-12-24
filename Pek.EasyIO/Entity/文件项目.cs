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

/// <summary>文件项目</summary>
[Serializable]
[DataObject]
[Description("文件项目")]
[BindIndex("IU_FileProject_Code", true, "Code")]
[BindIndex("IX_FileProject_Enable", false, "Enable")]
[BindTable("FileProject", Description = "文件项目", ConnName = "EasyFile", DbType = DatabaseType.None)]
public partial class FileProject : IFileProject, IEntity<IFileProject>
{
    #region 属性
    private Int64 _Id;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("Id", "编号", "")]
    public Int64 Id { get => _Id; set { if (OnPropertyChanging("Id", value)) { _Id = value; OnPropertyChanged("Id"); } } }

    private String _Code = null!;
    /// <summary>项目编码（唯一）</summary>
    [DisplayName("项目编码（唯一）")]
    [Description("项目编码（唯一）")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn("Code", "项目编码（唯一）", "")]
    public String Code { get => _Code; set { if (OnPropertyChanging("Code", value)) { _Code = value; OnPropertyChanged("Code"); } } }

    private String _Name = null!;
    /// <summary>项目名称</summary>
    [DisplayName("项目名称")]
    [Description("项目名称")]
    [DataObjectField(false, false, false, 100)]
    [BindColumn("Name", "项目名称", "", Master = true)]
    public String Name { get => _Name; set { if (OnPropertyChanging("Name", value)) { _Name = value; OnPropertyChanged("Name"); } } }

    private String? _Description;
    /// <summary>项目描述</summary>
    [DisplayName("项目描述")]
    [Description("项目描述")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("Description", "项目描述", "")]
    public String? Description { get => _Description; set { if (OnPropertyChanging("Description", value)) { _Description = value; OnPropertyChanged("Description"); } } }

    private String? _StoragePath;
    /// <summary>存储根目录（相对或绝对路径）</summary>
    [DisplayName("存储根目录（相对或绝对路径）")]
    [Description("存储根目录（相对或绝对路径）")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("StoragePath", "存储根目录（相对或绝对路径）", "")]
    public String? StoragePath { get => _StoragePath; set { if (OnPropertyChanging("StoragePath", value)) { _StoragePath = value; OnPropertyChanged("StoragePath"); } } }

    private String? _ApiSecret;
    /// <summary>API密钥</summary>
    [DisplayName("API密钥")]
    [Description("API密钥")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("ApiSecret", "API密钥", "")]
    public String? ApiSecret { get => _ApiSecret; set { if (OnPropertyChanging("ApiSecret", value)) { _ApiSecret = value; OnPropertyChanged("ApiSecret"); } } }

    private Int64 _MaxStorageSize;
    /// <summary>最大存储空间（字节，0=不限制）</summary>
    [DisplayName("最大存储空间（字节")]
    [Description("最大存储空间（字节，0=不限制）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MaxStorageSize", "最大存储空间（字节，0=不限制）", "")]
    public Int64 MaxStorageSize { get => _MaxStorageSize; set { if (OnPropertyChanging("MaxStorageSize", value)) { _MaxStorageSize = value; OnPropertyChanged("MaxStorageSize"); } } }

    private Int64 _UsedStorageSize;
    /// <summary>已用存储空间</summary>
    [DisplayName("已用存储空间")]
    [Description("已用存储空间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UsedStorageSize", "已用存储空间", "")]
    public Int64 UsedStorageSize { get => _UsedStorageSize; set { if (OnPropertyChanging("UsedStorageSize", value)) { _UsedStorageSize = value; OnPropertyChanged("UsedStorageSize"); } } }

    private Int64 _MaxFileSize;
    /// <summary>单文件最大大小（字节）</summary>
    [DisplayName("单文件最大大小（字节）")]
    [Description("单文件最大大小（字节）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MaxFileSize", "单文件最大大小（字节）", "")]
    public Int64 MaxFileSize { get => _MaxFileSize; set { if (OnPropertyChanging("MaxFileSize", value)) { _MaxFileSize = value; OnPropertyChanged("MaxFileSize"); } } }

    private String? _AllowedExtensions;
    /// <summary>允许的扩展名（.jpg,.png）</summary>
    [DisplayName("允许的扩展名（")]
    [Description("允许的扩展名（.jpg,.png）")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("AllowedExtensions", "允许的扩展名（.jpg,.png）", "")]
    public String? AllowedExtensions { get => _AllowedExtensions; set { if (OnPropertyChanging("AllowedExtensions", value)) { _AllowedExtensions = value; OnPropertyChanged("AllowedExtensions"); } } }

    private String? _ForbiddenExtensions;
    /// <summary>禁止的扩展名</summary>
    [DisplayName("禁止的扩展名")]
    [Description("禁止的扩展名")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("ForbiddenExtensions", "禁止的扩展名", "")]
    public String? ForbiddenExtensions { get => _ForbiddenExtensions; set { if (OnPropertyChanging("ForbiddenExtensions", value)) { _ForbiddenExtensions = value; OnPropertyChanged("ForbiddenExtensions"); } } }

    private Int32 _DefaultAccessLevel;
    /// <summary>默认访问级别。1公开 2私有 3内部</summary>
    [DisplayName("默认访问级别")]
    [Description("默认访问级别。1公开 2私有 3内部")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("DefaultAccessLevel", "默认访问级别。1公开 2私有 3内部", "")]
    public Int32 DefaultAccessLevel { get => _DefaultAccessLevel; set { if (OnPropertyChanging("DefaultAccessLevel", value)) { _DefaultAccessLevel = value; OnPropertyChanged("DefaultAccessLevel"); } } }

    private Int32 _RateLimitPerIp;
    /// <summary>每IP每分钟限制次数</summary>
    [DisplayName("每IP每分钟限制次数")]
    [Description("每IP每分钟限制次数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("RateLimitPerIp", "每IP每分钟限制次数", "")]
    public Int32 RateLimitPerIp { get => _RateLimitPerIp; set { if (OnPropertyChanging("RateLimitPerIp", value)) { _RateLimitPerIp = value; OnPropertyChanged("RateLimitPerIp"); } } }

    private Int32 _RateLimitPerFile;
    /// <summary>每文件每分钟限制次数</summary>
    [DisplayName("每文件每分钟限制次数")]
    [Description("每文件每分钟限制次数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("RateLimitPerFile", "每文件每分钟限制次数", "")]
    public Int32 RateLimitPerFile { get => _RateLimitPerFile; set { if (OnPropertyChanging("RateLimitPerFile", value)) { _RateLimitPerFile = value; OnPropertyChanged("RateLimitPerFile"); } } }

    private Boolean _Enable;
    /// <summary>是否启用</summary>
    [DisplayName("是否启用")]
    [Description("是否启用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Enable", "是否启用", "")]
    public Boolean Enable { get => _Enable; set { if (OnPropertyChanging("Enable", value)) { _Enable = value; OnPropertyChanged("Enable"); } } }

    private DateTime _CreateTime;
    /// <summary>创建时间</summary>
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateTime", "创建时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [DisplayName("更新时间")]
    [Description("更新时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("UpdateTime", "更新时间", "")]
    public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging("UpdateTime", value)) { _UpdateTime = value; OnPropertyChanged("UpdateTime"); } } }

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
    public void Copy(IFileProject model)
    {
        Id = model.Id;
        Code = model.Code;
        Name = model.Name;
        Description = model.Description;
        StoragePath = model.StoragePath;
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

    #region 获取/设置 字段值
    /// <summary>获取/设置 字段值</summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    public override Object? this[String name]
    {
        get => name switch
        {
            "Id" => _Id,
            "Code" => _Code,
            "Name" => _Name,
            "Description" => _Description,
            "StoragePath" => _StoragePath,
            "ApiSecret" => _ApiSecret,
            "MaxStorageSize" => _MaxStorageSize,
            "UsedStorageSize" => _UsedStorageSize,
            "MaxFileSize" => _MaxFileSize,
            "AllowedExtensions" => _AllowedExtensions,
            "ForbiddenExtensions" => _ForbiddenExtensions,
            "DefaultAccessLevel" => _DefaultAccessLevel,
            "RateLimitPerIp" => _RateLimitPerIp,
            "RateLimitPerFile" => _RateLimitPerFile,
            "Enable" => _Enable,
            "CreateTime" => _CreateTime,
            "UpdateTime" => _UpdateTime,
            "Remark" => _Remark,
            _ => base[name]
        };
        set
        {
            switch (name)
            {
                case "Id": _Id = value.ToLong(); break;
                case "Code": _Code = Convert.ToString(value); break;
                case "Name": _Name = Convert.ToString(value); break;
                case "Description": _Description = Convert.ToString(value); break;
                case "StoragePath": _StoragePath = Convert.ToString(value); break;
                case "ApiSecret": _ApiSecret = Convert.ToString(value); break;
                case "MaxStorageSize": _MaxStorageSize = value.ToLong(); break;
                case "UsedStorageSize": _UsedStorageSize = value.ToLong(); break;
                case "MaxFileSize": _MaxFileSize = value.ToLong(); break;
                case "AllowedExtensions": _AllowedExtensions = Convert.ToString(value); break;
                case "ForbiddenExtensions": _ForbiddenExtensions = Convert.ToString(value); break;
                case "DefaultAccessLevel": _DefaultAccessLevel = value.ToInt(); break;
                case "RateLimitPerIp": _RateLimitPerIp = value.ToInt(); break;
                case "RateLimitPerFile": _RateLimitPerFile = value.ToInt(); break;
                case "Enable": _Enable = value.ToBoolean(); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
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
    public static FileProject? FindById(Int64 id)
    {
        if (id < 0) return null;

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Id == id);

        // 单对象缓存
        return Meta.SingleCache[id];

        //return Find(_.Id == id);
    }

    /// <summary>根据项目编码（唯一）查找</summary>
    /// <param name="code">项目编码（唯一）</param>
    /// <returns>实体对象</returns>
    public static FileProject? FindByCode(String code)
    {
        if (code.IsNullOrEmpty()) return null;

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Code.EqualIgnoreCase(code));

        return Find(_.Code == code);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="code">项目编码（唯一）</param>
    /// <param name="enable">是否启用</param>
    /// <param name="start">更新时间开始</param>
    /// <param name="end">更新时间结束</param>
    /// <param name="key">关键字</param>
    /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
    /// <returns>实体列表</returns>
    public static IList<FileProject> Search(String code, Boolean? enable, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();

        if (!code.IsNullOrEmpty()) exp &= _.Code == code;
        if (enable != null) exp &= _.Enable == enable;
        exp &= _.UpdateTime.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= SearchWhereByKeys(key);

        return FindAll(exp, page);
    }
    #endregion

    #region 字段名
    /// <summary>取得文件项目字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field Id = FindByName("Id");

        /// <summary>项目编码（唯一）</summary>
        public static readonly Field Code = FindByName("Code");

        /// <summary>项目名称</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>项目描述</summary>
        public static readonly Field Description = FindByName("Description");

        /// <summary>存储根目录（相对或绝对路径）</summary>
        public static readonly Field StoragePath = FindByName("StoragePath");

        /// <summary>API密钥</summary>
        public static readonly Field ApiSecret = FindByName("ApiSecret");

        /// <summary>最大存储空间（字节，0=不限制）</summary>
        public static readonly Field MaxStorageSize = FindByName("MaxStorageSize");

        /// <summary>已用存储空间</summary>
        public static readonly Field UsedStorageSize = FindByName("UsedStorageSize");

        /// <summary>单文件最大大小（字节）</summary>
        public static readonly Field MaxFileSize = FindByName("MaxFileSize");

        /// <summary>允许的扩展名（.jpg,.png）</summary>
        public static readonly Field AllowedExtensions = FindByName("AllowedExtensions");

        /// <summary>禁止的扩展名</summary>
        public static readonly Field ForbiddenExtensions = FindByName("ForbiddenExtensions");

        /// <summary>默认访问级别。1公开 2私有 3内部</summary>
        public static readonly Field DefaultAccessLevel = FindByName("DefaultAccessLevel");

        /// <summary>每IP每分钟限制次数</summary>
        public static readonly Field RateLimitPerIp = FindByName("RateLimitPerIp");

        /// <summary>每文件每分钟限制次数</summary>
        public static readonly Field RateLimitPerFile = FindByName("RateLimitPerFile");

        /// <summary>是否启用</summary>
        public static readonly Field Enable = FindByName("Enable");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        /// <summary>备注</summary>
        public static readonly Field Remark = FindByName("Remark");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得文件项目字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String Id = "Id";

        /// <summary>项目编码（唯一）</summary>
        public const String Code = "Code";

        /// <summary>项目名称</summary>
        public const String Name = "Name";

        /// <summary>项目描述</summary>
        public const String Description = "Description";

        /// <summary>存储根目录（相对或绝对路径）</summary>
        public const String StoragePath = "StoragePath";

        /// <summary>API密钥</summary>
        public const String ApiSecret = "ApiSecret";

        /// <summary>最大存储空间（字节，0=不限制）</summary>
        public const String MaxStorageSize = "MaxStorageSize";

        /// <summary>已用存储空间</summary>
        public const String UsedStorageSize = "UsedStorageSize";

        /// <summary>单文件最大大小（字节）</summary>
        public const String MaxFileSize = "MaxFileSize";

        /// <summary>允许的扩展名（.jpg,.png）</summary>
        public const String AllowedExtensions = "AllowedExtensions";

        /// <summary>禁止的扩展名</summary>
        public const String ForbiddenExtensions = "ForbiddenExtensions";

        /// <summary>默认访问级别。1公开 2私有 3内部</summary>
        public const String DefaultAccessLevel = "DefaultAccessLevel";

        /// <summary>每IP每分钟限制次数</summary>
        public const String RateLimitPerIp = "RateLimitPerIp";

        /// <summary>每文件每分钟限制次数</summary>
        public const String RateLimitPerFile = "RateLimitPerFile";

        /// <summary>是否启用</summary>
        public const String Enable = "Enable";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";

        /// <summary>备注</summary>
        public const String Remark = "Remark";
    }
    #endregion
}
