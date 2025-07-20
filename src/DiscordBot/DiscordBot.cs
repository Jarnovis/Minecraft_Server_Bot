namespace MinecraftServerDiscordBot.DiscordBot;

using Discord;
using Discord.WebSocket;
using MinecraftServerDiscordBot.Data;
using MinecraftServerDiscordBot.Commands;
using CoreRCON;
using System.Net;
using MinecraftServerDiscordBot.BackgroundTaskss;

public class DiscordBot
{
    private static readonly DiscordSocketClient _client;
    private static readonly string _botToken = EnvConfig.Get("DISCORD_TOKEN");
    private static ITextChannel _targetChannel;
    private readonly object _shutdownLock = new object();
    private static ServerLogs? _serverLogs;
    private static CancellationTokenSource _cts = new CancellationTokenSource();

    static DiscordBot()
    {
        _client = _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.MessageContent
        });

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            _cts.Cancel();
        };
        
    }

    public static void SetTargetChannel(ITextChannel channel)
    {
        _targetChannel = channel;
    }

    public static async Task StartAsync()
    {
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += HandleMessageAsync;

        await _client.LoginAsync(TokenType.Bot, _botToken);
        await _client.StartAsync();

        await Task.Delay(10000);
        _ = new BackgroundTasks(_cts.Token);

        await Task.Delay(-1);
    }

    private static Task LogAsync(LogMessage message)
    {
        return Task.CompletedTask;
    }

    private static async Task ReadyAsync()
    {
        Console.WriteLine($"✅ Connected as -> [{_client.CurrentUser}]");

        if (_serverLogs != null)
            return;

        foreach (var guild in _client.Guilds)
        {
            var channel = await Channel.ChannelExistens(guild, "minecraft");
            _targetChannel = channel;
            _serverLogs = new ServerLogs(_client, channel);
        }
    }


    public static async Task SendDiscordMessage(SocketMessage message)
    {
        if (_targetChannel != null)
        {
            await _targetChannel.SendMessageAsync(message.ToString());
        }
    }

    public static async Task SendDiscordMessage(string message)
    {
        if (_targetChannel != null)
        {
            await _targetChannel.SendMessageAsync(message.ToString());
        }
        else
        {
            Console.WriteLine($"target channel ({_targetChannel}) is null");
            await ReadyAsync();
        }
    }

    private static async Task HandleMessageAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        if (message.Content.StartsWith("!mc_bot help"))
            await message.Channel.SendMessageAsync("Available commands: !mc OP command");
    }
}