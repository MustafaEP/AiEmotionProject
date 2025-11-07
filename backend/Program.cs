using backend;
using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<EmotionService>();

builder.Services.AddControllers();
builder.Services.AddMemoryCache();



builder.Services.AddHttpClient<EmotionService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    // client.BaseAddress = new Uri("https://mustafaep-emotion-analyzer.hf.space/");
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
));

// Render.com gibi reverse proxy'ler için ForwardedHeaders yapılandırması
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// SelfApi HttpClient'ı runtime'da base URL ile yapılandırılacak
// SyncAnalyzeController'da request'in base URL'i kullanılacak
builder.Services.AddHttpClient("SelfApi");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=emotiondata.db"));

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port)) builder.WebHost.UseUrls($"http://0.0.0.0:{port}");



var app = builder.Build();

// Render.com gibi reverse proxy'ler için ForwardedHeaders middleware'i
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

app.Run();
