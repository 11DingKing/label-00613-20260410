package com.converter.controller;

import com.converter.dto.ConvertRequest;
import com.converter.dto.ConvertResponse;
import com.converter.service.OfdConvertService;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

@WebMvcTest(ConvertController.class)
class ConvertControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @MockBean
    private OfdConvertService convertService;

    @Test
    @DisplayName("健康检查接口应返回 UP 状态")
    void healthCheck_ShouldReturnUpStatus() throws Exception {
        mockMvc.perform(get("/api/health"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.status").value("UP"))
                .andExpect(jsonPath("$.service").value("ofd-converter"))
                .andExpect(jsonPath("$.version").value("1.0.0"));
    }

    @Test
    @DisplayName("转换接口 - 成功转换应返回正确响应")
    void convert_Success_ShouldReturnOfdPath() throws Exception {
        ConvertRequest request = new ConvertRequest();
        request.setPdfPath("/data/pdf/test.pdf");
        request.setOfdPath("/data/ofd/test.ofd");

        ConvertResponse response = ConvertResponse.success("/data/ofd/test.ofd", 5);
        when(convertService.convertPdfToOfd(any(ConvertRequest.class))).thenReturn(response);

        mockMvc.perform(post("/api/convert")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.success").value(true))
                .andExpect(jsonPath("$.ofdPath").value("/data/ofd/test.ofd"))
                .andExpect(jsonPath("$.pageCount").value(5));
    }


    @Test
    @DisplayName("转换接口 - 转换失败应返回错误信息")
    void convert_Failure_ShouldReturnErrorMessage() throws Exception {
        ConvertRequest request = new ConvertRequest();
        request.setPdfPath("/data/pdf/test.pdf");
        request.setOfdPath("/data/ofd/test.ofd");

        when(convertService.convertPdfToOfd(any(ConvertRequest.class)))
                .thenThrow(new RuntimeException("PDF file not found"));

        mockMvc.perform(post("/api/convert")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.success").value(false))
                .andExpect(jsonPath("$.errorMessage").value("PDF file not found"));
    }

    @Test
    @DisplayName("转换接口 - 缺少必填字段应返回 400")
    void convert_MissingPdfPath_ShouldReturnBadRequest() throws Exception {
        ConvertRequest request = new ConvertRequest();
        request.setOfdPath("/data/ofd/test.ofd");
        // pdfPath 为空

        mockMvc.perform(post("/api/convert")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isBadRequest());
    }

    @Test
    @DisplayName("转换接口 - 空请求体应返回 400")
    void convert_EmptyBody_ShouldReturnBadRequest() throws Exception {
        mockMvc.perform(post("/api/convert")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{}"))
                .andExpect(status().isBadRequest());
    }
}
