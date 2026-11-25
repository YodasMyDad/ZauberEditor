using ZauberCMS.RTE.Services;
using ZauberRTE.Web.Components;
using ZauberRTE.Web.Toolbar;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddZauberRte(options =>
{
    options.AddToolbarItem<HighlightItem>();
    options.AddToolbarItem<InsertTimeItem>();
    options.AddToolbarItem<CalloutItem>();
    options.AddToolbarItem<TextColorDropdownItem>();
    options.AddToolbarItem<RemoveTextColorItem>();
    foreach (var color in TextColorItem.Palette)
    {
        options.AddToolbarItem(color);
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ZauberRTE.Web.Client._Imports).Assembly);

app.Run();
