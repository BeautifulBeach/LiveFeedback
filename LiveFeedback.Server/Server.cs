using LiveFeedback.Server.Services.SignalR;
using LiveFeedback.Shared;
using LiveFeedback.Shared.Enums;

namespace LiveFeedback.Server;

public class Server
{
    private WebApplication _app = null!;

    public async Task StartAsync(Mode mode = Mode.Local)
    {
        try
        {
            GlobalConfig globalConfig = new();
            WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
            {
                WebRootPath = globalConfig.WwwRootPath,
            });
            builder.Services.AddSingleton<GlobalConfig>();
            builder.Services.AddSingleton<SliderHubHelpers>();
            builder.WebHost.UseUrls($"http://{globalConfig.ServerHost}:{globalConfig.ServerPort}");
            builder.Services.AddSignalR();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
            });

            builder.Services.AddServerSideBlazor().AddHubOptions(options =>
            {
                options.MaximumReceiveMessageSize = 102400;
            });

            builder.Services.AddRazorPages()
                .AddApplicationPart(typeof(LiveFeedback.BlazorFrontend._Imports).Assembly);

            _app = builder.Build();
            _app.Logger.LogInformation("Trying to start server…");

            _app.UseStaticFiles();
            _app.UseRouting();


            _app.UseWebSockets();
            _app.MapBlazorHub();
            _app.MapHub<SliderHub>("/slider-hub");

            _app.MapFallbackToPage("/_Host");
            _app.MapGet("/api/hello", () => "Hello from LiveFeedback server!");

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
            if (_app is null)
            {
                Console.WriteLine($"Failed to start server: {e}");
            }
            else
            {
                _app.Logger.LogError("Failed to start server: {EMessage}", e.Message);
            }
        }
    }

    public async Task StopAsync()
    {
        _app.Logger.LogInformation("Stopping Server…");
        await _app.StopAsync();
    }
}