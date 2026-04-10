package com.converter.service;

import com.converter.dto.ConvertRequest;
import com.converter.dto.ConvertResponse;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.nio.file.Files;
import java.nio.file.Path;

import static org.junit.jupiter.api.Assertions.*;

class OfdConvertServiceTest {

    private OfdConvertService convertService;

    @TempDir
    Path tempDir;

    @BeforeEach
    void setUp() {
        convertService = new OfdConvertService();
    }

    @Test
    @DisplayName("PDF 文件不存在时应抛出异常")
    void convertPdfToOfd_PdfNotFound_ShouldThrowException() {
        ConvertRequest request = new ConvertRequest();
        request.setPdfPath("/nonexistent/path/test.pdf");
        request.setOfdPath("/data/ofd/test.ofd");

        IllegalArgumentException exception = assertThrows(
                IllegalArgumentException.class,
                () -> convertService.convertPdfToOfd(request)
        );

        assertTrue(exception.getMessage().contains("PDF file not found"));
    }

    @Test
    @DisplayName("输出目录不存在时应自动创建")
    void convertPdfToOfd_OutputDirNotExists_ShouldCreateDir() throws Exception {
        // 创建一个空的 PDF 文件用于测试（实际转换会失败，但目录应该被创建）
        Path pdfPath = tempDir.resolve("test.pdf");
        Files.createFile(pdfPath);
        
        Path ofdDir = tempDir.resolve("output/nested/dir");
        Path ofdPath = ofdDir.resolve("test.ofd");

        ConvertRequest request = new ConvertRequest();
        request.setPdfPath(pdfPath.toString());
        request.setOfdPath(ofdPath.toString());

        // 由于是空 PDF，转换会失败，但我们只验证目录创建逻辑
        try {
            convertService.convertPdfToOfd(request);
        } catch (Exception e) {
            // 预期会失败，因为不是有效的 PDF
        }

        // 验证父目录已创建
        assertTrue(Files.exists(ofdDir), "输出目录应该被创建");
    }
}
