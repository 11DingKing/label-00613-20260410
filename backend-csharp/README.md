# PDF to OFD - C# Web API

基于 .NET 8 的 RESTful API 服务，提供 PDF 转 OFD 的 Web 接口。

## 技术栈

- .NET 8 + ASP.NET Core
- Entity Framework Core + MySQL
- Serilog 日志
- Swagger API 文档

## 项目结构

```
backend-csharp/
├── Dockerfile
├── PdfToOfd.Api/
│   ├── Controllers/        # API 控制器
│   │   ├── FileController.cs      # 文件上传/下载/状态
│   │   ├── HistoryController.cs   # 历史记录管理
│   │   └── HealthController.cs    # 健康检查
│   ├── Services/           # 业务服务
│   │   ├── IConversionService.cs  # 转换服务
│   │   ├── IHistoryService.cs     # 历史服务
│   │   └── IJavaConverterClient.cs # Java 服务客户端
│   ├── Models/             # 数据模型
│   ├── DTOs/               # 数据传输对象
│   ├── Data/               # 数据库上下文
│   ├── Middleware/         # 中间件
│   └── Program.cs          # 入口文件
└── PdfToOfd.Api.Tests/     # 单元测试
    ├── Controllers/
    └── Services/
```

## API 接口

### 文件操作 `/api/file`

| 方法 | 路径 | 描述 |
|------|------|------|
| POST | `/upload` | 上传 PDF 文件并转换 |
| GET | `/status/{id}` | 查询转换状态 |
| GET | `/download/{id}` | 下载 OFD 文件 |

### 历史记录 `/api/history`

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/` | 获取历史列表（支持分页） |
| DELETE | `/{id}` | 删除记录 |

### 健康检查 `/api/health`

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/` | 服务健康状态 |

## 本地开发

```bash
# 还原依赖
dotnet restore

# 运行
dotnet run --project PdfToOfd.Api

# 运行测试
dotnet test
```

## 环境变量

| 变量 | 描述 | 默认值 |
|------|------|--------|
| `ConnectionStrings__DefaultConnection` | MySQL 连接字符串 | - |
| `JavaService__BaseUrl` | Java 转换服务地址 | `http://backend-java:8080` |
| `DataPath` | 文件存储路径 | `/data` |

## Docker 构建

```bash
docker build -t pdf-to-ofd-api .
```
