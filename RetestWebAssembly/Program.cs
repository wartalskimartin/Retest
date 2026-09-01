using RetestWebAssembly;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RetestWebAssembly.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Medium;
});

builder.Services.AddScoped<BilboxService>();
builder.Services.AddSingleton<FilterService>();

builder.Services.AddScoped<ClientLogStoreService>();
builder.Services.AddScoped<AppLogService>();
builder.Services.AddScoped<ClientLogUploaderService>();

var host = builder.Build();

// startup log = jistota, že vznikne aspoň 1 záznam
await host.Services.GetRequiredService<AppLogService>()
    .InfoAsync("App started");

host.Services.GetRequiredService<ClientLogUploaderService>().Start();
await host.RunAsync();