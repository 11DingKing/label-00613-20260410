# PDF to OFD - 测试套件

API 集成测试和 Docker 测试环境。

## 文件结构

```
tests/
├── api-test.sh             # API 集成测试脚本
├── docker-compose.test.yml # 测试环境 Docker 配置
├── run-tests.sh            # 一键运行测试
└── test-files/
    ├── sample.pdf          # 测试用 PDF 文件
    └── create-sample-pdf.sh
```

## 运行测试

### 一键运行

```bash
./run-tests.sh
```

### 手动运行

```bash
# 启动测试环境
docker compose -f docker-compose.test.yml up -d

# 等待服务就绪
sleep 30

# 运行测试
BASE_URL="http://localhost:8081" ./api-test.sh

# 清理
docker compose -f docker-compose.test.yml down -v
```

## 测试用例

| 测试项 | 描述 |
|--------|------|
| 健康检查 | C# API 和 Java 服务状态 |
| 无文件上传 | 验证返回 400 |
| 非 PDF 上传 | 验证文件类型校验 |
| PDF 上传转换 | 完整转换流程 |
| 状态查询 | 查询存在/不存在的记录 |
| 历史记录 | 列表和分页功能 |
| 文件下载 | 下载转换后的 OFD |
| 记录删除 | 删除历史记录 |

## 测试结果示例

```
==========================================
  PDF to OFD API 集成测试
==========================================

--- 1. 健康检查测试 ---
✓ PASS: C# API 健康检查返回 UP
✓ PASS: Java 转换服务健康检查返回 UP

--- 2. 文件上传测试 ---
✓ PASS: 无文件上传返回 400
✓ PASS: 非 PDF 文件上传返回正确错误信息
✓ PASS: PDF 文件上传成功，记录 ID: 1

--- 3. 状态查询测试 ---
✓ PASS: 查询不存在记录返回 404
✓ PASS: 查询记录状态成功

--- 4. 历史记录测试 ---
✓ PASS: 获取历史记录列表成功
✓ PASS: 历史记录分页参数正确

--- 5. 下载测试 ---
✓ PASS: 下载不存在文件返回 404
✓ PASS: 下载 OFD 文件成功

--- 6. 删除测试 ---
✓ PASS: 删除不存在记录返回 404

==========================================
  测试结果汇总
==========================================
  通过: 12
  失败: 0
==========================================
```

## 环境要求

- Docker
- Docker Compose
- curl
- bash
