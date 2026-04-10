using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PdfToOfd.Api.Models;

[Table("operation_log")]
public class OperationLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("operation")]
    [MaxLength(50)]
    public string Operation { get; set; } = string.Empty;

    [Column("target_id")]
    [MaxLength(50)]
    public string? TargetId { get; set; }

    [Column("ip_address")]
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [Column("request_body")]
    public string? RequestBody { get; set; }

    [Column("response_code")]
    public int ResponseCode { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
