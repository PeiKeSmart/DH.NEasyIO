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

/// <summary>文件条目</summary>
[Serializable]
[DataObject]
[Description("文件条目")]
[BindIndex("IX_FileEntry_Hash", false, "Hash")]
[BindIndex("IX_FileEntry_ProjectId", false, "ProjectId")]
[BindIndex("IX_FileEntry_AccessLevel", false, "AccessLevel")]
[BindIndex("IX_FileEntry_CreateTime", false, "CreateTime")]
[BindIndex("IX_FileEntry_BusinessType_BusinessId", false, "BusinessType,BusinessId")]
[BindIndex("IX_FileEntry_OwnerId", false, "OwnerId")]
[BindTable("FileEntry", Description = "文件条目", ConnName = "EasyFile", DbType = DatabaseType.None)]
public partial class FileEntry : IFileEntry, IEntity<IFileEntry>
{
    #region 属性
    private Int64 _Id;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("Id", "编号", "")]
    public Int64 Id { get => _Id; set { if (OnPropertyChanging("Id", value)) { _Id = value; OnPropertyChanged("Id"); } } }

    private String _Name = null!;
    /// <summary>文件名</summary>
    [DisplayName("文件名")]
    [Description("文件名")]
    [DataObjectField(false, false, false, 200)]
    [BindColumn("Name", "文件名", "", Master = true)]
    public String Name { get => _Name; set { if (OnPropertyChanging("Name", value)) { _Name = value; OnPropertyChanged("Name"); } } }

    private String? _OriginalName;
    /// <summary>原始文件名</summary>
    [DisplayName("原始文件名")]
    [Description("原始文件名")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("OriginalName", "原始文件名", "")]
    public String? OriginalName { get => _OriginalName; set { if (OnPropertyChanging("OriginalName", value)) { _OriginalName = value; OnPropertyChanged("OriginalName"); } } }

    private String? _Extension;
    /// <summary>扩展名（.jpg）</summary>
    [DisplayName("扩展名（")]
    [Description("扩展名（.jpg）")]
    [DataObjectField(false, false, true, 20)]
    [BindColumn("Extension", "扩展名（.jpg）", "")]
    public String? Extension { get => _Extension; set { if (OnPropertyChanging("Extension", value)) { _Extension = value; OnPropertyChanged("Extension"); } } }

    private String? _ContentType;
    /// <summary>MIME类型（image/jpeg）</summary>
    [DisplayName("MIME类型（image_jpeg）")]
    [Description("MIME类型（image/jpeg）")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("ContentType", "MIME类型（image/jpeg）", "")]
    public String? ContentType { get => _ContentType; set { if (OnPropertyChanging("ContentType", value)) { _ContentType = value; OnPropertyChanged("ContentType"); } } }

    private Int64 _Size;
    /// <summary>文件大小（字节）</summary>
    [DisplayName("文件大小（字节）")]
    [Description("文件大小（字节）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Size", "文件大小（字节）", "")]
    public Int64 Size { get => _Size; set { if (OnPropertyChanging("Size", value)) { _Size = value; OnPropertyChanged("Size"); } } }

    private String? _Hash;
    /// <summary>文件哈希（MD5/SHA256）</summary>
    [DisplayName("文件哈希（MD5_SHA256）")]
    [Description("文件哈希（MD5/SHA256）")]
    [DataObjectField(false, false, true, 64)]
    [BindColumn("Hash", "文件哈希（MD5/SHA256）", "")]
    public String? Hash { get => _Hash; set { if (OnPropertyChanging("Hash", value)) { _Hash = value; OnPropertyChanged("Hash"); } } }

    private String? _StorageType;
    /// <summary>存储类型（Local/OSS/S3）</summary>
    [DisplayName("存储类型（Local_OSS_S3）")]
    [Description("存储类型（Local/OSS/S3）")]
    [DataObjectField(false, false, true, 20)]
    [BindColumn("StorageType", "存储类型（Local/OSS/S3）", "")]
    public String? StorageType { get => _StorageType; set { if (OnPropertyChanging("StorageType", value)) { _StorageType = value; OnPropertyChanged("StorageType"); } } }

    private String _RelativePath = null!;
    /// <summary>相对于项目存储目录的路径</summary>
    [DisplayName("相对于项目存储目录的路径")]
    [Description("相对于项目存储目录的路径")]
    [DataObjectField(false, false, false, 500)]
    [BindColumn("RelativePath", "相对于项目存储目录的路径", "")]
    public String RelativePath { get => _RelativePath; set { if (OnPropertyChanging("RelativePath", value)) { _RelativePath = value; OnPropertyChanged("RelativePath"); } } }

    private String? _BucketName;
    /// <summary>存储桶名称</summary>
    [DisplayName("存储桶名称")]
    [Description("存储桶名称")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("BucketName", "存储桶名称", "")]
    public String? BucketName { get => _BucketName; set { if (OnPropertyChanging("BucketName", value)) { _BucketName = value; OnPropertyChanged("BucketName"); } } }

    private Int32 _AccessLevel;
    /// <summary>访问级别（1=Public,2=Private,3=Internal）</summary>
    [DisplayName("访问级别（1=Public")]
    [Description("访问级别（1=Public,2=Private,3=Internal）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("AccessLevel", "访问级别（1=Public,2=Private,3=Internal）", "")]
    public Int32 AccessLevel { get => _AccessLevel; set { if (OnPropertyChanging("AccessLevel", value)) { _AccessLevel = value; OnPropertyChanged("AccessLevel"); } } }

    private Int32 _DownloadCount;
    /// <summary>已下载次数</summary>
    [DisplayName("已下载次数")]
    [Description("已下载次数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("DownloadCount", "已下载次数", "")]
    public Int32 DownloadCount { get => _DownloadCount; set { if (OnPropertyChanging("DownloadCount", value)) { _DownloadCount = value; OnPropertyChanged("DownloadCount"); } } }

    private Int64 _ProjectId;
    /// <summary>所属项目ID</summary>
    [DisplayName("所属项目ID")]
    [Description("所属项目ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ProjectId", "所属项目ID", "")]
    public Int64 ProjectId { get => _ProjectId; set { if (OnPropertyChanging("ProjectId", value)) { _ProjectId = value; OnPropertyChanged("ProjectId"); } } }

    private String? _ProjectName;
    /// <summary>项目名称（冗余）</summary>
    [DisplayName("项目名称（冗余）")]
    [Description("项目名称（冗余）")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("ProjectName", "项目名称（冗余）", "")]
    public String? ProjectName { get => _ProjectName; set { if (OnPropertyChanging("ProjectName", value)) { _ProjectName = value; OnPropertyChanged("ProjectName"); } } }

    private String? _Category;
    /// <summary>分类（Document/Image/Video等）</summary>
    [DisplayName("分类（Document_Image_Video等）")]
    [Description("分类（Document/Image/Video等）")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Category", "分类（Document/Image/Video等）", "")]
    public String? Category { get => _Category; set { if (OnPropertyChanging("Category", value)) { _Category = value; OnPropertyChanged("Category"); } } }

    private String? _Tags;
    /// <summary>标签（JSON数组或逗号分隔）</summary>
    [DisplayName("标签（JSON数组或逗号分隔）")]
    [Description("标签（JSON数组或逗号分隔）")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("Tags", "标签（JSON数组或逗号分隔）", "")]
    public String? Tags { get => _Tags; set { if (OnPropertyChanging("Tags", value)) { _Tags = value; OnPropertyChanged("Tags"); } } }

    private String? _BusinessType;
    /// <summary>业务类型</summary>
    [DisplayName("业务类型")]
    [Description("业务类型")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("BusinessType", "业务类型", "")]
    public String? BusinessType { get => _BusinessType; set { if (OnPropertyChanging("BusinessType", value)) { _BusinessType = value; OnPropertyChanged("BusinessType"); } } }

    private String? _BusinessId;
    /// <summary>业务ID</summary>
    [DisplayName("业务ID")]
    [Description("业务ID")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("BusinessId", "业务ID", "")]
    public String? BusinessId { get => _BusinessId; set { if (OnPropertyChanging("BusinessId", value)) { _BusinessId = value; OnPropertyChanged("BusinessId"); } } }

    private Int64 _OwnerId;
    /// <summary>所有者用户ID</summary>
    [DisplayName("所有者用户ID")]
    [Description("所有者用户ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("OwnerId", "所有者用户ID", "")]
    public Int64 OwnerId { get => _OwnerId; set { if (OnPropertyChanging("OwnerId", value)) { _OwnerId = value; OnPropertyChanged("OwnerId"); } } }

    private String? _OwnerName;
    /// <summary>所有者名称</summary>
    [DisplayName("所有者名称")]
    [Description("所有者名称")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("OwnerName", "所有者名称", "")]
    public String? OwnerName { get => _OwnerName; set { if (OnPropertyChanging("OwnerName", value)) { _OwnerName = value; OnPropertyChanged("OwnerName"); } } }

    private Int32 _Width;
    /// <summary>图片宽度</summary>
    [DisplayName("图片宽度")]
    [Description("图片宽度")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Width", "图片宽度", "")]
    public Int32 Width { get => _Width; set { if (OnPropertyChanging("Width", value)) { _Width = value; OnPropertyChanged("Width"); } } }

    private Int32 _Height;
    /// <summary>图片高度</summary>
    [DisplayName("图片高度")]
    [Description("图片高度")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Height", "图片高度", "")]
    public Int32 Height { get => _Height; set { if (OnPropertyChanging("Height", value)) { _Height = value; OnPropertyChanged("Height"); } } }

    private Int32 _Duration;
    /// <summary>视频/音频时长（秒）</summary>
    [DisplayName("视频_音频时长（秒）")]
    [Description("视频/音频时长（秒）")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Duration", "视频/音频时长（秒）", "")]
    public Int32 Duration { get => _Duration; set { if (OnPropertyChanging("Duration", value)) { _Duration = value; OnPropertyChanged("Duration"); } } }

    private String? _Metadata;
    /// <summary>其他元数据（JSON）</summary>
    [DisplayName("其他元数据（JSON）")]
    [Description("其他元数据（JSON）")]
    [DataObjectField(false, false, true, 2000)]
    [BindColumn("Metadata", "其他元数据（JSON）", "")]
    public String? Metadata { get => _Metadata; set { if (OnPropertyChanging("Metadata", value)) { _Metadata = value; OnPropertyChanged("Metadata"); } } }

    private Boolean _IsScanned;
    /// <summary>是否已病毒扫描</summary>
    [DisplayName("是否已病毒扫描")]
    [Description("是否已病毒扫描")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("IsScanned", "是否已病毒扫描", "")]
    public Boolean IsScanned { get => _IsScanned; set { if (OnPropertyChanging("IsScanned", value)) { _IsScanned = value; OnPropertyChanged("IsScanned"); } } }

    private Boolean _IsEncrypted;
    /// <summary>是否加密存储</summary>
    [DisplayName("是否加密存储")]
    [Description("是否加密存储")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("IsEncrypted", "是否加密存储", "")]
    public Boolean IsEncrypted { get => _IsEncrypted; set { if (OnPropertyChanging("IsEncrypted", value)) { _IsEncrypted = value; OnPropertyChanged("IsEncrypted"); } } }

    private String? _EncryptionKey;
    /// <summary>加密密钥ID</summary>
    [DisplayName("加密密钥ID")]
    [Description("加密密钥ID")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("EncryptionKey", "加密密钥ID", "")]
    public String? EncryptionKey { get => _EncryptionKey; set { if (OnPropertyChanging("EncryptionKey", value)) { _EncryptionKey = value; OnPropertyChanged("EncryptionKey"); } } }

    private Int64 _CreateUserId;
    /// <summary>创建者ID</summary>
    [DisplayName("创建者ID")]
    [Description("创建者ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateUserId", "创建者ID", "")]
    public Int64 CreateUserId { get => _CreateUserId; set { if (OnPropertyChanging("CreateUserId", value)) { _CreateUserId = value; OnPropertyChanged("CreateUserId"); } } }

    private String? _CreateUser;
    /// <summary>创建者</summary>
    [DisplayName("创建者")]
    [Description("创建者")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("CreateUser", "创建者", "")]
    public String? CreateUser { get => _CreateUser; set { if (OnPropertyChanging("CreateUser", value)) { _CreateUser = value; OnPropertyChanged("CreateUser"); } } }

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

    private Int64 _UpdateUserId;
    /// <summary>更新者ID</summary>
    [DisplayName("更新者ID")]
    [Description("更新者ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UpdateUserId", "更新者ID", "")]
    public Int64 UpdateUserId { get => _UpdateUserId; set { if (OnPropertyChanging("UpdateUserId", value)) { _UpdateUserId = value; OnPropertyChanged("UpdateUserId"); } } }

    private String? _UpdateUser;
    /// <summary>更新者</summary>
    [DisplayName("更新者")]
    [Description("更新者")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("UpdateUser", "更新者", "")]
    public String? UpdateUser { get => _UpdateUser; set { if (OnPropertyChanging("UpdateUser", value)) { _UpdateUser = value; OnPropertyChanged("UpdateUser"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [DisplayName("更新时间")]
    [Description("更新时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("UpdateTime", "更新时间", "")]
    public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging("UpdateTime", value)) { _UpdateTime = value; OnPropertyChanged("UpdateTime"); } } }

    private String? _UpdateIP;
    /// <summary>更新IP</summary>
    [DisplayName("更新IP")]
    [Description("更新IP")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UpdateIP", "更新IP", "")]
    public String? UpdateIP { get => _UpdateIP; set { if (OnPropertyChanging("UpdateIP", value)) { _UpdateIP = value; OnPropertyChanged("UpdateIP"); } } }

    private String? _Remark;
    /// <summary>备注说明</summary>
    [DisplayName("备注说明")]
    [Description("备注说明")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("Remark", "备注说明", "")]
    public String? Remark { get => _Remark; set { if (OnPropertyChanging("Remark", value)) { _Remark = value; OnPropertyChanged("Remark"); } } }

    private Int32 _IpRateLimitPerMinute;
    /// <summary>IP限流-每分钟最大请求次数(0=不限制)</summary>
    [DisplayName("IP限流-每分钟最大请求次数(0=不限制)")]
    [Description("IP限流-每分钟最大请求次数(0=不限制)")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("IpRateLimitPerMinute", "IP限流-每分钟最大请求次数(0=不限制)", "")]
    public Int32 IpRateLimitPerMinute { get => _IpRateLimitPerMinute; set { if (OnPropertyChanging("IpRateLimitPerMinute", value)) { _IpRateLimitPerMinute = value; OnPropertyChanged("IpRateLimitPerMinute"); } } }
    #endregion

    #region 拷贝
    /// <summary>拷贝模型对象</summary>
    /// <param name="model">模型</param>
    public void Copy(IFileEntry model)
    {
        Id = model.Id;
        Name = model.Name;
        OriginalName = model.OriginalName;
        Extension = model.Extension;
        ContentType = model.ContentType;
        Size = model.Size;
        Hash = model.Hash;
        StorageType = model.StorageType;
        RelativePath = model.RelativePath;
        BucketName = model.BucketName;
        AccessLevel = model.AccessLevel;
        DownloadCount = model.DownloadCount;
        ProjectId = model.ProjectId;
        ProjectName = model.ProjectName;
        Category = model.Category;
        Tags = model.Tags;
        BusinessType = model.BusinessType;
        BusinessId = model.BusinessId;
        OwnerId = model.OwnerId;
        OwnerName = model.OwnerName;
        Width = model.Width;
        Height = model.Height;
        Duration = model.Duration;
        Metadata = model.Metadata;
        IsScanned = model.IsScanned;
        IsEncrypted = model.IsEncrypted;
        EncryptionKey = model.EncryptionKey;
        CreateUserId = model.CreateUserId;
        CreateUser = model.CreateUser;
        CreateTime = model.CreateTime;
        CreateIP = model.CreateIP;
        UpdateUserId = model.UpdateUserId;
        UpdateUser = model.UpdateUser;
        UpdateTime = model.UpdateTime;
        UpdateIP = model.UpdateIP;
        Remark = model.Remark;
        IpRateLimitPerMinute = model.IpRateLimitPerMinute;
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
            "Name" => _Name,
            "OriginalName" => _OriginalName,
            "Extension" => _Extension,
            "ContentType" => _ContentType,
            "Size" => _Size,
            "Hash" => _Hash,
            "StorageType" => _StorageType,
            "RelativePath" => _RelativePath,
            "BucketName" => _BucketName,
            "AccessLevel" => _AccessLevel,
            "DownloadCount" => _DownloadCount,
            "ProjectId" => _ProjectId,
            "ProjectName" => _ProjectName,
            "Category" => _Category,
            "Tags" => _Tags,
            "BusinessType" => _BusinessType,
            "BusinessId" => _BusinessId,
            "OwnerId" => _OwnerId,
            "OwnerName" => _OwnerName,
            "Width" => _Width,
            "Height" => _Height,
            "Duration" => _Duration,
            "Metadata" => _Metadata,
            "IsScanned" => _IsScanned,
            "IsEncrypted" => _IsEncrypted,
            "EncryptionKey" => _EncryptionKey,
            "CreateUserId" => _CreateUserId,
            "CreateUser" => _CreateUser,
            "CreateTime" => _CreateTime,
            "CreateIP" => _CreateIP,
            "UpdateUserId" => _UpdateUserId,
            "UpdateUser" => _UpdateUser,
            "UpdateTime" => _UpdateTime,
            "UpdateIP" => _UpdateIP,
            "Remark" => _Remark,
            "IpRateLimitPerMinute" => _IpRateLimitPerMinute,
            _ => base[name]
        };
        set
        {
            switch (name)
            {
                case "Id": _Id = value.ToLong(); break;
                case "Name": _Name = Convert.ToString(value); break;
                case "OriginalName": _OriginalName = Convert.ToString(value); break;
                case "Extension": _Extension = Convert.ToString(value); break;
                case "ContentType": _ContentType = Convert.ToString(value); break;
                case "Size": _Size = value.ToLong(); break;
                case "Hash": _Hash = Convert.ToString(value); break;
                case "StorageType": _StorageType = Convert.ToString(value); break;
                case "RelativePath": _RelativePath = Convert.ToString(value); break;
                case "BucketName": _BucketName = Convert.ToString(value); break;
                case "AccessLevel": _AccessLevel = value.ToInt(); break;
                case "DownloadCount": _DownloadCount = value.ToInt(); break;
                case "ProjectId": _ProjectId = value.ToLong(); break;
                case "ProjectName": _ProjectName = Convert.ToString(value); break;
                case "Category": _Category = Convert.ToString(value); break;
                case "Tags": _Tags = Convert.ToString(value); break;
                case "BusinessType": _BusinessType = Convert.ToString(value); break;
                case "BusinessId": _BusinessId = Convert.ToString(value); break;
                case "OwnerId": _OwnerId = value.ToLong(); break;
                case "OwnerName": _OwnerName = Convert.ToString(value); break;
                case "Width": _Width = value.ToInt(); break;
                case "Height": _Height = value.ToInt(); break;
                case "Duration": _Duration = value.ToInt(); break;
                case "Metadata": _Metadata = Convert.ToString(value); break;
                case "IsScanned": _IsScanned = value.ToBoolean(); break;
                case "IsEncrypted": _IsEncrypted = value.ToBoolean(); break;
                case "EncryptionKey": _EncryptionKey = Convert.ToString(value); break;
                case "CreateUserId": _CreateUserId = value.ToLong(); break;
                case "CreateUser": _CreateUser = Convert.ToString(value); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "CreateIP": _CreateIP = Convert.ToString(value); break;
                case "UpdateUserId": _UpdateUserId = value.ToLong(); break;
                case "UpdateUser": _UpdateUser = Convert.ToString(value); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                case "UpdateIP": _UpdateIP = Convert.ToString(value); break;
                case "Remark": _Remark = Convert.ToString(value); break;
                case "IpRateLimitPerMinute": _IpRateLimitPerMinute = value.ToInt(); break;
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
    public static FileEntry? FindById(Int64 id)
    {
        if (id < 0) return null;

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Id == id);

        // 单对象缓存
        return Meta.SingleCache[id];

        //return Find(_.Id == id);
    }

    /// <summary>根据文件哈希（MD5_SHA256）查找</summary>
    /// <param name="hash">文件哈希（MD5_SHA256）</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> FindAllByHash(String? hash)
    {
        if (hash == null) return [];

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.Hash.EqualIgnoreCase(hash));

        return FindAll(_.Hash == hash);
    }

    /// <summary>根据所属项目ID查找</summary>
    /// <param name="projectId">所属项目ID</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> FindAllByProjectId(Int64 projectId)
    {
        if (projectId < 0) return [];

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.ProjectId == projectId);

        return FindAll(_.ProjectId == projectId);
    }

    /// <summary>根据访问级别（1=Public查找</summary>
    /// <param name="accessLevel">访问级别（1=Public</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> FindAllByAccessLevel(Int32 accessLevel)
    {
        if (accessLevel < 0) return [];

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.AccessLevel == accessLevel);

        return FindAll(_.AccessLevel == accessLevel);
    }

    /// <summary>根据业务类型、业务ID查找</summary>
    /// <param name="businessType">业务类型</param>
    /// <param name="businessId">业务ID</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> FindAllByBusinessTypeAndBusinessId(String? businessType, String? businessId)
    {
        if (businessType == null) return [];
        if (businessId == null) return [];

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.BusinessType.EqualIgnoreCase(businessType) && e.BusinessId.EqualIgnoreCase(businessId));

        return FindAll(_.BusinessType == businessType & _.BusinessId == businessId);
    }

    /// <summary>根据所有者用户ID查找</summary>
    /// <param name="ownerId">所有者用户ID</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> FindAllByOwnerId(Int64 ownerId)
    {
        if (ownerId < 0) return [];

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.OwnerId == ownerId);

        return FindAll(_.OwnerId == ownerId);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="hash">文件哈希（MD5/SHA256）</param>
    /// <param name="accessLevel">访问级别（1=Public,2=Private,3=Internal）</param>
    /// <param name="projectId">所属项目ID</param>
    /// <param name="businessType">业务类型</param>
    /// <param name="businessId">业务ID</param>
    /// <param name="ownerId">所有者用户ID</param>
    /// <param name="isScanned">是否已病毒扫描</param>
    /// <param name="isEncrypted">是否加密存储</param>
    /// <param name="start">创建时间开始</param>
    /// <param name="end">创建时间结束</param>
    /// <param name="key">关键字</param>
    /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> Search(String? hash, Int32 accessLevel, Int64 projectId, String? businessType, String? businessId, Int64 ownerId, Boolean? isScanned, Boolean? isEncrypted, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();

        if (!hash.IsNullOrEmpty()) exp &= _.Hash == hash;
        if (accessLevel >= 0) exp &= _.AccessLevel == accessLevel;
        if (projectId >= 0) exp &= _.ProjectId == projectId;
        if (!businessType.IsNullOrEmpty()) exp &= _.BusinessType == businessType;
        if (!businessId.IsNullOrEmpty()) exp &= _.BusinessId == businessId;
        if (ownerId >= 0) exp &= _.OwnerId == ownerId;
        if (isScanned != null) exp &= _.IsScanned == isScanned;
        if (isEncrypted != null) exp &= _.IsEncrypted == isEncrypted;
        exp &= _.CreateTime.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= SearchWhereByKeys(key);

        return FindAll(exp, page);
    }
    #endregion

    #region 字段名
    /// <summary>取得文件条目字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field Id = FindByName("Id");

        /// <summary>文件名</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>原始文件名</summary>
        public static readonly Field OriginalName = FindByName("OriginalName");

        /// <summary>扩展名（.jpg）</summary>
        public static readonly Field Extension = FindByName("Extension");

        /// <summary>MIME类型（image/jpeg）</summary>
        public static readonly Field ContentType = FindByName("ContentType");

        /// <summary>文件大小（字节）</summary>
        public static readonly Field Size = FindByName("Size");

        /// <summary>文件哈希（MD5/SHA256）</summary>
        public static readonly Field Hash = FindByName("Hash");

        /// <summary>存储类型（Local/OSS/S3）</summary>
        public static readonly Field StorageType = FindByName("StorageType");

        /// <summary>相对于项目存储目录的路径</summary>
        public static readonly Field RelativePath = FindByName("RelativePath");

        /// <summary>存储桶名称</summary>
        public static readonly Field BucketName = FindByName("BucketName");

        /// <summary>访问级别（1=Public,2=Private,3=Internal）</summary>
        public static readonly Field AccessLevel = FindByName("AccessLevel");

        /// <summary>已下载次数</summary>
        public static readonly Field DownloadCount = FindByName("DownloadCount");

        /// <summary>所属项目ID</summary>
        public static readonly Field ProjectId = FindByName("ProjectId");

        /// <summary>项目名称（冗余）</summary>
        public static readonly Field ProjectName = FindByName("ProjectName");

        /// <summary>分类（Document/Image/Video等）</summary>
        public static readonly Field Category = FindByName("Category");

        /// <summary>标签（JSON数组或逗号分隔）</summary>
        public static readonly Field Tags = FindByName("Tags");

        /// <summary>业务类型</summary>
        public static readonly Field BusinessType = FindByName("BusinessType");

        /// <summary>业务ID</summary>
        public static readonly Field BusinessId = FindByName("BusinessId");

        /// <summary>所有者用户ID</summary>
        public static readonly Field OwnerId = FindByName("OwnerId");

        /// <summary>所有者名称</summary>
        public static readonly Field OwnerName = FindByName("OwnerName");

        /// <summary>图片宽度</summary>
        public static readonly Field Width = FindByName("Width");

        /// <summary>图片高度</summary>
        public static readonly Field Height = FindByName("Height");

        /// <summary>视频/音频时长（秒）</summary>
        public static readonly Field Duration = FindByName("Duration");

        /// <summary>其他元数据（JSON）</summary>
        public static readonly Field Metadata = FindByName("Metadata");

        /// <summary>是否已病毒扫描</summary>
        public static readonly Field IsScanned = FindByName("IsScanned");

        /// <summary>是否加密存储</summary>
        public static readonly Field IsEncrypted = FindByName("IsEncrypted");

        /// <summary>加密密钥ID</summary>
        public static readonly Field EncryptionKey = FindByName("EncryptionKey");

        /// <summary>创建者ID</summary>
        public static readonly Field CreateUserId = FindByName("CreateUserId");

        /// <summary>创建者</summary>
        public static readonly Field CreateUser = FindByName("CreateUser");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>创建IP</summary>
        public static readonly Field CreateIP = FindByName("CreateIP");

        /// <summary>更新者ID</summary>
        public static readonly Field UpdateUserId = FindByName("UpdateUserId");

        /// <summary>更新者</summary>
        public static readonly Field UpdateUser = FindByName("UpdateUser");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        /// <summary>更新IP</summary>
        public static readonly Field UpdateIP = FindByName("UpdateIP");

        /// <summary>备注说明</summary>
        public static readonly Field Remark = FindByName("Remark");

        /// <summary>IP限流-每分钟最大请求次数(0=不限制)</summary>
        public static readonly Field IpRateLimitPerMinute = FindByName("IpRateLimitPerMinute");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得文件条目字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String Id = "Id";

        /// <summary>文件名</summary>
        public const String Name = "Name";

        /// <summary>原始文件名</summary>
        public const String OriginalName = "OriginalName";

        /// <summary>扩展名（.jpg）</summary>
        public const String Extension = "Extension";

        /// <summary>MIME类型（image/jpeg）</summary>
        public const String ContentType = "ContentType";

        /// <summary>文件大小（字节）</summary>
        public const String Size = "Size";

        /// <summary>文件哈希（MD5/SHA256）</summary>
        public const String Hash = "Hash";

        /// <summary>存储类型（Local/OSS/S3）</summary>
        public const String StorageType = "StorageType";

        /// <summary>相对于项目存储目录的路径</summary>
        public const String RelativePath = "RelativePath";

        /// <summary>存储桶名称</summary>
        public const String BucketName = "BucketName";

        /// <summary>访问级别（1=Public,2=Private,3=Internal）</summary>
        public const String AccessLevel = "AccessLevel";

        /// <summary>已下载次数</summary>
        public const String DownloadCount = "DownloadCount";

        /// <summary>所属项目ID</summary>
        public const String ProjectId = "ProjectId";

        /// <summary>项目名称（冗余）</summary>
        public const String ProjectName = "ProjectName";

        /// <summary>分类（Document/Image/Video等）</summary>
        public const String Category = "Category";

        /// <summary>标签（JSON数组或逗号分隔）</summary>
        public const String Tags = "Tags";

        /// <summary>业务类型</summary>
        public const String BusinessType = "BusinessType";

        /// <summary>业务ID</summary>
        public const String BusinessId = "BusinessId";

        /// <summary>所有者用户ID</summary>
        public const String OwnerId = "OwnerId";

        /// <summary>所有者名称</summary>
        public const String OwnerName = "OwnerName";

        /// <summary>图片宽度</summary>
        public const String Width = "Width";

        /// <summary>图片高度</summary>
        public const String Height = "Height";

        /// <summary>视频/音频时长（秒）</summary>
        public const String Duration = "Duration";

        /// <summary>其他元数据（JSON）</summary>
        public const String Metadata = "Metadata";

        /// <summary>是否已病毒扫描</summary>
        public const String IsScanned = "IsScanned";

        /// <summary>是否加密存储</summary>
        public const String IsEncrypted = "IsEncrypted";

        /// <summary>加密密钥ID</summary>
        public const String EncryptionKey = "EncryptionKey";

        /// <summary>创建者ID</summary>
        public const String CreateUserId = "CreateUserId";

        /// <summary>创建者</summary>
        public const String CreateUser = "CreateUser";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>创建IP</summary>
        public const String CreateIP = "CreateIP";

        /// <summary>更新者ID</summary>
        public const String UpdateUserId = "UpdateUserId";

        /// <summary>更新者</summary>
        public const String UpdateUser = "UpdateUser";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";

        /// <summary>更新IP</summary>
        public const String UpdateIP = "UpdateIP";

        /// <summary>备注说明</summary>
        public const String Remark = "Remark";

        /// <summary>IP限流-每分钟最大请求次数(0=不限制)</summary>
        public const String IpRateLimitPerMinute = "IpRateLimitPerMinute";
    }
    #endregion
}
