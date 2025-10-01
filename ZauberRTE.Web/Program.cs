using Zauber.RTE.Services;
using ZauberRTE.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure SignalR to support larger messages (for base64 images)
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1 * 1024 * 1024; // 1 MB (allows images up to ~750KB)
});

// Add Zauber RTE services
builder.Services.AddZauberRte();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();