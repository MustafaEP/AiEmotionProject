using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq;

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
        Description = "AI-based emotion analysis API"
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

// CORS - Specific origins in production
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

// EF Migrations - Auto apply (critical, fail if migration fails)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        logger.LogInformation("Checking database connection...");
        var canConnect = db.Database.CanConnect();
        
        if (!canConnect)
        {
            logger.LogWarning("Cannot connect to database. Attempting to create database...");
            db.Database.EnsureCreated();
            logger.LogInformation("Database created successfully.");
        }
        
        // Check if migration history table exists
        var hasMigrationHistory = false;
        try
        {
            var connection = db.Database.GetDbConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory'";
            var result = command.ExecuteScalar();
            hasMigrationHistory = result != null;
            connection.Close();
        }
        catch
        {
            // If we can't check, assume it doesn't exist
            hasMigrationHistory = false;
        }
        
        // Check if EmotionRecords table exists
        var hasEmotionRecordsTable = false;
        try
        {
            var connection = db.Database.GetDbConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='EmotionRecords'";
            var result = command.ExecuteScalar();
            hasEmotionRecordsTable = result != null;
            connection.Close();
        }
        catch
        {
            hasEmotionRecordsTable = false;
        }
        
        // If table exists but migration history doesn't, we need to seed the migration history
        if (hasEmotionRecordsTable && !hasMigrationHistory)
        {
            logger.LogWarning("EmotionRecords table exists but migration history is missing. Seeding migration history...");
            try
            {
                var connection = db.Database.GetDbConnection();
                connection.Open();
                using var command = connection.CreateCommand();
                
                // Create migration history table
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (
                        MigrationId TEXT NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
                        ProductVersion TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();
                
                // Insert existing migrations (assuming they were applied manually)
                var migrations = new[]
                {
                    ("20251107113822_InitialCreate", "9.0.10"),
                    ("20251107114343_AddTextColumnToEmotionRecord", "9.0.10")
                };
                
                foreach (var (migrationId, productVersion) in migrations)
                {
                    command.CommandText = @"
                        INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion)
                        VALUES (@MigrationId, @ProductVersion)";
                    var param1 = command.CreateParameter();
                    param1.ParameterName = "@MigrationId";
                    param1.Value = migrationId;
                    command.Parameters.Add(param1);
                    
                    var param2 = command.CreateParameter();
                    param2.ParameterName = "@ProductVersion";
                    param2.Value = productVersion;
                    command.Parameters.Add(param2);
                    
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                
                connection.Close();
                logger.LogInformation("Migration history seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed migration history. Will attempt to apply migrations normally.");
            }
        }
        
        logger.LogInformation("Applying migrations...");
        db.Database.Migrate();
        logger.LogInformation("Migrations applied successfully.");
    }
}
catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("already exists"))
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Table already exists. This might indicate a migration history mismatch. Attempting to continue...");
    // Try to apply only pending migrations
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Get pending migrations and apply them one by one
            var pendingMigrations = db.Database.GetPendingMigrations().ToList();
            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Found {pendingMigrations.Count} pending migrations. Applying...");
                db.Database.Migrate();
                logger.LogInformation("Pending migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("No pending migrations. Database is up to date.");
            }
        }
    }
    catch (Exception innerEx)
    {
        logger.LogCritical(innerEx, "CRITICAL: Failed to apply pending migrations. Application cannot start.");
        throw;
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "CRITICAL: Failed to apply database migrations. Application cannot start.");
    throw; // Fail fast - don't start the app without a working database
}

app.Run();
