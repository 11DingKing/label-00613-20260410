package com.converter.service;

import com.converter.dto.ConvertRequest;
import com.converter.dto.ConvertResponse;
import lombok.extern.slf4j.Slf4j;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.ofdrw.converter.ofdconverter.PDFConverter;
import org.springframework.stereotype.Service;

import java.io.File;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

@Slf4j
@Service
public class OfdConvertService {

    public ConvertResponse convertPdfToOfd(ConvertRequest request) throws Exception {
        Path pdfPath = Paths.get(request.getPdfPath());
        Path ofdPath = Paths.get(request.getOfdPath());

        // Validate PDF exists
        if (!Files.exists(pdfPath)) {
            throw new IllegalArgumentException("PDF file not found: " + pdfPath);
        }

        // Ensure output directory exists
        Path parentDir = ofdPath.getParent();
        if (parentDir != null && !Files.exists(parentDir)) {
            Files.createDirectories(parentDir);
        }

        log.info("Starting PDF to OFD conversion: {} -> {}", pdfPath, ofdPath);

        // Get page count from PDF
        int pageCount;
        try (PDDocument pdfDoc = PDDocument.load(new File(pdfPath.toString()))) {
            pageCount = pdfDoc.getNumberOfPages();
            log.info("PDF has {} pages", pageCount);
        }

        // Use ofdrw PDFConverter to convert PDF to OFD
        try (PDFConverter converter = new PDFConverter(ofdPath)) {
            converter.convert(pdfPath);
        }

        // Verify output
        if (!Files.exists(ofdPath)) {
            throw new RuntimeException("OFD file was not created");
        }

        log.info("Conversion successful: {}", ofdPath);
        return ConvertResponse.success(ofdPath.toString(), pageCount);
    }
}
