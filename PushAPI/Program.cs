using Microsoft.AspNetCore.Authentication.OAuth;
using SharedLib.DataAccess;
using Microsoft.EntityFrameworkCore;
using PushAPI.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("CockpitDatabase");
builder.Services.AddDbContextFactory<cockpitContext>(
                options =>
                    options.UseSqlServer(
                        connectionString,
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.CommandTimeout(180);
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 10,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);
                        }));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add OpenTelemetry services to the container.
builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Retest"))
            .AddSource("RetestAPI")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddJaegerExporter(jaegerOptions =>
            {
                jaegerOptions.AgentHost = "10.13.0.54";
                jaegerOptions.AgentPort = 6831;
            });
    });

builder.Services.AddScoped<BilboxService>();

builder.Services.AddControllers();

string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options => {
    options.AddPolicy(MyAllowSpecificOrigins,
    builder => {
        builder.AllowAnyOrigin();
        builder.AllowAnyHeader();
    });
});

builder.Services.AddResponseCaching();

var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
 
app.UseResponseCaching();
app.UseCors(MyAllowSpecificOrigins);
app.MapControllers();

app.Run();
