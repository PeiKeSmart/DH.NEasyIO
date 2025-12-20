using System.Security.Cryptography;
using System.Text;

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
    /// <param name="remotePath">远程文件路径（如：users/avatar.jpg）</param>
    /// <param name="category">分类（可选）</param>
    /// <param name="businessType">业务类型（可选）</param>
    /// <param name="businessId">业务ID（可选）</param>
    /// <param name="isPublic">是否公开</param>
    /// <returns></returns>
    public async Task<UploadResult> UploadFileAsync(String localFilePath, String remotePath,
        String category = null, String businessType = null, String businessId = null, Boolean isPublic = false)
    {
        Console.WriteLine($"开始上传文件：{localFilePath} -> {remotePath}");

        // 1. 读取文件
        if (!File.Exists(localFilePath))
            throw new FileNotFoundException("文件不存在", localFilePath);

        var fileBytes = await File.ReadAllBytesAsync(localFilePath);
        Console.WriteLine($"文件大小：{fileBytes.Length:N0} 字节");

        // 2. 构建URL
        var path = $"/api/v1/io/{remotePath}";
        var queryParams = new List<String>();
        if (!String.IsNullOrEmpty(category)) queryParams.Add($"category={category}");
        if (!String.IsNullOrEmpty(businessType)) queryParams.Add($"businessType={businessType}");
        if (!String.IsNullOrEmpty(businessId)) queryParams.Add($"businessId={businessId}");
        queryParams.Add($"isPublic={isPublic}");

        var queryString = String.Join("&", queryParams);
        var url = $"{_baseUrl}{path}?{queryString}";

        // 3. 生成签名
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = GenerateSignature("PUT", path, queryString, fileBytes, timestamp);

        Console.WriteLine($"签名信息：");
        Console.WriteLine($"  ProjectCode: {_projectCode}");
        Console.WriteLine($"  Timestamp: {timestamp}");
        Console.WriteLine($"  Signature: {signature}");

        // 4. 构建请求
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Add("X-Project-Code", _projectCode);
        request.Headers.Add("X-Timestamp", timestamp);
        request.Headers.Add("X-Signature", signature);
        request.Content = new ByteArrayContent(fileBytes);
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        // 5. 发送请求
        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"上传失败：{response.StatusCode}");
            Console.WriteLine($"错误信息：{result}");
            throw new Exception($"上传失败：{result}");
        }

        Console.WriteLine($"上传成功：{result}");
        return System.Text.Json.JsonSerializer.Deserialize<UploadResult>(result);
    }

    /// <summary>下载文件</summary>
    /// <param name="remotePath">远程文件路径</param>
    /// <param name="saveToPath">保存到本地路径（可选）</param>
    /// <returns></returns>
    public async Task<Byte[]> DownloadFileAsync(String remotePath, String saveToPath = null)
    {
        Console.WriteLine($"开始下载文件：{remotePath}");

        var path = $"/api/v1/io/{remotePath}";
        var url = $"{_baseUrl}{path}";

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = GenerateSignature("GET", path, "", null, timestamp);

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
    /// <param name="remotePath">远程文件路径</param>
    /// <returns></returns>
    public async Task<Boolean> DeleteFileAsync(String remotePath)
    {
        Console.WriteLine($"开始删除文件：{remotePath}");

        var path = $"/api/v1/io/{remotePath}";
        var url = $"{_baseUrl}{path}";

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = GenerateSignature("DELETE", path, "", null, timestamp);

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
    private String GenerateSignature(String method, String path, String query, Byte[] body, String timestamp)
    {
        // 1. 构建签名字符串
        var bodyStr = body != null && body.Length > 0 ? Convert.ToBase64String(body) : "";
        var signString = $"{method}\n{path}\n{SortQuery(query)}\n{bodyStr}\n{timestamp}";

        // 调试输出
        //Console.WriteLine($"SignString: {signString.Replace("\n", "\\n")}");

        // 2. 计算HMAC-SHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signString));
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
    public Boolean Duplicate { get; set; }
}
