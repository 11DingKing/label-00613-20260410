using Microsoft.EntityFrameworkCore;
using PdfToOfd.Api.Data;
using PdfToOfd.Api.Middleware;
using PdfToOfd.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PDF to OFD API", Version = "v1" });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

// HTTP Client for Java service
builder.Services.AddHttpClient<IJavaConverterClient, JavaConverterClient>(client =>
{
    var javaServiceUrl = builder.Configuration["JavaService:BaseUrl"] ?? "http://backend-java:8080";
    client.BaseAddress = new Uri(javaServiceUrl);
    client.Timeout = TimeSpan.FromMinutes(10);
});

// Services
builder.Services.AddSingleton<IConversionTaskQueue, ConversionTaskQueue>();
builder.Services.AddHostedService<ConversionBackgroundService>();
builder.Services.AddScoped<IConversionService, ConversionService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Global exception handler
app.UseGlobalExceptionHandler();

// Apply migrations with retry
var maxRetries = 10;
for (int i = 0; i < maxRetries; i++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        Log.Information("Database connection established");
        break;
    }
    catch (Exception ex)
    {
        Log.Warning("Database connection attempt {Attempt}/{MaxRetries} failed: {Message}", i + 1, maxRetries, ex.Message);
        if (i == maxRetries - 1) throw;
        Thread.Sleep(3000);
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
