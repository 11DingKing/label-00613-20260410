#!/bin/bash
# PDF to OFD API 集成测试脚本
# 用于 Docker 环境下的端到端测试

# 不使用 set -e，因为我们需要继续执行所有测试

BASE_URL="${BASE_URL:-http://localhost:8081}"
JAVA_URL="${JAVA_URL:-http://localhost:8080}"
TEST_PDF="${TEST_PDF:-./test-files/sample.pdf}"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

passed=0
failed=0

log_pass() {
    echo -e "${GREEN}✓ PASS${NC}: $1"
    passed=$((passed + 1))
}

log_fail() {
    echo -e "${RED}✗ FAIL${NC}: $1"
    failed=$((failed + 1))
}

log_info() {
    echo -e "${YELLOW}→${NC} $1"
}

# 等待服务就绪
wait_for_service() {
    local url=$1
    local name=$2
    local max_attempts=30
    local attempt=1

    log_info "等待 $name 服务就绪..."
    while [ $attempt -le $max_attempts ]; do
        if curl -s -f "$url" > /dev/null 2>&1; then
            log_pass "$name 服务已就绪"
            return 0
        fi
        sleep 2
        attempt=$((attempt + 1))
    done
    log_fail "$name 服务未能在 60 秒内就绪"
    return 1
}

echo "=========================================="
echo "  PDF to OFD API 集成测试"
echo "=========================================="
echo ""

# 1. 健康检查测试
echo "--- 1. 健康检查测试 ---"

# 1.1 C# API 健康检查
log_info "测试 C# API 健康检查..."
response=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/health")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" = "200" ]; then
    if echo "$body" | grep -q '"status":"UP"'; then
        log_pass "C# API 健康检查返回 UP"
    else
        log_fail "C# API 健康检查状态异常: $body"
    fi
else
    log_fail "C# API 健康检查 HTTP 状态码: $http_code"
fi

# 1.2 Java 服务健康检查（通过 C# API 依赖检查）
log_info "测试 Java 服务健康检查..."
if echo "$body" | grep -q '"javaConverter":"UP"'; then
    log_pass "Java 转换服务健康检查返回 UP"
else
    log_fail "Java 转换服务健康检查状态异常"
fi

echo ""
echo "--- 2. 文件上传测试 ---"

# 2.1 无文件上传
log_info "测试无文件上传..."
response=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/file/upload")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" = "400" ]; then
    log_pass "无文件上传返回 400"
else
    log_fail "无文件上传应返回 400，实际: $http_code"
fi

# 2.2 非 PDF 文件上传
log_info "测试非 PDF 文件上传..."
echo "test content" > /tmp/test.txt
response=$(curl -s -w "\n%{http_code}" -X POST -F "file=@/tmp/test.txt" "$BASE_URL/api/file/upload")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" = "400" ]; then
    if echo "$body" | grep -q "Only PDF files are allowed"; then
        log_pass "非 PDF 文件上传返回正确错误信息"
    else
        log_fail "非 PDF 文件上传错误信息不正确: $body"
    fi
else
    log_fail "非 PDF 文件上传应返回 400，实际: $http_code"
fi
rm -f /tmp/test.txt

# 2.3 有效 PDF 文件上传（如果测试文件存在）
if [ -f "$TEST_PDF" ]; then
    log_info "测试有效 PDF 文件上传..."
    response=$(curl -s -w "\n%{http_code}" -X POST -F "file=@$TEST_PDF" "$BASE_URL/api/file/upload")
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')

    if [ "$http_code" = "200" ]; then
        if echo "$body" | grep -q '"success":true'; then
            record_id=$(echo "$body" | grep -o '"recordId":[0-9]*' | grep -o '[0-9]*')
            log_pass "PDF 文件上传成功，记录 ID: $record_id"
            
            # 保存 record_id 供后续测试使用
            export RECORD_ID=$record_id
        else
            log_fail "PDF 文件上传失败: $body"
        fi
    else
        log_fail "PDF 文件上传 HTTP 状态码: $http_code"
    fi
