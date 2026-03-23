using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using TravelPlaner.Api.Middleware;
using TravelPlaner.Application.Common;
using TravelPlaner.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Application & Infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT authentication
var signingKey = builder.Configuration["AUTH_LOCAL_JWT_SIGNING_KEY"]
    ?? builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("JWT signing key not configured. Set AUTH_LOCAL_JWT_SIGNING_KEY env var.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "travelplaner",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "travelplaner",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TravelPlaner API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT Bearer token"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            []
        }
    });
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins(builder.Configuration["CORS_ORIGINS"]?.Split(',') ?? ["http://localhost:4200"])
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

// Apply DB migrations on startup
await app.Services.ApplyMigrationsAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Serve Angular SPA static files (built output placed in wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// Serve uploaded images
var imagePath = Path.GetFullPath(builder.Configuration["IMAGE_STORE_PATH"] ?? "/data/images");
Directory.CreateDirectory(imagePath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagePath),
    RequestPath = "/images"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Angular SPA fallback
app.MapFallbackToFile("index.html");

app.Run();
