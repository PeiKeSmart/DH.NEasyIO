﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using NewLife.Log;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Threading;
using NewLife.Web;
using XCode;
using XCode.Cache;
using XCode.Configuration;
using XCode.DataAccessLayer;
using XCode.Membership;
using XCode.Shards;

namespace EasyWeb.Data;

public partial class FileSource : Entity<FileSource>
{
    #region 对象操作
    static FileSource()
    {
        // 累加字段，生成 Update xx Set Count=Count+1234 Where xxx
        //var df = Meta.Factory.AdditionalFields;
        //df.Add(nameof(Period));

        // 过滤器 UserModule、TimeModule、IPModule
        Meta.Modules.Add(new UserModule { AllowEmpty = false });
        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add(new IPModule { AllowEmpty = false });

        // 实体缓存
        // var ec = Meta.Cache;
        // ec.Expire = 60;

        // 单对象缓存
        var sc = Meta.SingleCache;
        // sc.Expire = 60;
        sc.FindSlaveKeyMethod = k => Find(_.Name == k);
        sc.GetSlaveKeyMethod = e => e.Name;
    }

    /// <summary>验证并修补数据，返回验证结果，或者通过抛出异常的方式提示验证失败。</summary>
    /// <param name="method">添删改方法</param>
    public override Boolean Valid(DataMethod method)
    {
        //if (method == DataMethod.Delete) return true;
        // 如果没有脏数据，则不需要进行任何处理
        if (!HasDirty) return true;

        // 建议先调用基类方法，基类方法会做一些统一处理
        if (!base.Valid(method)) return false;

        if (StorageId == 0) StorageId = FileStorage.FindAllWithCache().FirstOrDefault(e => e.Enable)?.Id ?? 0;

        return true;
    }

    /// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected override void InitData()
    {
        // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
        if (Meta.Session.Count > 0) return;

        if (XTrace.Debug) XTrace.WriteLine("开始初始化FileSource[文件源]数据……");

        // https://github.com/dotnet/core/blob/main/release-notes/releases-index.json
        var entity = new FileSource
        {
            Name = "dotnet6",
            Kind = "dotNet",
            Url = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/6.0/releases.json",
            Enable = true,
            Period = 86400,
            Blacks = "*preview*,*rc*",
        };
        entity.Insert();

        entity = new FileSource
        {
            Name = "dotnet7",
            Kind = "dotNet",
            Url = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/7.0/releases.json",
            Enable = true,
            Period = 86400,
            Blacks = "*preview*,*rc*",
        };
        entity.Insert();

        entity = new FileSource
        {
            Name = "dotnet8",
            Kind = "dotNet",
            Url = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/8.0/releases.json",
            Enable = true,
            Period = 86400,
            Blacks = "*preview*,*rc*",
        };
        entity.Insert();

        entity = new FileSource
        {
            Name = "dotnet9",
            Kind = "dotNet",
            Url = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/9.0/releases.json",
            Enable = true,
            Period = 86400,
            //Blacks = "preview,rc",
        };
        entity.Insert();

        if (XTrace.Debug) XTrace.WriteLine("完成初始化FileSource[文件源]数据！");
    }
    #endregion

    #region 扩展属性
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id">编号</param>
    /// <returns>实体对象</returns>
    public static FileSource FindById(Int32 id)
    {
        if (id <= 0) return null;

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Id == id);

        // 单对象缓存
        return Meta.SingleCache[id];

        //return Find(_.Id == id);
    }

    /// <summary>根据名称查找</summary>
    /// <param name="name">名称</param>
    /// <returns>实体对象</returns>
    public static FileSource FindByName(String name)
    {
        if (name.IsNullOrEmpty()) return null;

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Name.EqualIgnoreCase(name));

        // 单对象缓存
        //return Meta.SingleCache.GetItemWithSlaveKey(name) as FileSource;

        return Find(_.Name == name);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="name">名称</param>
    /// <param name="start">更新时间开始</param>
    /// <param name="end">更新时间结束</param>
    /// <param name="key">关键字</param>
    /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
    /// <returns>实体列表</returns>
    public static IList<FileSource> Search(String name, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();

        if (!name.IsNullOrEmpty()) exp &= _.Name == name;
        exp &= _.UpdateTime.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= _.Name.Contains(key) | _.Kind.Contains(key) | _.Url.Contains(key) | _.CreateIP.Contains(key) | _.UpdateIP.Contains(key) | _.Remark.Contains(key);

        return FindAll(exp, page);
    }

    // Select Count(Id) as Id,Category From FileSource Where CreateTime>'2020-01-24 00:00:00' Group By Category Order By Id Desc limit 20
    //static readonly FieldCache<FileSource> _CategoryCache = new FieldCache<FileSource>(nameof(Category))
    //{
    //Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
    //};

    ///// <summary>获取类别列表，字段缓存10分钟，分组统计数据最多的前20种，用于魔方前台下拉选择</summary>
    ///// <returns></returns>
    //public static IDictionary<String, String> GetCategoryList() => _CategoryCache.FindAllName();
    #endregion

    #region 业务操作
    private String[] _blacks;
    private String[] _whites;
    public Boolean IsMatch(String input)
    {
        // 黑白名单
        _blacks ??= Blacks?.Split(",") ?? [];
        _whites ??= Whites?.Split(",") ?? [];

        // 如果指定黑名单，只要命中，则不匹配
        if (_blacks.Length > 0 && _blacks.Any(e => e.IsMatch(input))) return false;

        // 如果指定白名单，则必须命中
        if (_whites.Length > 0) return _whites.Any(e => e.IsMatch(input));

        // 如果未指定白名单，则全部通过
        return true;
    }
    #endregion
}
