using Microsoft.EntityFrameworkCore;
using PdfToOfd.Api.Models;

namespace PdfToOfd.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ConversionRecord> ConversionRecords => Set<ConversionRecord>();
    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConversionRecord>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<OperationLog>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Operation);
        });
    }
}
