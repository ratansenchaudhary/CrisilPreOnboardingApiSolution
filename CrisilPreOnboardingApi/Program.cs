using System.Text.Json;
using System.Text.Json.Serialization;
using CrisilPreOnboardingApi.Data;
using CrisilPreOnboardingApi.Models;
using CrisilPreOnboardingApi.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON snake_case
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Middlewares
builder.Services.AddTransient<CrisilPreOnboardingApi.Infrastructure.ExceptionHandlingMiddleware>();
builder.Services.AddTransient<CrisilPreOnboardingApi.Infrastructure.RequestResponseLoggingMiddleware>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core SQL Server
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<PreOnboardingRequestValidator>();

// Unified model-state error response (for JSON binding / format issues)
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var traceId = context.HttpContext.TraceIdentifier;

        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors.Select(e => new ApiFieldError
            {
                Field = kvp.Key,
                ErrorCode = "INVALID",
                Message = string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage
            }))
            .ToList();

        var payload = new ApiErrorResponse
        {
            Code = "VALIDATION_FAILED",
            Message = "One or more validation errors occurred.",
            Errors = errors,
            TraceId = traceId
        };

        return new BadRequestObjectResult(payload);
    };
});

var app = builder.Build();

app.Logger.LogInformation("DB Connection: {cs}", app.Configuration.GetConnectionString("DefaultConnection"));


app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<CrisilPreOnboardingApi.Infrastructure.ExceptionHandlingMiddleware>();
app.UseMiddleware<CrisilPreOnboardingApi.Infrastructure.RequestResponseLoggingMiddleware>();

app.MapControllers();

app.Run();
