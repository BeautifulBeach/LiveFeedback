using System.Reflection;
using LiveFeedback.Server.Services.SignalR;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;
using LiveFeedback.Shared.Models;

namespace LiveFeedback.Server;

public class Server
{
    private WebApplication _app = null!;

    public async Task StartAsync(Mode mode = Mode.Local)
    {
        try
        {
            GlobalConfig globalConfig = new();

            Assembly serverAssembly = typeof(Server).Assembly;
            string serverFolder = Path.GetDirectoryName(serverAssembly.Location)!;

            WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ApplicationName = serverAssembly.GetName().Name,
                ContentRootPath = serverFolder,
                WebRootPath = Path.Combine(serverFolder, "wwwroot")
            });

            builder.WebHost.UseStaticWebAssets();

            // 1) Services
            builder.Services.AddSingleton<GlobalConfig>();
            builder.Services.AddSingleton<SliderHubHelpers>();
            builder.Services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.TypeInfoResolver = EfficientJsonContext.Default;
                });
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
            });

            // builder.Services.AddServerSideBlazor();

            // builder.Services.AddServerSideBlazor().AddHubOptions(options =>
            // {
            //     options.MaximumReceiveMessageSize = 102400;
            // });

            // builder.Services.AddRazorPages()
            //     .AddApplicationPart(typeof(BlazorFrontend._Imports).Assembly);

            builder.WebHost.UseUrls($"http://{globalConfig.ServerHost}:{globalConfig.ServerPort}");
            _app = builder.Build();

            // 2) Routing
            _app.UseStaticFiles();
            _app.UseWebSockets();
            _app.UseRouting();

            // 3) SignalR hubs
            _app.MapHub<SliderHub>("/slider-hub");

            // 4) API
            _app.MapGet("/api/hello", () => "Hello from LiveFeedback server!");

            // 5) Special deep link fallbacks for WebFrontends
            _app.MapFallbackToFile("{*path}", "index.html");

            // _app.UseFileServer(new FileServerOptions()
            // {
            //     FileProvider = new PhysicalFileProvider(Path.Combine(_app.Environment.ContentRootPath, "wwwroot")),
            //     RequestPath = "",
            //     EnableDefaultFiles = true,
            //     EnableDirectoryBrowsing = false,
            // });

            // _app.UseStaticFiles(new StaticFileOptions()
            // {
            //     FileProvider = new PhysicalFileProvider(Path.Combine(_app.Environment.ContentRootPath, "wwwroot")),
            //     RequestPath = ""
            // });


            _app.Logger.LogInformation("Trying to start server…");

            // _app.MapBlazorHub();

            // _app.MapFallbackToPage("/_Host");

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
            _app.Logger.LogError("Failed to start server: {EMessage}", e.Message);
        }
    }

    public async Task StopAsync()
    {
        _app.Logger.LogInformation("Stopping Server…");
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}