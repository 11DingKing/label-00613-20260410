# PDF 转 OFD 转换系统 - 项目设计文档

## 1. 系统架构

```mermaid
flowchart TD
    subgraph Client["客户端"]
        A[HTTP 请求] --> B[上传 PDF]
        C[下载 OFD]
        D[查询状态/历史]
    end

    subgraph CSharpAPI["C# Web API (.NET 8) - 端口 5000"]
        E[FileController] --> F[ConversionService]
        F --> G[JavaClientService]
        H[HistoryController] --> I[HistoryService]
        J[HealthController]
    end

    subgraph JavaService["Java 转换服务 (Spring Boot 3) - 端口 8080"]
        K[ConvertController] --> L[OfdConvertService]
        L --> M[ofdrw 库]
    end

    subgraph Storage["存储"]
        N[(MySQL 8.0)]
        O[文件存储 /data]
    end

    Client -->|HTTP| CSharpAPI
    CSharpAPI -->|HTTP| JavaService
    CSharpAPI --> N
    CSharpAPI --> O
    JavaService --> O
```

## 2. ER 图

```mermaid
erDiagram
    CONVERSION_RECORD {
        bigint id PK "主键"
        varchar file_name "原始文件名"
        varchar pdf_path "PDF 存储路径"
        varchar ofd_path "OFD 存储路径"
        int status "状态: 0-待处理 1-处理中 2-成功 3-失败"
        varchar error_message "错误信息"
        bigint file_size "文件大小(bytes)"
        int page_count "页数"
        datetime created_at "创建时间"
        datetime updated_at "更新时间"
    }

    OPERATION_LOG {
        bigint id PK "主键"
        varchar operation "操作类型"
        varchar target_id "目标ID"
        varchar ip_address "IP地址"
        varchar user_agent "用户代理"
        text request_body "请求体"
        int response_code "响应码"
        datetime created_at "创建时间"
    }
```

## 3. 接口清单

### 3.1 C# Web API (端口 5000)

#### FileController
| 方法 | 路径 | 描述 |
|------|------|------|
| POST | `/api/file/upload` | 上传 PDF 文件并转换 |
| GET | `/api/file/download/{id}` | 下载 OFD 文件 |
| GET | `/api/file/status/{id}` | 查询转换状态 |

#### HistoryController
| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/api/history` | 获取转换历史列表 |
| DELETE | `/api/history/{id}` | 删除历史记录 |

#### HealthController
| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/api/health` | 健康检查 |

### 3.2 Java 转换服务 (端口 8080，内部服务)

#### ConvertController
| 方法 | 路径 | 描述 |
|------|------|------|
| POST | `/api/convert` | 执行 PDF 转 OFD |
| GET | `/api/health` | 健康检查 |

## 4. 技术栈

| 层级 | 技术 | 版本 |
|------|------|------|
| C# API | .NET 8 + ASP.NET Core + EF Core | .NET 8.0 |
| Java 服务 | Spring Boot 3 + ofdrw | Spring Boot 3.2+ |
| 数据库 | MySQL | 8.0 |
| 容器化 | Docker + Docker Compose | Latest |

## 5. 目录结构

```
label-00613/
├── docker-compose.yml
├── .gitignore
├── README.md
├── docs/
│   └── project_design.md
├── backend-csharp/
│   ├── Dockerfile
│   └── PdfToOfd.Api/
│       ├── Controllers/
│       ├── Services/
│       ├── Models/
│       ├── Data/
│       └── ...
└── backend-java/
    ├── Dockerfile
    ├── pom.xml
    └── src/
```
