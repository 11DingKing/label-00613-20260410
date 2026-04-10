# 数据库脚本

PDF to OFD 转换系统的数据库初始化脚本。

## 文件说明

| 文件 | 描述 |
|------|------|
| `schema.sql` | 数据库表结构初始化脚本 |

## 表结构

### conversion_record - 转换记录表

| 字段 | 类型 | 描述 |
|------|------|------|
| id | BIGINT | 主键，自增 |
| file_name | VARCHAR(255) | 原始文件名 |
| pdf_path | VARCHAR(500) | PDF 存储路径 |
| ofd_path | VARCHAR(500) | OFD 存储路径 |
| status | INT | 状态：0-待处理 1-处理中 2-成功 3-失败 |
| error_message | VARCHAR(1000) | 错误信息 |
| file_size | BIGINT | 文件大小（字节） |
| page_count | INT | PDF 页数 |
| created_at | DATETIME | 创建时间 |
| updated_at | DATETIME | 更新时间 |

### operation_log - 操作日志表

| 字段 | 类型 | 描述 |
|------|------|------|
| id | BIGINT | 主键，自增 |
| operation | VARCHAR(50) | 操作类型 |
| target_id | VARCHAR(50) | 目标 ID |
| ip_address | VARCHAR(50) | IP 地址 |
| user_agent | VARCHAR(500) | 用户代理 |
| request_body | TEXT | 请求体 |
| response_code | INT | 响应码 |
| created_at | DATETIME | 创建时间 |

## 手动执行

```bash
# 连接 MySQL
mysql -u root -p

# 执行脚本
source /path/to/schema.sql
```

## Docker 自动初始化

在 Docker Compose 中，`schema.sql` 会自动挂载到 MySQL 容器的初始化目录：

```yaml
volumes:
  - ./sql/schema.sql:/docker-entrypoint-initdb.d/schema.sql
```

MySQL 容器首次启动时会自动执行该脚本。