else
    log_info "跳过 PDF 上传测试（测试文件不存在: $TEST_PDF）"
fi

echo ""
echo "--- 3. 状态查询测试 ---"

# 3.1 查询不存在的记录
log_info "测试查询不存在的记录..."
response=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/file/status/99999")
http_code=$(echo "$response" | tail -n1)

if [ "$http_code" = "404" ]; then
    log_pass "查询不存在记录返回 404"
else
    log_fail "查询不存在记录应返回 404，实际: $http_code"
fi

# 3.2 查询已上传的记录（如果存在）
if [ -n "$RECORD_ID" ]; then
    log_info "测试查询已上传记录状态..."
    response=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/file/status/$RECORD_ID")
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')

    if [ "$http_code" = "200" ]; then
        log_pass "查询记录状态成功"
        echo "    状态: $(echo "$body" | grep -o '"status":"[^"]*"')"
    else
        log_fail "查询记录状态失败: $http_code"
    fi
fi

echo ""
echo "--- 4. 历史记录测试 ---"

# 4.1 获取历史记录列表
log_info "测试获取历史记录列表..."
response=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/history")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" = "200" ]; then
    if echo "$body" | grep -q '"records"'; then
        log_pass "获取历史记录列表成功"
    else
        log_fail "历史记录响应格式异常: $body"
    fi
else
    log_fail "获取历史记录列表失败: $http_code"
fi

# 4.2 分页测试
log_info "测试历史记录分页..."
response=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/history?page=1&pageSize=5")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" = "200" ]; then
    if echo "$body" | grep -q '"pageSize":5'; then
        log_pass "历史记录分页参数正确"
    else
        log_fail "历史记录分页参数异常"
    fi
else
    log_fail "历史记录分页请求失败: $http_code"
fi

echo ""
echo "--- 5. 下载测试 ---"

# 5.1 下载不存在的文件
log_info "测试下载不存在的文件..."
response=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/file/download/99999")
http_code=$(echo "$response" | tail -n1)

if [ "$http_code" = "404" ]; then
    log_pass "下载不存在文件返回 404"
else
    log_fail "下载不存在文件应返回 404，实际: $http_code"
fi

# 5.2 下载已转换的文件（如果存在且转换成功）
if [ -n "$RECORD_ID" ]; then
    log_info "测试下载已转换文件..."
    response=$(curl -s -w "\n%{http_code}" -o /tmp/downloaded.ofd "$BASE_URL/api/file/download/$RECORD_ID")
    http_code=$(echo "$response" | tail -n1)

    if [ "$http_code" = "200" ]; then
        if [ -f /tmp/downloaded.ofd ] && [ -s /tmp/downloaded.ofd ]; then
            log_pass "下载 OFD 文件成功"
            rm -f /tmp/downloaded.ofd
        else
            log_fail "下载的文件为空或不存在"
        fi
    elif [ "$http_code" = "404" ]; then
        log_info "文件尚未转换完成或转换失败"
    else
        log_fail "下载文件失败: $http_code"
    fi
fi

echo ""
echo "--- 6. 删除测试 ---"

# 6.1 删除不存在的记录
log_info "测试删除不存在的记录..."
response=$(curl -s -w "\n%{http_code}" -X DELETE "$BASE_URL/api/history/99999")
http_code=$(echo "$response" | tail -n1)

if [ "$http_code" = "404" ]; then
    log_pass "删除不存在记录返回 404"
else
    log_fail "删除不存在记录应返回 404，实际: $http_code"
fi

echo ""
echo "=========================================="
echo "  测试结果汇总"
echo "=========================================="
echo -e "  ${GREEN}通过${NC}: $passed"
echo -e "  ${RED}失败${NC}: $failed"
echo "=========================================="

if [ $failed -gt 0 ]; then
    exit 1
fi
exit 0
