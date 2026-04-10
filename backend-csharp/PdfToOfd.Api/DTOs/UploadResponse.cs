namespace PdfToOfd.Api.DTOs;

public record UploadResponse(
    bool Success,
    long? RecordId,
    string? Message
);

public record StatusResponse(
    long Id,
    string FileName,
    string Status,
    int? PageCount,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record HistoryResponse(
    List<StatusResponse> Records,
    int Total,
    int Page,
    int PageSize
);

public record JavaConvertRequest(
    string PdfPath,
    string OfdPath
);

public record JavaConvertResponse(
    bool Success,
    string? OfdPath,
    int? PageCount,
    string? ErrorMessage
);
