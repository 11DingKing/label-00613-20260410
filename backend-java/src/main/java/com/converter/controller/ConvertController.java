package com.converter.controller;

import com.converter.dto.ConvertRequest;
import com.converter.dto.ConvertResponse;
import com.converter.service.OfdConvertService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@Slf4j
@RestController
@RequestMapping("/api")
@RequiredArgsConstructor
public class ConvertController {

    private final OfdConvertService convertService;

    @PostMapping("/convert")
    public ResponseEntity<ConvertResponse> convert(@Valid @RequestBody ConvertRequest request) {
        log.info("Received convert request: pdfPath={}", request.getPdfPath());
        try {
            ConvertResponse response = convertService.convertPdfToOfd(request);
            log.info("Conversion completed: ofdPath={}", response.getOfdPath());
            return ResponseEntity.ok(response);
        } catch (Exception e) {
            log.error("Conversion failed: {}", e.getMessage(), e);
            return ResponseEntity.ok(ConvertResponse.failure(e.getMessage()));
        }
    }

    @GetMapping("/health")
    public ResponseEntity<Map<String, Object>> health() {
        return ResponseEntity.ok(Map.of(
                "status", "UP",
                "service", "ofd-converter",
                "version", "1.0.0"
        ));
    }
}
