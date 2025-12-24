using System;
using System.IO;
using System.Threading.Tasks;

using NewLife.Log;

namespace Test;

class Program
{
    static async Task Main(String[] args)
    {
        XTrace.UseConsole();

        try
        {
            await TestEasyIOClientAsync();
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }

        Console.WriteLine("OK!");
        Console.ReadKey();
    }

    /// <summary>测试EasyIO客户端</summary>
    static async Task TestEasyIOClientAsync()
    {
        Console.WriteLine("=== EasyIO API 客户端测试 ===\n");

        // 配置（实际使用时请替换为真实值）
        var baseUrl = "http://localhost:5000";
        var projectCode = "TEST_PROJECT";  // 替换为你的项目编码
        var apiSecret = "your-api-secret-key";  // 替换为你的API密钥

        var client = new EasyIOClient(baseUrl, projectCode, apiSecret);

        // 测试文件路径
        var testFilePath = "test.txt";
        var downloadPath = "downloaded_test.txt";

        // 1. 创建测试文件
        Console.WriteLine("\n【1】创建测试文件");
        await File.WriteAllTextAsync(testFilePath, $"测试内容 - {DateTime.Now}");
        Console.WriteLine($"测试文件已创建：{testFilePath}\n");

        try
        {
            // 2. 上传文件
            Console.WriteLine("\n【2】上传文件");
            var uploadResult = await client.UploadFileAsync(
                localFilePath: testFilePath,
                remotePath: "test/demo/test.txt",
                category: "测试",
                businessType: "Demo",
                businessId: "123",
                isPublic: false
            );

            Console.WriteLine($"\n上传成功！");
            Console.WriteLine($"  文件ID: {uploadResult.Id}");
            Console.WriteLine($"  文件名: {uploadResult.Name}");
            Console.WriteLine($"  原始名: {uploadResult.OriginalName}");
            Console.WriteLine($"  大小: {uploadResult.Length} 字节");
            Console.WriteLine($"  哈希: {uploadResult.Hash}");
            Console.WriteLine($"  重复: {uploadResult.Duplicate}");

            // 3. 下载文件
            Console.WriteLine("\n\n【3】下载文件");
            await client.DownloadFileAsync("test/demo/test.txt", downloadPath);

            // 4. 验证文件内容
            Console.WriteLine("\n【4】验证文件内容");
            var originalContent = await File.ReadAllTextAsync(testFilePath);
            var downloadedContent = await File.ReadAllTextAsync(downloadPath);

            if (originalContent == downloadedContent)
            {
                Console.WriteLine("✓ 文件内容验证成功，上传下载一致！");
            }
            else
            {
                Console.WriteLine("✗ 文件内容不一致！");
            }

            // 5. 再次上传相同文件（测试去重）
            Console.WriteLine("\n\n【5】测试文件去重");
            var uploadResult2 = await client.UploadFileAsync(
                localFilePath: testFilePath,
                remotePath: "test/demo/test2.txt",
                isPublic: false
            );

            Console.WriteLine($"  重复标记: {uploadResult2.Duplicate}");
            Console.WriteLine($"  哈希相同: {uploadResult.Hash == uploadResult2.Hash}");

            // 6. 删除文件
            Console.WriteLine("\n\n【6】删除文件");
            var deleteSuccess = await client.DeleteFileAsync("test/demo/test.txt");
            Console.WriteLine($"  删除结果: {(deleteSuccess ? "成功" : "失败")}");

            Console.WriteLine("\n\n=== 所有测试完成 ===");
        }
        finally
        {
            // 清理本地测试文件
            if (File.Exists(testFilePath)) File.Delete(testFilePath);
            if (File.Exists(downloadPath)) File.Delete(downloadPath);
        }
    }
}