using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PdfToOfd.Api.Models;

[Table("conversion_record")]
public class ConversionRecord
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("file_name")]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Column("pdf_path")]
    [MaxLength(500)]
    public string PdfPath { get; set; } = string.Empty;

    [Column("ofd_path")]
    [MaxLength(500)]
    public string? OfdPath { get; set; }

    [Column("status")]
    public ConversionStatus Status { get; set; } = ConversionStatus.Pending;

    [Column("error_message")]
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    [Column("file_size")]
    public long FileSize { get; set; }

    [Column("page_count")]
    public int? PageCount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ConversionStatus
{
    Pending = 0,
    Processing = 1,
    Success = 2,
    Failed = 3
}
