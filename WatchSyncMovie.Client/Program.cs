using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WatchSyncMovie.Client;
using WatchSyncMovie.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7083/") });
builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<VideoPlayerService>();

await builder.Build().RunAsync();
