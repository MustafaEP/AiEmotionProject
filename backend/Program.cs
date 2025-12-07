using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Controllers with validation
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AI Emotion API",
        Version = "v1",
        Description = "AI tabanlı duygu analizi API'si"
    });
});

builder.Services.AddMemoryCache();

// HTTP Clients
builder.Services.AddHttpClient<EmotionService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(30);
});

var selfBaseUrl = Environment.GetEnvironmentVariable("SELF_BASE_URL")
                  ?? builder.Configuration["Self:BaseUrl"]
                  ?? "https://aiemotionproject.onrender.com/";
builder.Services.AddHttpClient("SelfApi", c => c.BaseAddress = new Uri(selfBaseUrl));

// CORS - Production'da spesifik origin'ler
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "https://ai-emotion-project-llej9t1cm-mustafa-erhans-projects.vercel.app" };

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    ));
}
else
{
    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()
    ));
}

// Database configuration
var env = builder.Environment;
string dbPath;
if (env.IsDevelopment())
{
    dbPath = Path.Combine(Directory.GetCurrentDirectory(), "emotiondata.db");
}
else
{
    Directory.CreateDirectory("/var/data");
    dbPath = Path.Combine("/var/data", "emotiondata.db");
}
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

// Port configuration
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck<backend.HealthChecks.DatabaseHealthCheck>("database");

var app = builder.Build();

// Error handling middleware
app.UseExceptionHandler("/error");
app.UseStatusCodePages();

// Proxy/HTTPS headers (Render SSL termination)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

// Swagger - Only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Emotion API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// EF Migrations - Auto apply
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Migration sırasında hata oluştu.");
    }
}

app.Run();
