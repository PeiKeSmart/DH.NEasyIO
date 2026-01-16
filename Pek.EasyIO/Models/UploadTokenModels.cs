using System;

namespace Pek.EasyIO.Models;

/// <summary>上传令牌响应</summary>
public class UploadTokenResponse
{
    /// <summary>上传令牌（用于 X-Upload-Token 请求头）</summary>
    public String UploadToken { get; set; }

    /// <summary>分片上传地址</summary>
    public String ChunkUploadUrl { get; set; }

    /// <summary>合并分片地址</summary>
    public String MergeUrl { get; set; }

    /// <summary>查询上传状态地址</summary>
    public String StatusUrl { get; set; }

    /// <summary>令牌过期时间（UTC）</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>文件哈希</summary>
    public String FileHash { get; set; }

    /// <summary>建议的单个分片大小（字节）</summary>
    public Int32 MaxChunkSize { get; set; }

    /// <summary>提示信息</summary>
    public String Message { get; set; }
}
