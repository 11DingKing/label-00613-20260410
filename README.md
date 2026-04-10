# PDF 转 OFD 转换系统

基于 C# + Java 的 PDF 转 OFD（国标版式文档）转换服务。

## 快速开始

```bash
# 启动所有服务
docker-compose up --build -d

# 查看日志
docker-compose logs -f

# 停止服务
docker-compose down
```

## 服务架构

| 服务 | 端口 | 描述 |
|------|------|------|
| backend-csharp | 8081 | C# Web API，对外提供接口 |
| backend-java | 8080 (内部) | Java 转换服务，使用 ofdrw 开源库 |
| mysql | 3306 | MySQL 数据库 |

- API 地址：http://localhost:8081
- Swagger 文档：http://localhost:8081/swagger

## API 接口

### 文件操作

| 方法 | 路径 | 描述 |
|------|------|------|
| POST | `/api/file/upload` | 上传 PDF 并转换为 OFD |
| GET | `/api/file/status/{id}` | 查询转换状态 |
| GET | `/api/file/download/{id}` | 下载 OFD 文件 |

### 历史记录

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/api/history` | 获取转换历史列表 |
| DELETE | `/api/history/{id}` | 删除历史记录 |

### 健康检查

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/api/health` | 服务健康检查 |

## 使用示例

```bash
# 健康检查
curl http://localhost:8081/api/health

# 上传 PDF 并转换
curl -X POST -F "file=@test.pdf" http://localhost:8081/api/file/upload

# 查询转换状态
curl http://localhost:8081/api/file/status/1

# 下载 OFD 文件
curl -O http://localhost:8081/api/file/download/1

# 查询历史记录（支持分页）
curl "http://localhost:8081/api/history?page=1&pageSize=10"

# 删除记录
curl -X DELETE http://localhost:8081/api/history/1
```

## 运行测试

```bash
cd tests
./run-tests.sh
```

测试覆盖：
- 健康检查（C# API + Java 服务）
- 文件上传（无文件、非 PDF、有效 PDF）
- 状态查询
- 历史记录分页
- 文件下载
- 记录删除

## 技术栈

| 组件 | 技术 | 版本 |
|------|------|------|
| Web API | .NET 8 + ASP.NET Core | 8.0 |
| 转换引擎 | Java + ofdrw | Spring Boot 3.2 |
| 数据库 | MySQL | 8.0 |
| 容器化 | Docker + Docker Compose | Latest |

## 项目结构

```
label-00613/
├── docker-compose.yml          # Docker 编排配置
├── README.md
├── docs/
│   └── project_design.md       # 项目设计文档
├── sql/
│   ├── schema.sql              # 数据库初始化脚本
│   └── README.md               # 数据库说明
├── backend-csharp/             # C# Web API
│   ├── Dockerfile
│   ├── README.md
│   ├── PdfToOfd.Api/           # 主项目
│   └── PdfToOfd.Api.Tests/     # 单元测试
├── backend-java/               # Java 转换服务
│   ├── Dockerfile
│   ├── README.md
│   ├── pom.xml
│   └── src/
└── tests/                      # 集成测试
    ├── README.md
    ├── api-test.sh
    ├── docker-compose.test.yml
    └── run-tests.sh
```

## 关于 OFD

OFD（Open Fixed-layout Document）是由工业和信息化部软件司牵头中国电子技术标准化研究院成立的版式编写组制定的版式文档国家标准，属于中国自主格式，于2016年10月13日发布为国家标准（GB/T 33190-2016）。
