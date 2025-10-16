# EasyIO - 轻量级文件存储

EasyIO简单文件存储，在分布式系统架构中支持集中存储文件。只有几个webapi接口：put/get/geturl/delete/search。sdk只有一个 EasyClient。  

## 项目结构与作用

- DH.NEasyIO/（类库，多目标框架）
  - 定位：核心文件存储能力与HTTP接口实现，提供最小可用的文件服务。
  - 主要内容：
    - `Controllers/IOController.cs`：WebAPI 接口，包含 put/get/geturl/delete/search 等动作，面向文件上传、获取、删除与搜索；`GetUrl` 预留计算直链/签名地址能力。
    - `EasyClient.cs`：轻量 SDK 客户端，封装对上述接口的访问（Server/AppId/Secret），可直接引用或拷贝到业务侧使用。
    - `EasyIOService.cs`：扩展方法 AddEasyIO/UseEasyIO，注册 `FileStorageOptions` 等运行期依赖。
  - 产物：编译输出到 `Bin/`，同时生成 `DH.NEasyIO.xml` 文档，便于引用与打包。

- DH.NEasyWeb/（类库，net7.0）
  - 定位：基于 EasyIO 的 Web 辅助层，提供统一的控制器基类与服务注册包装，便于在 Web 项目中快速集成。
  - 主要内容：
    - `Controllers/ApiControllerBase.cs`：统一返回格式、异常处理、TraceId 等；可承载 Token 校验逻辑。
    - `Controllers/IOController.cs` 与 `EasyIOService.cs`：与核心实现协同，面向 Web 的封装与日志输出。

- EasyWeb/（可运行 Web 应用，net10.0）
  - 定位：文件管理系统示例/参考实现，集成 NewLife.Cube 后台，提供可视化浏览、下载与目录导航。
  - 主要内容：
    - `Program.cs`：应用启动入口，注册 Stardust、Cube、EasyIO 等；配置路由用于目录展示与文件下载。
    - `Controllers/HomeController.cs`：目录展示、下载重定向/直传、限流等逻辑；与 `Services/` 协作构建模型。
  - 产物：发布输出到 `Bin/Web/`，用于实际部署或演示。

- Test/（控制台，net10.0）
  - 定位：最小化本地调试/演示入口，便于验证基础能力或编写临时实验代码。
  - 主要内容：`Program.cs` 带有 `XTrace` 控制台日志。
  - 产物：输出到 `Bin/Test/`。

- XUnitTest/（单元测试，net10.0）
  - 定位：核心逻辑的单测工程，当前包含对 `EntryService` 的版本/标签解析用例示例。
  - 主要内容：`EntryServiceTests.cs`。
  - 产物：测试构建输出到 `Bin/UnitTest/`。

- Doc/（文档与素材）
  - `DH.png` 等包图标/文档资源，会在打包时作为 `PackageIcon` 引入。

- Bin/（统一输出目录）
  - `netX.Y/`：`DH.NEasyIO` 多目标框架的编译物与 XML 文档。
  - `Web/`：`EasyWeb` 的发布/运行目录，含 `appsettings.json` 与静态资源。
  - 其他：`Test/`、`UnitTest/` 等子目录为对应项目的输出位置。

## 典型使用场景

1) 在现有 Web 项目中集成文件存储
   - 引用 `DH.NEasyIO`（或 `DH.NEasyWeb`）项目/包
   - 在 `Program.cs` 中调用 `services.AddEasyIO()` 与 `app.UseEasyIO(...)`
   - 通过 `IOController` 提供的 API 对接业务端；业务端可使用 `EasyClient` SDK 进行上传/下载。

2) 直接运行示例站点
   - 打开并运行 `EasyWeb` 项目，使用内置页面浏览目录、下载文件，验证服务端能力。

如需进一步扩展（鉴权/签名直链/CDN 适配/多存储后端），建议在 `GetUrl`、`ApiControllerBase`、以及 `Services/` 目录下进行增强，实现 Token 校验、URL 生成策略与多源路由等能力。
