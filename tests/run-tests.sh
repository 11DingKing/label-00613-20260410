#!/bin/bash
# 运行完整的 Docker 测试套件

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

echo "=========================================="
echo "  PDF to OFD 测试套件"
echo "=========================================="
echo ""

# 清理旧容器
echo "→ 清理旧测试容器..."
docker compose -f "$SCRIPT_DIR/docker-compose.test.yml" down -v 2>/dev/null || true

# 构建镜像
echo ""
echo "→ 构建 Docker 镜像..."
docker compose -f "$SCRIPT_DIR/docker-compose.test.yml" build

# 启动服务
echo ""
echo "→ 启动测试服务..."
docker compose -f "$SCRIPT_DIR/docker-compose.test.yml" up -d mysql backend-java backend-csharp

# 等待服务就绪
echo ""
echo "→ 等待服务就绪..."
sleep 5

max_attempts=60
attempt=1
while [ $attempt -le $max_attempts ]; do
    if curl -s -f "http://localhost:8081/api/health" > /dev/null 2>&1; then
        echo "✓ 服务已就绪"
        break
    fi
    echo "  等待中... ($attempt/$max_attempts)"
    sleep 2
    ((attempt++))
done

if [ $attempt -gt $max_attempts ]; then
    echo "✗ 服务启动超时"
    docker compose -f "$SCRIPT_DIR/docker-compose.test.yml" logs
    docker compose -f "$SCRIPT_DIR/docker-compose.test.yml" down -v
    exit 1
fi

# 运行 API 测试
echo ""
echo "→ 运行 API 集成测试..."
chmod +x "$SCRIPT_DIR/api-test.sh"
BASE_URL="http://localhost:8081" TEST_PDF="$SCRIPT_DIR/test-files/sample.pdf" "$SCRIPT_DIR/api-test.sh"
test_result=$?

# 清理
echo ""
echo "→ 清理测试环境..."
docker compose -f "$SCRIPT_DIR/docker-compose.test.yml" down -v

if [ $test_result -eq 0 ]; then
    echo ""
    echo "=========================================="
    echo "  ✓ 所有测试通过！"
    echo "=========================================="
else
    echo ""
    echo "=========================================="
    echo "  ✗ 测试失败"
    echo "=========================================="
    exit 1
fi
