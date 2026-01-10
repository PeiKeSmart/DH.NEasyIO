using System;
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

namespace HlktechFileStorage.Entity;

public partial class FileOperationLog : DHEntityBase<FileOperationLog>
{
    #region 对象操作
    // 控制最大缓存数量，Find/FindAll查询方法在表行数小于该值时走实体缓存
    private static Int32 MaxCacheCount = 1000;

    static FileOperationLog()
    {
        // 累加字段，生成 Update xx Set Count=Count+1234 Where xxx
        //var df = Meta.Factory.AdditionalFields;
        //df.Add(nameof(FileId));

        // 过滤器 UserModule、TimeModule、IPModule
        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add<IPModule>();
        Meta.Modules.Add<TraceModule>();
    }

    /// <summary>验证并修补数据，返回验证结果，或者通过抛出异常的方式提示验证失败。</summary>
    /// <param name="method">添删改方法</param>
    public override Boolean Valid(DataMethod method)
    {
        //if (method == DataMethod.Delete) return true;
        // 如果没有脏数据，则不需要进行任何处理
        if (!HasDirty) return true;

        // 这里验证参数范围，建议抛出参数异常，指定参数名，前端用户界面可以捕获参数异常并聚焦到对应的参数输入框
        if (OperationType.IsNullOrEmpty()) throw new ArgumentNullException(nameof(OperationType), "操作类型不能为空！");

        // 建议先调用基类方法，基类做一些统一处理
        if (!base.Valid(method)) return false;

        // 在新插入数据或者修改了指定字段时进行修正
        //if (method == DataMethod.Insert && !Dirtys[nameof(CreateTime)]) CreateTime = DateTime.Now;

        return true;
    }

    #endregion

    #region 扩展属性
    #endregion

    #region 扩展查询
    /// <summary>根据文件ID查询</summary>
    /// <param name="fileId">文件ID</param>
    /// <returns></returns>
    public static IList<FileOperationLog> FindByFileId(Int64 fileId)
    {
        if (fileId <= 0) return [];
        return FindAll(_.FileId == fileId, _.CreateTime.Desc(), null, 0, 0);
    }

    /// <summary>根据操作类型查询</summary>
    /// <param name="operationType">操作类型</param>
    /// <returns></returns>
    public static IList<FileOperationLog> FindByOperationType(String operationType)
    {
        if (operationType.IsNullOrEmpty()) return [];
        return FindAll(_.OperationType == operationType, _.CreateTime.Desc(), null, 0, 0);
    }
    #endregion

    #region 高级查询

    // Select Count(Id) as Id,OperationType From FileOperationLog Where CreateTime>'2020-01-24 00:00:00' Group By OperationType Order By Id Desc limit 20
    static readonly FieldCache<FileOperationLog> _OperationTypeCache = new(nameof(OperationType))
    {
        //Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
    };

    /// <summary>获取操作类型列表，字段缓存10分钟，分组统计数据最多的前20种，用于魔方前台下拉选择</summary>
    /// <returns></returns>
    public static IDictionary<String, String> GetOperationTypeList() => _OperationTypeCache.FindAllName();
    #endregion

    #region 业务操作
    /// <summary>记录文件操作日志</summary>
    /// <param name="entry">文件条目</param>
    /// <param name="operationType">操作类型</param>
    /// <param name="success">是否成功</param>
    /// <param name="errorMessage">错误信息</param>
    /// <param name="operationDetail">操作详情</param>
    /// <param name="duration">耗时（毫秒）</param>
    /// <returns></returns>
    public static FileOperationLog Log(FileEntry entry, String operationType, Boolean success = true, String errorMessage = null, String operationDetail = null, Int32 duration = 0)
    {
        var log = new FileOperationLog
        {
            FileId = entry?.Id ?? 0,
            FileName = entry?.Name,
            ProjectId = entry?.ProjectId ?? 0,
            ProjectName = entry?.ProjectName,
            OperationType = operationType,
            OperationDetail = operationDetail,
            FilePath = entry?.RelativePath,
            FileSize = entry?.Size ?? 0,
            FileHash = entry?.Hash,
            Success = success,
            ErrorMessage = errorMessage,
            Duration = duration
        };

        try
        {
            log.Insert();
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }

        return log;
    }

    /// <summary>转为模型对象</summary>
    /// <returns></returns>
    public IFileOperationLog ToModel()
    {
        var model = new FileOperationLog();
        model.Copy(this);

        return model;
    }

    #endregion
}
