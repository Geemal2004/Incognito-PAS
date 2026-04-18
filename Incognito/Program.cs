using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using IncognitoPAS.Data;
using IncognitoPAS.DTOs.Api;
using IncognitoPAS.Models;
using IncognitoPAS.Services;
using IncognitoPAS.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            // Handle short-lived SQL connectivity issues (especially during startup/local DB spin-up).
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

// Identity with strict password policy
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Stores.MaxLengthForKeys = 128;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// DECISION: Using JWT Bearer tokens for SPA authentication
// This allows stateless API authentication suitable for Angular
var jwtKey = builder.Configuration["Jwt:Key"] ?? "IncognitoPAS_SecretKey_ForDevelopment_2026!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "IncognitoPAS";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "IncognitoPAS";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// CORS for Angular development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("https://localhost:4200", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// API Controllers only (no MVC views)
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Swagger/OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IncognitoPAS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Register services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<IResearchAreaService, ResearchAreaService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IExpertiseService, ExpertiseService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", p => p.RequireRole("Student"));
    options.AddPolicy("SupervisorOnly", p => p.RequireRole("Supervisor"));
    options.AddPolicy("LeaderOrAdmin", p => p.RequireRole("ModuleLeader", "SysAdmin"));
});

var app = builder.Build();

// Security middleware headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ApiErrorDto
        {
            Message = app.Environment.IsDevelopment()
                ? exception?.Message ?? "An unexpected error occurred."
                : "An unexpected error occurred.",
            Code = "ServerError"
        });
    });
});

app.UseHttpsRedirection();

// CORS for Angular
app.UseCors("AllowAngularDev");

app.UseAuthentication();
app.UseAuthorization();

// API endpoints
app.MapControllers();

// Seed data on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    await EnsureDatabaseReadyAsync(dbContext);
    await SeedData.InitializeAsync(services);
}

app.Run();

static async Task EnsureDatabaseReadyAsync(ApplicationDbContext dbContext)
{
    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (SqlException ex) when (ex.Number == 2714)
    {
        await MarkInitialMigrationAsAppliedAsync(dbContext);
        await dbContext.Database.MigrateAsync();
    }

    await EnsureAuditLogsSchemaAsync(dbContext);
}

static async Task MarkInitialMigrationAsAppliedAsync(ApplicationDbContext dbContext)
{
    const string baselineMigrationSql = """
IF OBJECT_ID(N'[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260416100429_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260416100429_InitialCreate', N'8.0.26');
END;
""";

    await dbContext.Database.ExecuteSqlRawAsync(baselineMigrationSql);
}

static async Task EnsureAuditLogsSchemaAsync(ApplicationDbContext dbContext)
{
    const string ensureAuditLogsTableSql = """
IF OBJECT_ID(N'[dbo].[AuditLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(450) NOT NULL,
        [ActorUserId] nvarchar(450) NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [Timestamp] datetime2 NOT NULL,
        [IpAddress] nvarchar(50) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_AuditLogs_EntityType_EntityId'
      AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]')
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityType_EntityId]
    ON [dbo].[AuditLogs] ([EntityType], [EntityId]);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_AuditLogs_Timestamp'
      AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]')
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Timestamp]
    ON [dbo].[AuditLogs] ([Timestamp]);
END;
""";

    await dbContext.Database.ExecuteSqlRawAsync(ensureAuditLogsTableSql);
}