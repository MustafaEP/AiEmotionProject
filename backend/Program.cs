using Microsoft.EntityFrameworkCore;
using backend.Data;

var builder = WebApplication.CreateBuilder(args);

// DB + CORS
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=emotiondata.db"));
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddControllers();

// Swagger prod'da açık
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SelfApi base URL'i ENV'den al, yoksa config, yoksa local
var selfBaseUrl =
    Environment.GetEnvironmentVariable("SELF_BASE_URL") ??
    builder.Configuration["Self:BaseUrl"] ??
    "https://localhost:7104/";

builder.Services.AddHttpClient("SelfApi", c => c.BaseAddress = new Uri(selfBaseUrl));

// Render portu dinle
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// (İsteğe bağlı) https redirect'ı kaldır, Render zaten https veriyor
// app.UseHttpsRedirection();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Emotion API v1");
    c.RoutePrefix = "swagger";
});

// health
app.MapGet("/", () => Results.Ok(new {
    ok = true,
    env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    selfBase = Environment.GetEnvironmentVariable("SELF_BASE_URL")
}));

app.MapControllers();
app.Run();
