namespace MinecraftServerDiscordBot.DiscordBot;

using Discord;
using Discord.WebSocket;
using MinecraftServerDiscordBot.Data;
using MinecraftServerDiscordBot.Commands;

public class DiscordBot
{
    private static readonly DiscordSocketClient _client;
    private static readonly string _botToken = EnvConfig.Get("DISCORD_TOKEN");
    private static readonly ITextChannel _targetChannel;

    static DiscordBot()
    {
        _client = _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.MessageContent
        });

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += SendDiscordMessage;

        _client.Ready += async () =>
        {
            foreach (var guild in _client.Guilds)
            {
                var channel = await Channel.ChannelExistens(guild, "minecraft");
            }
        };
    }

    public static async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _botToken);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private static Task LogAsync(LogMessage message)
    {
        return Task.CompletedTask;
    }

    private static Task ReadyAsync()
    {
        Console.WriteLine($"✅ Connected as -> [{_client.CurrentUser}]");
        return Task.CompletedTask;
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
    }
}