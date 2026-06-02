using node.Components;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Get service name from environment
var serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "node-service";

// Configure OpenTelemetry logging
builder.Logging.ClearProviders();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.AddOtlpExporter(otlpOptions =>
    {
        otlpOptions.Endpoint = new Uri("http://aspire-dashboard:18889");
    });
    logging.SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddAttributes(new Dictionary<string, object> { { "service.name", serviceName } })
    );
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
