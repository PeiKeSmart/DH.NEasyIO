# Pek.EasyIO 文件存储系统 API 使用文档

## 目录

- [1. 快速开始](#1-快速开始)
- [2. 项目配置](#2-项目配置)
- [3. API 鉴权机制](#3-api-鉴权机制)
- [4. 接口说明](#4-接口说明)
- [5. 客户端调用示例](#5-客户端调用示例)
- [6. 错误码说明](#6-错误码说明)
- [7. 最佳实践](#7-最佳实践)

---

## 1. 快速开始

### 1.1 系统架构

```
┌─────────────┐         HMAC签名鉴权           ┌─────────────┐
│   客户端A   │  ──────────────────────────>  │             │
│ (Project A) │                                │  EasyIO API │
└─────────────┘                                │   服务端    │
                                               │             │
┌─────────────┐         独立密钥鉴权           │             │
│   客户端B   │  ──────────────────────────>  │  文件存储   │
│ (Project B) │                                │  数据库管理 │
└─────────────┘                                │  限流防护   │
                                               └─────────────┘
```

### 1.2 核心特性

- ✅ **多项目隔离**：每个项目独立的 API 密钥和配额
- ✅ **HMAC 签名**：基于 SHA-256 的安全签名验证
- ✅ **防重放攻击**：时间戳验证，默认 5 分钟过期
- ✅ **文件去重**：基于 MD5 哈希的自动去重
- ✅ **限流保护**：IP 和文件级别的双重限流
- ✅ **存储配额**：可配置的项目存储空间限制
- ✅ **扩展名控制**：白名单/黑名单机制

---

## 2. 项目配置

### 2.1 创建项目

在数据库 `FileProject` 表中创建项目记录：

```sql
INSERT INTO FileProject (
    Code, Name, Description, ApiSecret,
    MaxStorageSize, MaxFileSize,
    AllowedExtensions, ForbiddenExtensions,
    DefaultAccessLevel, RateLimitPerIp, RateLimitPerFile,
    Enable, Status, CreateTime
) VALUES (
    'myapp',                          -- 项目编码（唯一）
    '我的应用',                        -- 项目名称
    '我的第一个应用项目',              -- 描述
    'your-secret-key-here-32-chars',  -- API密钥（请使用随机字符串）
    10737418240,                      -- 最大存储10GB（字节）
    104857600,                        -- 单文件最大100MB
    '.jpg,.png,.pdf,.zip,.docx',      -- 允许的扩展名
    '.exe,.bat,.sh',                  -- 禁止的扩展名
    2,                                -- 默认访问级别（1=Public,2=Private）
    10,                               -- 每IP每分钟10次
    20,                               -- 每文件每分钟20次
    1,                                -- 启用
    1,                                -- 状态
    GETDATE()
);
```

### 2.2 生成 API 密钥

**推荐使用强随机字符串（至少 32 字符）：**

```csharp
// C# 生成示例
var apiSecret = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) 
    + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
// 结果：qXR8vK9mL4nP2sT6wY1zA==jH5fD8gN3cM7xV4bU0oE==
```

```bash
# Linux/Mac 生成
openssl rand -base64 32
```

```powershell
# PowerShell 生成
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

### 2.3 应用配置（appsettings.json）

```json
{
  "EasyIO": {
    "Path": "../files",              // 文件存储路径
    "RateLimitPerIp": 10,            // 全局IP限流
    "RateLimitPerFile": 20           // 全局文件限流
  }
}
```

---

## 3. API 鉴权机制

### 3.1 鉴权流程

```
客户端                                  服务端
  │                                      │
  ├──1. 准备请求参数                      │
  │   - projectCode: myapp               │
  │   - timestamp: 1703001234            │
  │   - 其他业务参数                      │
  │                                      │
  ├──2. 构建签名字符串                    │
  │   Method\nPath\nQuery\nBody\nTimestamp│
  │                                      │
  ├──3. 使用 ApiSecret 计算 HMAC-SHA256  │
  │   signature = HMAC(signString, secret)│
  │                                      │
  ├──4. 发送请求（携带签名）──────────────>│
  │   Headers:                           │
  │     X-Project-Code: myapp            │
  │     X-Timestamp: 1703001234          │
  │     X-Signature: abc123...           │
  │                                      │
  │                         <────5. 验证签名
  │                                      ├─时间戳验证
  │                                      ├─查找项目配置
  │                                      ├─重新计算签名
  │                                      └─比对签名
  │                                      │
  │ <──────6. 返回结果（成功/失败）────────┤
```

### 3.2 签名算法

#### 签名字符串格式

```
{HTTPMethod}\n
{Path}\n
{QueryString(sorted)}\n
{Body}\n
{Timestamp}
```

#### 示例

**请求：**
```
PUT /api/v1/io/test.jpg?category=avatar&isPublic=true
Body: [binary data]
```

**签名字符串：**
```
PUT
/api/v1/io/test.jpg
category=avatar&isPublic=true
[binary data]
1703001234
```

**计算签名：**
```csharp
var signString = "PUT\n/api/v1/io/test.jpg\ncategory=avatar&isPublic=true\n[body]\n1703001234";
var signature = HMACSHA256(signString, apiSecret);
```

### 3.3 传递方式

支持两种方式传递鉴权参数：

#### 方式1：HTTP 请求头（推荐）

```http
PUT /api/v1/io/test.jpg HTTP/1.1
Host: api.example.com
X-Project-Code: myapp
X-Timestamp: 1703001234
X-Signature: abc123def456...
Content-Type: application/octet-stream
```

#### 方式2：URL 查询参数

```
PUT /api/v1/io/test.jpg?projectCode=myapp&timestamp=1703001234&signature=abc123...
```

---

## 4. 接口说明

### 4.1 上传文件

**接口地址：** `PUT /api/v1/io/{filename}`

**请求方法：** PUT

**鉴权要求：** 必需

**请求参数：**

| 参数 | 位置 | 类型 | 必填 | 说明 |
|------|------|------|------|------|
| filename | Path | String | 是 | 文件名（可含路径如：images/avatar.jpg） |
| category | Query | String | 否 | 文件分类（如：avatar、document） |
| businessType | Query | String | 否 | 业务类型（如：order、user） |
| businessId | Query | String | 否 | 业务ID（如：订单号、用户ID） |
| isPublic | Query | Boolean | 否 | 是否公开（默认false） |
| projectCode | Header/Query | String | 是 | 项目编码 |
| timestamp | Header/Query | String | 是 | Unix时间戳（秒） |
| signature | Header/Query | String | 是 | HMAC签名 |

**请求体：** 文件二进制流

**响应示例（成功）：**

```json
{
  "id": 123,
  "name": "test.jpg",
  "originalName": "test.jpg",
  "length": 102400,
  "hash": "5d41402abc4b2a76b9719d911017c592",
  "time": "2025-12-20T10:30:00",
  "isDirectory": false,
  "projectId": 1,
  "category": "avatar",
  "isPublic": false
}
```

**响应示例（去重）：**

```json
{
  "id": 100,
  "name": "test.jpg",
  "originalName": "test.jpg",
  "length": 102400,
  "hash": "5d41402abc4b2a76b9719d911017c592",
  "time": "2025-12-19T15:20:00",
  "isDirectory": false,
  "duplicate": true
}
```

### 4.2 下载文件

**接口地址：** `GET /api/v1/io/{filename}`

**请求方法：** GET

**鉴权要求：** 根据文件访问级别

**请求参数：**

| 参数 | 位置 | 类型 | 必填 | 说明 |
|------|------|------|------|------|
| filename | Path | String | 是 | 文件名（相对路径） |

**响应：** 文件二进制流

**响应头：**
```
Content-Type: application/octet-stream
Content-Disposition: attachment; filename="test.jpg"
```

### 4.3 重命名文件

**接口地址：** `PATCH /api/v1/io/{id}/rename`

**请求方法：** PATCH

**鉴权要求：** 必需

**请求参数：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | Int64 | 是 | 文件数据库ID（路径参数） |
| newOriginalName | String | 是 | 新的原始文件名（含扩展名，表单参数） |

**功能说明：**
- **同步重命名**：同时修改物理文件名和数据库记录（`Name`、`OriginalName`、`RelativePath`）
- **保持目录结构**：仅修改文件名，保持原有目录结构不变
- **自动去重**：重新生成带时间戳和 GUID 的唯一文件名，避免重名冲突
- **扩展名验证**：新文件名的扩展名必须符合项目允许的扩展名规则
- **权限控制**：仅能重命名本项目的文件
- **异常回滚**：如果数据库更新失败，自动回滚物理文件重命名操作

**请求示例：**

```bash
curl -X PATCH "https://your-api.com/api/v1/io/12345/rename" \
  -H "X-Project-Code: ProjectA" \
  -H "X-Timestamp: 1738339200" \
  -H "X-Signature: a1b2c3..." \
  -H "X-External-UserId: user_001" \
  -F "newOriginalName=新产品手册_2024版.pdf"
```

**响应示例：**

```json
{
  "code": 200,
  "message": "文件重命名成功",
  "data": {
    "id": 12345,
    "oldName": "20251224093045_report_a1b2c3d4.pdf",
    "newName": "20260113102030_新产品手册_2024版_b2c3d4e5.pdf",
    "oldOriginalName": "report.pdf",
    "newOriginalName": "新产品手册_2024版.pdf",
    "oldRelativePath": "Document/2025/12/24/20251224093045_report_a1b2c3d4.pdf",
    "newRelativePath": "Document/2025/12/24/20260113102030_新产品手册_2024版_b2c3d4e5.pdf",
    "extension": ".pdf",
    "renamed": true
  }
}
```

**错误码：**

| 错误码 | 说明 |
|--------|------|
| 10000 | 参数错误（文件ID无效、新文件名为空、缺少必填请求头等） |
| 10001 | 文件记录不存在 |
| 10003 | 文件所属项目不存在 |
| 10004 | 无权重命名其他项目的文件 |
| 10005 | 不支持的文件类型 |
| 10006 | 物理文件不存在，无法重命名 |
| 10007 | 目标文件名已存在 |
| 50000 | 服务器内部错误 |

### 4.4 移动文件

**接口地址：** `PATCH /api/v1/io/{id}/move`

**请求方法：** PATCH

**鉴权要求：** 必需

**请求参数：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| id | Int64 | 是 | 文件数据库ID（路径参数） |
| targetDirectory | String | 是 | 目标目录路径（表单参数，如：Images/Products 或 Documents/2024） |
| category | String | 否 | 新的分类标签（表单参数，如：Image、Document等） |

**功能说明：**
- **目录移动**：将文件从当前目录移动到指定的目标目录
- **保持文件名**：移动后保持存储文件名（`Name`）和原始文件名（`OriginalName`）不变
- **分类更新**：可选更新文件的分类标签（`Category`）
- **路径安全**：自动清理路径中的特殊字符，防止路径穿越攻击
- **重名检查**：检测目标位置是否已存在同名文件
- **空目录清理**：移动后如果源目录为空，自动删除
- **异常回滚**：如果数据库更新失败，自动回滚物理文件移动操作
- **权限控制**：仅能移动本项目的文件

**请求示例：**

```bash
curl -X PATCH "https://your-api.com/api/v1/io/12345/move" \
  -H "X-Project-Code: ProjectA" \
  -H "X-Timestamp: 1738339200" \
  -H "X-Signature: a1b2c3..." \
  -H "X-External-UserId: user_001" \
  -F "targetDirectory=Images/Products/2024" \
  -F "category=ProductImage"
```

**响应示例：**

```json
{
  "code": 200,
  "message": "文件移动成功",
  "data": {
    "id": 12345,
    "name": "20251224093045_product_a1b2c3d4.jpg",
    "originalName": "产品图片.jpg",
    "oldRelativePath": "Document/2025/12/24/20251224093045_product_a1b2c3d4.jpg",
    "newRelativePath": "Images/Products/2024/20251224093045_product_a1b2c3d4.jpg",
    "oldCategory": "Document",
    "newCategory": "ProductImage",
    "moved": true
  }
}
```

**错误码：**

| 错误码 | 说明 |
|--------|------|
| 10000 | 参数错误（文件ID无效、目标目录为空、目标目录格式无效、缺少必填请求头等） |
| 10001 | 文件记录不存在 |
| 10003 | 文件所属项目不存在 |
| 10004 | 无权移动其他项目的文件 |
| 10006 | 物理文件不存在，无法移动 |
| 10007 | 目标位置已存在同名文件 |
| 50000 | 服务器内部错误 |

### 4.5 删除文件

**接口地址：** `DELETE /api/v1/io/{filename}`

**请求方法：** DELETE

**鉴权要求：** 必需

**响应示例：**

```json
1  // 成功删除1个文件
```

### 4.5 搜索文件

**接口地址：** `GET /api/v1/io/search`

**请求参数：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| pattern | String | 否 | 匹配模式（如：2023/*.jpg） |
| start | Int32 | 否 | 起始位置（默认0） |
| count | Int32 | 否 | 返回数量（默认100） |

**响应示例：**

```json
[
  {
    "name": "2023/01/avatar.jpg",
    "time": "2025-01-15T10:30:00"
  },
  {
    "name": "2023/01/document.pdf",
    "time": "2025-01-16T14:20:00"
  }
]
```

---

## 5. 客户端调用示例

### 5.1 C# 客户端（完整示例）

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EasyIO.Client
{
    public class EasyIOClient
    {
        private readonly string _baseUrl;
        private readonly string _projectCode;
        private readonly string _apiSecret;
        private readonly HttpClient _httpClient;

        public EasyIOClient(string baseUrl, string projectCode, string apiSecret)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _projectCode = projectCode;
            _apiSecret = apiSecret;
            _httpClient = new HttpClient();
        }

        /// <summary>上传文件</summary>
        public async Task<string> UploadFileAsync(string localFilePath, string remotePath, 
            string category = null, bool isPublic = false)
        {
            // 1. 读取文件
            var fileBytes = await File.ReadAllBytesAsync(localFilePath);
            
            // 2. 构建URL
            var path = $"/api/v1/io/{remotePath}";
            var queryString = $"category={category}&isPublic={isPublic}";
            var url = $"{_baseUrl}{path}?{queryString}";
            
            // 3. 生成签名
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signature = GenerateSignature("PUT", path, queryString, fileBytes, timestamp);
            
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
                throw new Exception($"上传失败：{result}");
            
            return result;
        }

        /// <summary>下载文件</summary>
        public async Task<byte[]> DownloadFileAsync(string remotePath)
        {
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
                throw new Exception($"下载失败：{response.StatusCode}");
            
            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>生成签名</summary>
        private string GenerateSignature(string method, string path, string query, 
            byte[] body, string timestamp)
        {
            // 构建签名字符串
            var bodyStr = body != null ? Convert.ToBase64String(body) : "";
            var signString = $"{method}\n{path}\n{SortQuery(query)}\n{bodyStr}\n{timestamp}";
            
            // 计算HMAC-SHA256
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signString));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>排序查询参数</summary>
        private string SortQuery(string query)
        {
            if (string.IsNullOrEmpty(query)) return "";
            
            var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
            Array.Sort(pairs);
            return string.Join("&", pairs);
        }
    }
}
```

**使用示例：**

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        // 初始化客户端
        var client = new EasyIOClient(
            baseUrl: "http://localhost:5000",
            projectCode: "myapp",
            apiSecret: "your-secret-key-here-32-chars"
        );

        // 上传文件
        try
        {
            var result = await client.UploadFileAsync(
                localFilePath: @"C:\temp\avatar.jpg",
                remotePath: "users/avatar.jpg",
                category: "avatar",
                isPublic: false
            );
            
            Console.WriteLine($"上传成功：{result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"上传失败：{ex.Message}");
        }

        // 下载文件
        try
        {
            var bytes = await client.DownloadFileAsync("users/avatar.jpg");
            await File.WriteAllBytesAsync(@"C:\temp\downloaded.jpg", bytes);
            Console.WriteLine("下载成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"下载失败：{ex.Message}");
        }
    }
}
```

### 5.2 Python 客户端

```python
import hmac
import hashlib
import time
import requests
from urllib.parse import urlencode

class EasyIOClient:
    def __init__(self, base_url, project_code, api_secret):
        self.base_url = base_url.rstrip('/')
        self.project_code = project_code
        self.api_secret = api_secret

    def upload_file(self, local_path, remote_path, category=None, is_public=False):
        """上传文件"""
        # 读取文件
        with open(local_path, 'rb') as f:
            file_data = f.read()
        
        # 构建URL
        path = f'/api/v1/io/{remote_path}'
        query = f'category={category}&isPublic={is_public}'
        url = f'{self.base_url}{path}?{query}'
        
        # 生成签名
        timestamp = str(int(time.time()))
        signature = self._generate_signature('PUT', path, query, file_data, timestamp)
        
        # 发送请求
        headers = {
            'X-Project-Code': self.project_code,
            'X-Timestamp': timestamp,
            'X-Signature': signature,
            'Content-Type': 'application/octet-stream'
        }
        
        response = requests.put(url, data=file_data, headers=headers)
        
        if response.status_code != 200:
            raise Exception(f'上传失败：{response.text}')
        
        return response.json()

    def download_file(self, remote_path, save_path):
        """下载文件"""
        path = f'/api/v1/io/{remote_path}'
        url = f'{self.base_url}{path}'
        
        timestamp = str(int(time.time()))
        signature = self._generate_signature('GET', path, '', None, timestamp)
        
        headers = {
            'X-Project-Code': self.project_code,
            'X-Timestamp': timestamp,
            'X-Signature': signature
        }
        
        response = requests.get(url, headers=headers)
        
        if response.status_code != 200:
            raise Exception(f'下载失败：{response.status_code}')
        
        with open(save_path, 'wb') as f:
            f.write(response.content)

    def _generate_signature(self, method, path, query, body, timestamp):
        """生成签名"""
        import base64
        
        # 构建签名字符串
        body_str = base64.b64encode(body).decode() if body else ''
        sign_string = f'{method}\n{path}\n{self._sort_query(query)}\n{body_str}\n{timestamp}'
        
        # 计算HMAC-SHA256
        signature = hmac.new(
            self.api_secret.encode(),
            sign_string.encode(),
            hashlib.sha256
        ).hexdigest()
        
        return signature

    def _sort_query(self, query):
        """排序查询参数"""
        if not query:
            return ''
        pairs = query.split('&')
        return '&'.join(sorted(pairs))

# 使用示例
if __name__ == '__main__':
    client = EasyIOClient(
        base_url='http://localhost:5000',
        project_code='myapp',
        api_secret='your-secret-key-here-32-chars'
    )
    
    # 上传
    result = client.upload_file(
        local_path='avatar.jpg',
        remote_path='users/avatar.jpg',
        category='avatar',
        is_public=False
    )
    print('上传成功：', result)
    
    # 下载
    client.download_file('users/avatar.jpg', 'downloaded.jpg')
    print('下载成功')
```

### 5.3 JavaScript/Node.js 客户端

```javascript
const crypto = require('crypto');
const fs = require('fs');
const axios = require('axios');

class EasyIOClient {
    constructor(baseUrl, projectCode, apiSecret) {
        this.baseUrl = baseUrl.replace(/\/$/, '');
        this.projectCode = projectCode;
        this.apiSecret = apiSecret;
    }

    async uploadFile(localPath, remotePath, category = null, isPublic = false) {
        // 读取文件
        const fileData = fs.readFileSync(localPath);
        
        // 构建URL
        const path = `/api/v1/io/${remotePath}`;
        const query = `category=${category}&isPublic=${isPublic}`;
        const url = `${this.baseUrl}${path}?${query}`;
        
        // 生成签名
        const timestamp = Math.floor(Date.now() / 1000).toString();
        const signature = this.generateSignature('PUT', path, query, fileData, timestamp);
        
        // 发送请求
        const response = await axios.put(url, fileData, {
            headers: {
                'X-Project-Code': this.projectCode,
                'X-Timestamp': timestamp,
                'X-Signature': signature,
                'Content-Type': 'application/octet-stream'
            }
        });
        
        return response.data;
    }

    async downloadFile(remotePath, savePath) {
        const path = `/api/v1/io/${remotePath}`;
        const url = `${this.baseUrl}${path}`;
        
        const timestamp = Math.floor(Date.now() / 1000).toString();
        const signature = this.generateSignature('GET', path, '', null, timestamp);
        
        const response = await axios.get(url, {
            headers: {
                'X-Project-Code': this.projectCode,
                'X-Timestamp': timestamp,
                'X-Signature': signature
            },
            responseType: 'arraybuffer'
        });
        
        fs.writeFileSync(savePath, response.data);
    }

    generateSignature(method, path, query, body, timestamp) {
        // 构建签名字符串
        const bodyStr = body ? Buffer.from(body).toString('base64') : '';
        const signString = `${method}\n${path}\n${this.sortQuery(query)}\n${bodyStr}\n${timestamp}`;
        
        // 计算HMAC-SHA256
        const hmac = crypto.createHmac('sha256', this.apiSecret);
        hmac.update(signString);
        return hmac.digest('hex');
    }

    sortQuery(query) {
        if (!query) return '';
        const pairs = query.split('&');
        return pairs.sort().join('&');
    }
}

// 使用示例
(async () => {
    const client = new EasyIOClient(
        'http://localhost:5000',
        'myapp',
        'your-secret-key-here-32-chars'
    );
    
    try {
        // 上传
        const result = await client.uploadFile(
            'avatar.jpg',
            'users/avatar.jpg',
            'avatar',
            false
        );
        console.log('上传成功：', result);
        
        // 下载
        await client.downloadFile('users/avatar.jpg', 'downloaded.jpg');
        console.log('下载成功');
    } catch (error) {
        console.error('错误：', error.message);
    }
})();
```

### 5.4 cURL 命令示例

```bash
#!/bin/bash

# 配置
PROJECT_CODE="myapp"
API_SECRET="your-secret-key-here-32-chars"
BASE_URL="http://localhost:5000"
FILE_PATH="test.jpg"
REMOTE_PATH="uploads/test.jpg"

# 生成时间戳
TIMESTAMP=$(date +%s)

# 读取文件并转Base64
FILE_BASE64=$(base64 < "$FILE_PATH")

# 构建签名字符串
METHOD="PUT"
PATH="/api/v1/io/$REMOTE_PATH"
QUERY="category=test&isPublic=false"
SIGN_STRING="$METHOD\n$PATH\n$QUERY\n$FILE_BASE64\n$TIMESTAMP"

# 计算签名
SIGNATURE=$(echo -n "$SIGN_STRING" | openssl dgst -sha256 -hmac "$API_SECRET" | awk '{print $2}')

# 发送请求
curl -X PUT \
  -H "X-Project-Code: $PROJECT_CODE" \
  -H "X-Timestamp: $TIMESTAMP" \
  -H "X-Signature: $SIGNATURE" \
  -H "Content-Type: application/octet-stream" \
  --data-binary "@$FILE_PATH" \
  "$BASE_URL$PATH?$QUERY"
```

---

## 6. 错误码说明

| 错误码 | 说明 | 解决方案 |
|--------|------|----------|
| 400 | 参数错误 | 检查请求参数是否完整 |
| 401 | 缺少鉴权信息 | 检查是否传递了 projectCode、timestamp、signature |
| 403 | 鉴权失败 | 检查项目编码、密钥、签名算法是否正确 |
| 404 | 文件不存在 | 检查文件路径是否正确 |
| 410 | 文件已过期 | 文件超过有效期 |
| 413 | 文件过大 | 文件超过项目配置的最大限制 |
| 429 | 请求过于频繁 | 触发限流，稍后重试 |
| 500 | 服务器错误 | 联系管理员 |

### 详细错误信息

**鉴权失败示例：**

```json
{
  "code": 403,
  "message": "鉴权失败：签名验证失败"
}
```

**时间戳过期：**

```json
{
  "code": 403,
  "message": "鉴权失败：请求已过期（时间差：350秒）"
}
```

**限流触发：**

```json
{
  "code": 429,
  "message": "请求过于频繁，请稍后再试"
}
```

---

## 7. 最佳实践

### 7.1 安全建议

#### ✅ 密钥管理
- 使用强随机字符串（至少32字符）
- 定期轮换密钥
- 不要在代码中硬编码，使用环境变量或配置中心
- 不同环境使用不同密钥（开发/测试/生产）

```csharp
// ❌ 错误：硬编码
var apiSecret = "my-secret-123";

// ✅ 正确：从配置读取
var apiSecret = Environment.GetEnvironmentVariable("EASYIO_API_SECRET");
```

#### ✅ 时间同步
- 确保客户端和服务端时间同步
- 使用 NTP 协议校准时间
- 如遇签名失败，先检查时间差

```bash
# Linux 时间同步
sudo ntpdate time.windows.com
```

#### ✅ HTTPS 传输
- 生产环境必须使用 HTTPS
- 防止中间人攻击窃取签名

### 7.2 性能优化

#### ✅ 文件分片上传（大文件）
```csharp
// 对于超大文件，建议分片上传
public async Task UploadLargeFileAsync(string filePath, int chunkSize = 5 * 1024 * 1024)
{
    var fileInfo = new FileInfo(filePath);
    var totalChunks = (int)Math.Ceiling((double)fileInfo.Length / chunkSize);
    
    for (int i = 0; i < totalChunks; i++)
    {
        var chunk = ReadChunk(filePath, i * chunkSize, chunkSize);
        await UploadChunkAsync($"{filePath}.part{i}", chunk);
    }
    
    // 合并分片（需要服务端支持）
    await MergeChunksAsync(filePath, totalChunks);
}
```

#### ✅ 并发控制
```csharp
// 使用 SemaphoreSlim 控制并发
private SemaphoreSlim _semaphore = new SemaphoreSlim(5); // 最多5个并发

public async Task UploadMultipleFilesAsync(string[] files)
{
    var tasks = files.Select(async file =>
    {
        await _semaphore.WaitAsync();
        try
        {
            await UploadFileAsync(file);
        }
        finally
        {
            _semaphore.Release();
        }
    });
    
    await Task.WhenAll(tasks);
}
```

#### ✅ 重试机制
```csharp
public async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await action();
        }
        catch (HttpRequestException) when (i < maxRetries - 1)
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // 指数退避
        }
    }
    throw new Exception("重试次数已达上限");
}
```

### 7.3 日志监控

```csharp
public class EasyIOClient
{
    private readonly ILogger _logger;
    
    public async Task<string> UploadFileAsync(string localPath, string remotePath)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("开始上传：{File} -> {Remote}", localPath, remotePath);
            
            var result = await DoUploadAsync(localPath, remotePath);
            
            sw.Stop();
            _logger.LogInformation("上传成功，耗时：{Elapsed}ms", sw.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "上传失败：{File}，耗时：{Elapsed}ms", localPath, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
```

### 7.4 文件命名规范

```csharp
// ✅ 推荐的命名方式
var remotePath = $"{category}/{userId}/{DateTime.Now:yyyyMMdd}/{Guid.NewGuid():N}.jpg";
// 结果：avatar/12345/20251220/a1b2c3d4e5f6.jpg

// ❌ 避免的命名方式
var remotePath = "用户头像.jpg";  // 中文
var remotePath = "../../etc/passwd";  // 路径穿越
```

---

## 8. 常见问题

### Q1: 签名验证总是失败？

**检查清单：**
1. 项目编码是否正确
2. API密钥是否正确
3. 时间戳是否为Unix秒级时间戳
4. 签名字符串是否按规范构建（注意换行符）
5. 查询参数是否已排序
6. Body是否正确处理（Base64编码）

**调试方法：**
```csharp
// 打印签名字符串
Console.WriteLine($"SignString: {signString}");
Console.WriteLine($"Signature: {signature}");
```

### Q2: 时间戳过期错误？

- 检查客户端和服务端时间差
- 默认过期时间为300秒，可配置
- 考虑网络延迟，建议预留30秒缓冲

### Q3: 如何实现断点续传？

目前暂不支持断点续传，建议使用分片上传方式。

### Q4: 是否支持 CDN 加速？

文件下载接口可配合 CDN：
1. 上传后获取文件ID
2. 通过 `/api/v1/io/GetUrl/{id}` 获取CDN链接

---

## 9. 更新日志

### v1.0.0 (2025-12-20)
- ✅ 初始版本发布
- ✅ 多项目鉴权支持
- ✅ HMAC签名验证
- ✅ 文件上传/下载/删除
- ✅ 文件去重
- ✅ 限流保护

---

## 10. 技术支持

- **文档地址：** https://github.com/your-org/DH.NEasyIO
- **问题反馈：** https://github.com/your-org/DH.NEasyIO/issues
- **邮箱支持：** support@example.com

---

**版权所有 © 2025 PeiKeSmart**
