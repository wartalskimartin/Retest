using Microsoft.AspNetCore.Authentication.OAuth;
using SharedLib.DataAccess;
using Microsoft.EntityFrameworkCore;
using PushAPI.Services;

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

builder.Services.AddScoped<BibloxService>();

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
app.UseResponseCaching();
app.UseCors(MyAllowSpecificOrigins);
app.MapControllers();

app.Run();
