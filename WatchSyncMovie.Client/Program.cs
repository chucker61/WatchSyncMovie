using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WatchSyncMovie.Client;
using WatchSyncMovie.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://watchsync-server.delightfulbay-1a92ac00.francecentral.azurecontainerapps.io") });
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<VideoPlayerService>();

await builder.Build().RunAsync();
