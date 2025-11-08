using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- CORS / Controllers / Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// --- HTTP Clients
builder.Services.AddHttpClient<EmotionService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(30);
    // c.BaseAddress = new Uri("https://mustafaep-emotion-analyzer.hf.space/");
});

// --- Self API base url (env > appsettings > fallback)
var selfBaseUrl = Environment.GetEnvironmentVariable("SELF_BASE_URL")
                  ?? builder.Configuration["Self:BaseUrl"]
                  ?? "https://aiemotionproject.onrender.com/"; // Render için fallback
builder.Services.AddHttpClient("SelfApi", c => c.BaseAddress = new Uri(selfBaseUrl));

// --- CORS
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
));

// --- DB: SQLite dosyasýný /var/data’ya yaz
var env = builder.Environment;
string dbPath;
if (env.IsDevelopment())
{
    dbPath = Path.Combine(Directory.GetCurrentDirectory(), "emotiondata.db");
}
else
{
    // Render'da Persistent Disk'i /var/data olarak mount ettik
    Directory.CreateDirectory("/var/data");
    dbPath = Path.Combine("/var/data", "emotiondata.db");
}
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

// --- Render PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// --- Proxy/HTTPS head’leri (Render SSL terminasyonu)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

// --- Swagger (prod’da da açýk kalsýn)
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

// --- EF Migration’larý otomatik uygula
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
