package com.converter.dto;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

class ConvertResponseTest {

    @Test
    @DisplayName("success 工厂方法应创建成功响应")
    void success_ShouldCreateSuccessResponse() {
        ConvertResponse response = ConvertResponse.success("/data/ofd/test.ofd", 10);

        assertTrue(response.isSuccess());
        assertEquals("/data/ofd/test.ofd", response.getOfdPath());
        assertEquals(10, response.getPageCount());
        assertNull(response.getErrorMessage());
    }

    @Test
    @DisplayName("failure 工厂方法应创建失败响应")
    void failure_ShouldCreateFailureResponse() {
        ConvertResponse response = ConvertResponse.failure("Conversion failed");

        assertFalse(response.isSuccess());
        assertNull(response.getOfdPath());
        assertNull(response.getPageCount());
        assertEquals("Conversion failed", response.getErrorMessage());
    }

    @Test
    @DisplayName("Builder 模式应正确构建对象")
    void builder_ShouldBuildCorrectly() {
        ConvertResponse response = ConvertResponse.builder()
                .success(true)
                .ofdPath("/path/to/file.ofd")
                .pageCount(5)
                .build();

        assertTrue(response.isSuccess());
        assertEquals("/path/to/file.ofd", response.getOfdPath());
        assertEquals(5, response.getPageCount());
    }
}
