using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Test;

/// <summary>EasyIO API客户端</summary>
public class EasyIOClient
{
    private readonly String _baseUrl;
    private readonly String _projectCode;
    private readonly String _apiSecret;
    private readonly HttpClient _httpClient;

    /// <summary>实例化EasyIO客户端</summary>
    /// <param name="baseUrl">API地址（如：http://localhost:5000）</param>
    /// <param name="projectCode">项目编码</param>
    /// <param name="apiSecret">API密钥</param>
    public EasyIOClient(String baseUrl, String projectCode, String apiSecret)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _projectCode = projectCode;
        _apiSecret = apiSecret;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
    }

    /// <summary>上传文件</summary>
    /// <param name="localFilePath">本地文件路径</param>
    /// <param name="remark">备注说明（必填）</param>
    /// <param name="category">分类（可选）</param>
    /// <param name="businessType">业务类型（可选）</param>
    /// <param name="businessId">业务ID（可选）</param>
    /// <param name="isPublic">是否公开</param>
    /// <returns></returns>
    public async Task<UploadResult> UploadFileAsync(String localFilePath, String remark,
        String category = null, String businessType = null, String businessId = null, bool isPublic = false)
    {
        Console.WriteLine($"开始上传文件：{localFilePath}");

        // 1. 读取文件
        if (!File.Exists(localFilePath))
            throw new FileNotFoundException("文件不存在", localFilePath);

        var fileBytes = await File.ReadAllBytesAsync(localFilePath);
        var fileName = Path.GetFileName(localFilePath);
        Console.WriteLine($"文件名：{fileName}，大小：{fileBytes.Length:N0} 字节");

        // 2. 构建 URL
        var path = "/api/v1/io";
        var url = $"{_baseUrl}{path}";

        // 3. 计算文件哈希（用于签名，避免大文件内存问题）
        var fileHash = await CalculateFileHashAsync(localFilePath);
        Console.WriteLine($"文件哈希：{fileHash}");

        // 4. 构建 multipart/form-data
        var formData = new MultipartFormDataContent();
        formData.Add(new ByteArrayContent(fileBytes), "file", fileName);
        formData.Add(new StringContent(remark), "remark");
        if (!String.IsNullOrEmpty(category)) formData.Add(new StringContent(category), "category");
        if (!String.IsNullOrEmpty(businessType)) formData.Add(new StringContent(businessType), "businessType");
        if (!String.IsNullOrEmpty(businessId)) formData.Add(new StringContent(businessId), "businessId");
        formData.Add(new StringContent(isPublic.ToString().ToLower()), "isPublic");

        // 5. 生成签名（使用文件哈希）
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = GenerateSignature("PUT", path, "", fileHash, timestamp);

        Console.WriteLine($"签名信息：");
        Console.WriteLine($"  ProjectCode: {_projectCode}");
        Console.WriteLine($"  Timestamp: {timestamp}");
        Console.WriteLine($"  Signature: {signature}");

        // 5. 构建请求
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Add("X-Project-Code", _projectCode);
        request.Headers.Add("X-Timestamp", timestamp);
        request.Headers.Add("X-Signature", signature);
        request.Content = formData;

        // 6. 发送请求
        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"上传失败：{response.StatusCode}");
            Console.WriteLine($"错误信息：{result}");
            throw new Exception($"上传失败：{result}");
        }

        Console.WriteLine($"上传成功：{result}");
        
        // JSON 反序列化选项（API 返回 camelCase，C# 模型使用 PascalCase）
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return System.Text.Json.JsonSerializer.Deserialize<UploadResult>(result, options);
    }

    /// <summary>下载文件</summary>
    /// <param name="fileId">文件数据库ID</param>
    /// <param name="saveToPath">保存到本地路径（可选）</param>
    /// <param name="inline">是否内联显示（预览）</param>
    /// <returns></returns>
    public async Task<Byte[]> DownloadFileAsync(Int64 fileId, String saveToPath = null, bool inline = false)
    {
        Console.WriteLine($"开始下载文件：ID={fileId}");

        var path = $"/api/v1/io/{fileId}";
        var queryString = inline ? "inline=true" : "";
        var url = $"{_baseUrl}{path}" + (String.IsNullOrEmpty(queryString) ? "" : $"?{queryString}");

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = GenerateSignature("GET", path, queryString, "", timestamp);

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Project-Code", _projectCode);
        request.Headers.Add("X-Timestamp", timestamp);
        request.Headers.Add("X-Signature", signature);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"下载失败：{error}");
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Console.WriteLine($"下载成功，文件大小：{bytes.Length:N0} 字节");

        if (!String.IsNullOrEmpty(saveToPath))
        {
            await File.WriteAllBytesAsync(saveToPath, bytes);
            Console.WriteLine($"已保存到：{saveToPath}");
        }

        return bytes;
    }

    /// <summary>删除文件</summary>
    /// <param name="fileId">文件数据库ID</param>
    /// <returns></returns>
    public async Task<Boolean> DeleteFileAsync(Int64 fileId)
    {
        Console.WriteLine($"开始删除文件：ID={fileId}");

        // DELETE /api/v1/io?id={fileId}
        // ASP.NET Core 会自动将查询参数 id 绑定到 Delete(Int64 id) 方法参数
        var path = "/api/v1/io";
        var queryString = $"id={fileId}";
        var url = $"{_baseUrl}{path}?{queryString}";

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = GenerateSignature("DELETE", path, queryString, "", timestamp);

        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Add("X-Project-Code", _projectCode);
        request.Headers.Add("X-Timestamp", timestamp);
        request.Headers.Add("X-Signature", signature);

        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"删除失败：{result}");
            return false;
        }

        Console.WriteLine($"删除成功");
        return true;
    }

    /// <summary>生成API签名</summary>
    private String GenerateSignature(String method, String path, String query, String bodyHash, String timestamp)
    {
        // 使用文件哈希代替完整内容，避免大文件内存和性能问题
        // bodyHash: 文件的 MD5/SHA256 哈希（32/64字符），而非整个文件的 Base64（可能几百MB）
        var signString = $"{method}\n{path}\n{SortQuery(query)}\n{bodyHash}\n{timestamp}";

        // 调试输出
        //Console.WriteLine($"SignString: {signString.Replace("\n", "\\n")}");

        // 2. 计算HMAC-SHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signString));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    /// <summary>计算文件哈希（流式处理，节省内存）</summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>MD5 哈希值（小写十六进制）</returns>
    private async Task<String> CalculateFileHashAsync(String filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var md5 = MD5.Create();
        var hash = await md5.ComputeHashAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    /// <summary>排序查询参数（用于签名一致性）</summary>
    private String SortQuery(String query)
    {
        if (String.IsNullOrEmpty(query)) return "";

        var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        Array.Sort(pairs);
        return String.Join("&", pairs);
    }
}

/// <summary>上传结果</summary>
public class UploadResult
{
    public Int64 Id { get; set; }
    public String Name { get; set; }
    public String OriginalName { get; set; }
    public Int64 Length { get; set; }
    public String Hash { get; set; }
    public DateTime Time { get; set; }
    public Boolean IsDirectory { get; set; }
    public Int64 ProjectId { get; set; }
    public String Category { get; set; }
    public Boolean IsPublic { get; set; }
    public String Remark { get; set; }
    public Boolean Duplicate { get; set; }
}
