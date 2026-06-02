using httpNode.Components;
using httpNode.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Disable antiforgery validation in development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAntiforgery(options => options.SuppressXFrameOptionsHeader = true);
}

// Register NodeService as singleton
builder.Services.AddSingleton<NodeService>();

// Register the hosted service to run the node's election timer
builder.Services.AddHostedService<NodeHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
