package com.converter.dto;

import jakarta.validation.constraints.NotBlank;
import lombok.Data;

@Data
public class ConvertRequest {
    
    @NotBlank(message = "PDF path is required")
    private String pdfPath;
    
    @NotBlank(message = "OFD path is required")
    private String ofdPath;
}
