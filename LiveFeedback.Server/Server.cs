using System.Reflection;
using LiveFeedback.Server.Services.SignalR;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using LiveFeedback.Shared.Models;
using Microsoft.Extensions.FileProviders;

namespace LiveFeedback.Server;

public class Server
{
    private WebApplication _app = null!;

    public async Task StartAsync(Mode mode = Mode.Local)
    {
        try
        {
            GlobalConfig globalConfig = new();

            ManifestEmbeddedFileProvider embeddedFileProvider = new(typeof(Server).Assembly, "wwwroot");
            string serverFolder = Path.GetDirectoryName(AppContext.BaseDirectory)!;

            WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ApplicationName = typeof(Server).Assembly.GetName().Name,
                ContentRootPath = serverFolder
            });

            builder.WebHost.UseStaticWebAssets();

            // 1) Services
            builder.Services.AddSingleton<GlobalConfig>();
            builder.Services.AddSingleton<SliderHubHelpers>();
            builder.Services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;

                    options.PayloadSerializerOptions.TypeInfoResolver = EfficientJsonContext.Default;
                });
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
            });

            builder.WebHost.UseUrls($"http://{globalConfig.ServerHost}:{globalConfig.ServerPort}");
            _app = builder.Build();

            // 2) Routing
            _app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = embeddedFileProvider,
                RequestPath = ""
            });
            _app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = embeddedFileProvider,
                RequestPath = ""
            });
            _app.UseWebSockets();
            _app.UseRouting();

            // 3) SignalR hubs
            _app.MapHub<SliderHub>("/slider-hub");

            // 4) API
            _app.MapGet("/api/hello", () => "Hello from LiveFeedback server!");

            // 5) Special deep link fallbacks for WebFrontends
            _app.MapFallbackToFile("{*path}", "index.html", new StaticFileOptions
            {
                FileProvider = embeddedFileProvider
            });

            _app.Logger.LogInformation("Trying to start server…");

            if (mode == Mode.Distributed)
            {
                _app.Logger.LogInformation(
                    "Server runs in distributed mode and assumes that it runs in its own process.");
                await _app.RunAsync();
            }
            else
            {
                _app.Logger.LogInformation(
                    "Server running in local mode and assumes that it is running in the same process as the main program.");
                await _app.StartAsync(); // run server in the background
            }

            _app.Logger.LogInformation("Server started");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task StopAsync()
    {
        _app.Logger.LogInformation("Stopping Server…");
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}