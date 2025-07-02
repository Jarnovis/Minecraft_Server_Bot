using System.Runtime.Loader;
using CoreRCON;
using Discord;
using Discord.WebSocket;
using MinecraftServerDiscordBot.Data;
using MinecraftServerDiscordBot.DiscordBot;

namespace MinecraftServerDiscordBot.Commands;

public class ServerLogs : IDisposable
{
    private readonly RCON _rcon;
    private readonly DiscordSocketClient _client;
    private readonly ITextChannel _targetChannel;
    private readonly string _logFilePath = EnvConfig.Get("LOG_FILE_PATH");

    private List<string> _players = new List<string>();
    private CancellationTokenSource _cts = new CancellationTokenSource();

    private Task _monitorPlayersTask;
    private Task _watchAsyncTask;
    private bool _shutdownInitiated = false;
    private readonly object _shutdownLock = new object();

    public ServerLogs(RCON rcon, DiscordSocketClient client, ITextChannel targetChannel)
    {
        _rcon = rcon;
        _client = client;
        _targetChannel = targetChannel;
        _monitorPlayersTask = MonitorPlayers(_cts.Token);
        _watchAsyncTask = WatchAsync(_cts.Token);

        DiscordBot.DiscordBot.SendDiscordMessage($"🟢 Server ({EnvConfig.Get("PUBLIC_SERVER_IP")}:{EnvConfig.Get("PUBLIC_SERVER_PORT")}) is online").GetAwaiter().GetResult();

        AssemblyLoadContext.Default.Unloading += ctx => OnShutdown();
        AppDomain.CurrentDomain.ProcessExit += (s, e) => OnShutdown();

        Console.CancelKeyPress += async (sender, e) =>
        {
            e.Cancel = true;
            OnShutdown();
            Environment.Exit(0);
        };
    }

    public void Dispose()
    {
        _cts.Cancel();

        Task.WaitAll(new[] { _monitorPlayersTask, _watchAsyncTask }, TimeSpan.FromSeconds(10));

        HandleShutdownAsync().GetAwaiter().GetResult();
    }

    private List<string> ParsePlayerList(string response)
    {
        var parts = response.Split(':');

        if (parts.Length < 2) return new List<string>();

        var players = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return players.ToList();
    }

    private async Task MonitorPlayers(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var response = await _rcon.SendCommandAsync("list");
                var currentPlayers = ParsePlayerList(response);

                var joined = currentPlayers.Except(_players).ToList();
                var left = _players.Except(currentPlayers).ToList();

                foreach (var player in joined)
                    await DiscordBot.DiscordBot.SendDiscordMessage($"👋🏼 {player} joined the server ({EnvConfig.Get("PUBLIC_SERVER_IP")}:{EnvConfig.Get("PUBLIC_SERVER_PORT")})");

                foreach (var player in left)
                    await DiscordBot.DiscordBot.SendDiscordMessage($"🫡 {player} left the server ({EnvConfig.Get("PUBLIC_SERVER_IP")}:{EnvConfig.Get("PUBLIC_SERVER_PORT")})");

                _players = currentPlayers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Monitor error: {ex.Message}");
            }

            await Task.Delay(5000, ct).ContinueWith(t => { });
        }
    }

    private async Task WatchAsync(CancellationToken ct)
    {
        using var stream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        stream.Seek(0, SeekOrigin.End);

        while (!ct.IsCancellationRequested)
        {
            string? line = await reader.ReadLineAsync();

            if (line != null)
            {
                line = line.ToLower();
                if (line.Contains("saved") || line.Contains("saving") || line.Contains("save"))
                {
                    await DiscordBot.DiscordBot.SendDiscordMessage($"💾 The Minecraft server ({EnvConfig.Get("PUBLIC_SERVER_IP")}:{EnvConfig.Get("PUBLIC_SERVER_PORT")}) has just saved!");
                }
            }
            else
            {
                await Task.Delay(1000, ct).ContinueWith(t => { });
                reader.DiscardBufferedData();
                stream.Seek(stream.Position, SeekOrigin.Begin);
            }
        }
    }
    private void OnShutdown()
    {
        lock (_shutdownLock)
        {
            if (_shutdownInitiated) return;
            _shutdownInitiated = true;
        }

        Dispose();
    }

    private async Task HandleShutdownAsync()
    {
        await DiscordBot.DiscordBot.SendDiscordMessage($"🟠 Closing server {EnvConfig.Get("PUBLIC_SERVER_IP")}:{EnvConfig.Get("PUBLIC_SERVER_PORT")}.");
        await _rcon.SendCommandAsync("save-all");
        await DiscordBot.DiscordBot.SendDiscordMessage($"💾 The Minecraft server ({EnvConfig.Get("PUBLIC_SERVER_IP")}:{EnvConfig.Get("PUBLIC_SERVER_PORT")}) has just saved!");
        await DiscordBot.DiscordBot.SendDiscordMessage($"🔴 Server {EnvConfig.Get("PUBLIC_SERVER_IP")}:{EnvConfig.Get("PUBLIC_SERVER_PORT")} closed.");
    }

    private async Task DeathWatch(CancellationToken ct)
    {
        string[] container = new string[] {
            "was", "died", "drowned", "tried", "burned", "flames", "fire", "fall", "death", "cactus", "suffocated",
            "shot", "killed", "blown", "blew", "slain", "fell"
        };

        using var stream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        stream.Seek(0, SeekOrigin.End);

        while (!ct.IsCancellationRequested)
        {
            string? line = await reader.ReadLineAsync();

            if (line != null)
            {
                line = line.ToLower();

                foreach (string s in container)
                {
                    if (line.Contains(s))
                    {
                        await DiscordBot.DiscordBot.SendDiscordMessage($"💀 {line}");
                    }
                }
            }
            else
            {
                await Task.Delay(1000, ct).ContinueWith(t => { });
                reader.DiscardBufferedData();
                stream.Seek(stream.Position, SeekOrigin.Begin);
            }
        }
    }
}