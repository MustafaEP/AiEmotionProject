using backend;
using backend.Data;
using backend.Services;
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

var selfBaseUrl = Environment.GetEnvironmentVariable("SELF_BASE_URL")
                  ?? builder.Configuration["Self:BaseUrl"]
                  ?? "https://aiemotionproject.onrender.com/" // Render için. Normal durumda silinmeli
                  ?? "https://localhost:7104/";

builder.Services.AddHttpClient("SelfApi", c => c.BaseAddress = new Uri(selfBaseUrl));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHttpClient("SelfApi", client =>
{
    client.BaseAddress = new Uri(selfBaseUrl);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=emotiondata.db"));

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port)) builder.WebHost.UseUrls($"http://0.0.0.0:{port}");



var app = builder.Build();

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
