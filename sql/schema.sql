-- ============================================
-- PDF to OFD Conversion System
-- Database Schema
-- ============================================

CREATE DATABASE IF NOT EXISTS pdf_to_ofd;
USE pdf_to_ofd;

-- ============================================
-- 转换记录表
-- ============================================
CREATE TABLE IF NOT EXISTS conversion_record (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    file_name VARCHAR(255) NOT NULL COMMENT '原始文件名',
    pdf_path VARCHAR(500) NOT NULL COMMENT 'PDF存储路径',
    ofd_path VARCHAR(500) COMMENT 'OFD存储路径',
    status INT NOT NULL DEFAULT 0 COMMENT '状态: 0-待处理 1-处理中 2-成功 3-失败',
    error_message VARCHAR(1000) COMMENT '错误信息',
    file_size BIGINT NOT NULL DEFAULT 0 COMMENT '文件大小(bytes)',
    page_count INT COMMENT '页数',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
    INDEX idx_status (status),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='转换记录表';

-- ============================================
-- 操作日志表
-- ============================================
CREATE TABLE IF NOT EXISTS operation_log (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    operation VARCHAR(50) NOT NULL COMMENT '操作类型',
    target_id VARCHAR(50) COMMENT '目标ID',
    ip_address VARCHAR(50) COMMENT 'IP地址',
    user_agent VARCHAR(500) COMMENT '用户代理',
    request_body TEXT COMMENT '请求体',
    response_code INT NOT NULL DEFAULT 200 COMMENT '响应码',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    INDEX idx_created_at (created_at),
    INDEX idx_operation (operation)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='操作日志表';
