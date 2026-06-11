using System.Text;
using FluentMigrator.Runner;
using FluentValidation;
using FluentValidation.AspNetCore;
using Kron.Counting.API.Extensions;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Interfaces.Repositories;
using Kron.Counting.Application.Services;
using Kron.Counting.Application.Validators;
using Kron.Counting.Infrastructure.BackgroundJobs;
using Kron.Counting.Infrastructure.Data;
using Kron.Counting.Infrastructure.DeviceGateways;
using Kron.Counting.Infrastructure.Migrations;
using Kron.Counting.Infrastructure.Repositories;
using Kron.Counting.Infrastructure.Security;
using Kron.Counting.Shared.Responses;
using Kron.Counting.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Kron.Counting.Infrastructure.Cache;
using Kron.Counting.Infrastructure.Realtime;

var builder = WebApplication.CreateBuilder(args);

#region Settings

//builder.Services.Configure<DatabaseSettings>(
//    builder.Configuration.GetSection(DatabaseSettings.SectionName));

var connectionString =
    builder.Configuration["DatabaseSettings:ConnectionString"];

builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("Redis"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddSingleton<IConnectionMultiplexer>(
    sp =>
    {
        var settings =
            builder.Configuration
                .GetSection("Redis")
                .Get<RedisSettings>();

        return ConnectionMultiplexer.Connect(
            settings!.ConnectionString);
    });

builder.Services.AddHttpClient();
#endregion

#region Migrations

builder.Services
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(
            builder.Configuration["DatabaseSettings:ConnectionString"])
        .ScanIn(typeof(MigrationRunnerService).Assembly)
        .For.Migrations());

if (!builder.Environment.IsProduction())
{
    builder.Services.AddHostedService<MigrationRunnerService>();
}
builder.Services.AddHostedService<PayloadReprocessorService>();
if (!builder.Environment.IsProduction())
{
    builder.Services.AddHostedService<HourlyMetricsMaterializerBackgroundService>();
}
builder.Services.AddHostedService<DailyMetricsMaterializerBackgroundService>();
builder.Services.AddHostedService<MonthlyMetricsMaterializerBackgroundService>();

#endregion

#region Controllers / Validation

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<CreateBrandValidator>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        var response = new ErrorResponse
        {
            Message = "Validation failed",
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});

#endregion

#region Swagger

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kron Counting API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<ICacheService, RedisCacheService>();
#endregion

#region Authentication

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings!.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };
    });

#endregion

#region Infrastructure

builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDeviceReadingRepository, DeviceReadingRepository>();
builder.Services.AddScoped<IDevicePayloadRepository, DevicePayloadRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IMaterializationStateRepository, MaterializationStateRepository>();
builder.Services.AddScoped<IHourlyMaterializationRepository, HourlyMaterializationRepository>();
builder.Services.AddScoped<IHourlyMetricsMaterializerService, HourlyMetricsMaterializerService>();
builder.Services.AddScoped<IDailyMaterializationRepository, DailyMaterializationRepository>();
builder.Services.AddScoped<IMonthlyMaterializationRepository, MonthlyMaterializationRepository>();
builder.Services.AddScoped<IMonthlyMetricsMaterializerService, MonthlyMetricsMaterializerService>();
builder.Services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();

builder.Services.AddScoped<IRealtimeNotificationService, SignalRRealtimeNotificationService>();


builder.Services.AddScoped<IDailyMetricsMaterializerService, DailyMetricsMaterializerService>();

builder.Services.AddHttpClient<IChp015Gateway, Chp015Gateway>();

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

#endregion

#region Application

builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
builder.Services.AddScoped<IDeviceProvisioningService, DeviceProvisioningService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDevicePayloadProcessor, DevicePayloadProcessor>();


#endregion

var app = builder.Build();

//Desarrollo
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<AnalyticsHub>(
    "/hubs/analytics");

app.Run();