using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<EmotionService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(30);
});

var selfBaseUrl = Environment.GetEnvironmentVariable("SELF_BASE_URL")
                  ?? builder.Configuration["Self:BaseUrl"]
                  ?? "https://aiemotionproject.onrender.com/"; // Render i�in fallback
builder.Services.AddHttpClient("SelfApi", c => c.BaseAddress = new Uri(selfBaseUrl));

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
));

// --- DB: SQLite dosyas�n� /var/data�ya yaz
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

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// --- Proxy/HTTPS head�leri (Render SSL terminasyonu)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

// --- Swagger (prod�da da a��k kals�n)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Emotion API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// --- EF Migration�lar� otomatik uygula
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
