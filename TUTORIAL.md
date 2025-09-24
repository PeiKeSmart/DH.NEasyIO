# DH.NEasyIO 使用教程

## 项目简介

**DH.NEasyIO** 是一个轻量级的分布式文件存储系统，专为分布式系统架构中的集中文件存储而设计。该系统提供了简单易用的API接口，支持文件的上传、下载、删除和搜索功能。

### 核心特性

- 🚀 **轻量级设计**：仅提供核心的文件存储功能
- 📦 **简单易用**：SDK只有一个 `EasyClient` 类
- 🌐 **RESTful API**：提供标准的HTTP API接口
- 🔄 **分布式支持**：支持多服务器负载均衡
- 🔍 **搜索功能**：支持按模式搜索文件
- ⚡ **高性能**：基于.NET 6+ 构建，支持高并发

### 主要组件

1. **DH.NEasyIO** - SDK类库，提供客户端API
2. **EasyWeb** - Web服务端，提供HTTP API接口
3. **Test** - 测试项目
4. **XUnitTest** - 单元测试项目

## 快速开始

### 环境要求

- .NET 6.0 或更高版本
- 支持 Windows、Linux、macOS

### 安装SDK

#### 方法一：NuGet包安装（推荐）

```bash
dotnet add package DH.NEasyIO
```

#### 方法二：源码编译

```bash
# 克隆项目
git clone https://github.com/PeiKeSmart/DH.NEasyIO.git

# 进入项目目录
cd DH.NEasyIO

# 编译项目
dotnet build DH.NEasyIO/DH.NEasyIO.csproj

# 打包
dotnet pack DH.NEasyIO/DH.NEasyIO.csproj -c Release -o ./nupkg
```

### 启动服务端

#### 方法一：使用EasyWeb项目

```bash
# 进入EasyWeb目录
cd EasyWeb

# 运行项目
dotnet run
```

服务将在 `http://localhost:6800` 启动。

#### 方法二：在现有ASP.NET Core项目中集成

1. 引用DH.NEasyIO包：

```bash
dotnet add package DH.NEasyIO
```

2. 在 `Program.cs` 中添加服务：

```csharp
using NewLife.EasyIO.Options;

var builder = WebApplication.CreateBuilder(args);

// 添加EasyIO服务
builder.Services.AddEasyIO();

var app = builder.Build();

// 使用EasyIO中间件
app.UseEasyIO(true);

app.Run();
```

3. 在 `appsettings.json` 中配置存储路径：

```json
{
  "easyio": {
    "path": "../files"
  }
}
```

## API使用指南

### 1. SDK客户端使用

#### 初始化客户端

```csharp
using NewLife.EasyIO;

// 创建客户端实例
var client = new EasyClient
{
    Server = "http://localhost:6800",
    AppId = "your-app-id",     // 可选
    AppSecret = "your-secret"  // 可选
};
```

#### 上传文件

```csharp
// 上传文件
var fileId = "documents/example.txt";
var fileData = File.ReadAllBytes("example.txt");

var result = await client.Put(fileId, new Packet(fileData));
Console.WriteLine($"上传成功: {result}");
```

#### 下载文件

```csharp
// 下载文件
var fileId = "documents/example.txt";
var packet = await client.Get(fileId);

using var stream = new MemoryStream(packet.Data);
using var fileStream = new FileStream("downloaded.txt", FileMode.Create);
await stream.CopyToAsync(fileStream);
```

#### 获取下载链接

```csharp
// 获取文件下载链接
var fileId = "documents/example.txt";
var downloadUrl = await client.GetUrl(fileId);

Console.WriteLine($"下载链接: {downloadUrl}");
```

### 2. HTTP API接口

#### 上传文件

```bash
PUT /io/put?id=documents/example.txt
Content-Type: application/octet-stream

[文件内容]
```

#### 下载文件

```bash
GET /io/get?id=documents/example.txt
```

#### 获取下载链接

```bash
GET /io/geturl?id=documents/example.txt
```

#### 删除文件

```bash
DELETE /io/delete?id=documents/example.txt
```

#### 搜索文件

```bash
GET /io/search?pattern=documents/*.txt&start=0&count=100
```

## 配置文件详解

### EasyFileSetting 配置

在 `EasyFileSetting.cs` 中定义了以下配置项：

