package com.converter.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class ConvertResponse {
    
    private boolean success;
    private String ofdPath;
    private Integer pageCount;
    private String errorMessage;

    public static ConvertResponse success(String ofdPath, int pageCount) {
        return ConvertResponse.builder()
                .success(true)
                .ofdPath(ofdPath)
                .pageCount(pageCount)
                .build();
    }

    public static ConvertResponse failure(String errorMessage) {
        return ConvertResponse.builder()
                .success(false)
                .errorMessage(errorMessage)
                .build();
    }
}
