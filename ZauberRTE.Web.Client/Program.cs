using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ZauberCMS.RTE.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddZauberRte();

await builder.Build().RunAsync();