```csharp
public class EasyFileSetting
{
    /// <summary>根目录。文件管理根目录</summary>
    public String Root { get; set; } = "../files";

    /// <summary>限流周期。在指定限流周期内，流量超限时返回TooManyRequests，默认600秒</summary>
    public Int32 LimitCycle { get; set; } = 600;

    /// <summary>IP限流流量。在指定限流周期内，流量超限时返回TooManyRequests，默认200M</summary>
    public Int32 FlowLimitByIP { get; set; } = 200;
}
```

### FileStorageOptions 配置

```csharp
public class FileStorageOptions
{
    /// <summary>路径</summary>
    public String Path { get; set; }
}
```

## 高级配置

### 1. 数据库配置

在 `appsettings.json` 中配置数据库连接：

```json
{
  "ConnectionStrings": {
    "EasyFile": "Data Source=..\\Data\\EasyFile.db;Provider=SQLite",
    "Membership": "Data Source=..\\Data\\Membership.db;Provider=SQLite"
  }
}
```

### 2. 服务器配置

```json
{
  "Urls": "http://*:6800",
  "AllowedHosts": "*"
}
```

### 3. 日志配置

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

## 部署指南

### 1. 生产环境部署

#### Windows Server (IIS)

1. 发布项目：
```bash
dotnet publish EasyWeb/EasyWeb.csproj -c Release -o ./publish
```

2. 在IIS中创建网站，指向发布目录

3. 配置应用程序池使用无托管代码

#### Linux (Nginx + Kestrel)

1. 发布项目：
```bash
dotnet publish EasyWeb/EasyWeb.csproj -c Release -o ./publish
```

2. 创建systemd服务文件：
```ini
[Unit]
Description=DH.NEasyIO Service

[Service]
WorkingDirectory=/path/to/publish
ExecStart=/usr/bin/dotnet /path/to/publish/EasyWeb.dll
Restart=always
RestartSec=10
SyslogIdentifier=dh-neasyio
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

3. 配置Nginx反向代理：
```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:6800;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 2. Docker部署

创建 `Dockerfile`：

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
EXPOSE 6800
ENTRYPOINT ["dotnet", "EasyWeb.dll"]
```

构建和运行：

```bash
docker build -t dh-neasyio .
docker run -d -p 6800:6800 -v /host/files:/app/files dh-neasyio
```

## 监控和维护

### 1. 健康检查

服务提供健康检查端点：
```bash
GET /health
```

### 2. 日志管理

系统使用NewLife.Log进行日志记录，支持：
- 控制台输出
- 文件输出
- 数据库输出

### 3. 性能监控

- 使用性能计数器监控CPU和内存使用率
- 监控文件存储使用情况
- 设置流量限制和告警

## 故障排除

### 常见问题

#### 1. 文件上传失败

**问题**：上传文件时出现异常
**解决**：
- 检查存储目录权限
- 确认磁盘空间充足
- 查看日志文件获取详细错误信息

#### 2. 服务启动失败

**问题**：服务无法正常启动
**解决**：
- 检查配置文件格式
- 确认依赖项已正确安装
- 查看控制台错误输出

#### 3. 网络连接问题

**问题**：客户端无法连接到服务端
**解决**：
- 检查防火墙设置
- 确认服务端口未被占用
- 验证网络配置

### 调试模式

启用详细日志：

```csharp
XTrace.UseConsole();
XTrace.Log.Level = LogLevel.Debug;
```

## 最佳实践

### 1. 文件组织

- 使用有意义的文件路径结构
- 避免过深的文件目录层次
- 定期清理无用文件

### 2. 性能优化

- 设置合理的文件大小限制
- 启用文件压缩
- 使用CDN加速文件分发

### 3. 安全建议

- 配置访问控制
- 启用HTTPS
- 定期备份重要文件
- 设置文件访问审计

## 技术支持

- **GitHub**: https://github.com/PeiKeSmart/DH.NEasyIO
- **官网**: https://yuanrenyi.com
- **公司**: 湖北登灏科技有限公司

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 版本历史

### v4.13 (2025-01-24)
- 支持.NET 6.0-10.0
- 优化文件存储性能
- 增强错误处理机制

---

**注意**：本教程基于项目源码分析，如有疑问请参考最新文档或联系技术支持。
