# PDF to OFD - Java 转换服务

基于 Spring Boot 3 的 PDF 转 OFD 转换服务，使用 ofdrw 开源库。

## 技术栈

- Java 17
- Spring Boot 3.2
- ofdrw 2.2.5（开源 OFD 处理库）
- Apache PDFBox

## 项目结构

```
backend-java/
├── Dockerfile
├── pom.xml
└── src/
    ├── main/java/com/converter/
    │   ├── OfdConverterApplication.java  # 入口
    │   ├── controller/
    │   │   └── ConvertController.java    # 转换接口
    │   ├── service/
    │   │   └── OfdConvertService.java    # 转换服务
    │   ├── dto/
    │   │   ├── ConvertRequest.java
    │   │   └── ConvertResponse.java
    │   └── config/
    │       └── GlobalExceptionHandler.java
    └── test/java/com/converter/          # 单元测试
```

## API 接口

| 方法 | 路径 | 描述 |
|------|------|------|
| POST | `/api/convert` | 执行 PDF 转 OFD |
| GET | `/api/health` | 健康检查 |

### 转换请求

```json
POST /api/convert
{
  "pdfPath": "/data/pdf/1_test.pdf",
  "ofdPath": "/data/ofd/1_test.ofd"
}
```

### 转换响应

```json
{
  "success": true,
  "ofdPath": "/data/ofd/1_test.ofd",
  "pageCount": 5,
  "errorMessage": null
}
```

## 本地开发

```bash
# 编译
mvn clean package

# 运行
java -jar target/ofd-converter-1.0.0.jar

# 运行测试
mvn test
```

## Docker 构建

```bash
docker build -t pdf-to-ofd-java .
```

## 关于 ofdrw

[ofdrw](https://github.com/ofdrw/ofdrw) 是目前最成熟的开源 OFD 处理库，支持：
- OFD 文档生成
- PDF 转 OFD
- OFD 转 PDF/图片
- 数字签名
- 文档合并

遵循国家标准 GB/T 33190-2016。
