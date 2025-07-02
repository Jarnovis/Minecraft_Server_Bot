
using System;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using CoreRCON;
using dotenv.net;
using MinecraftServerDiscordBot.Data;
using MinecraftServerDiscordBot.Commands;
using MinecraftServerDiscordBot.BackgroundTaskss;

namespace MinecraftServerDiscordBot;

class Program
{
    private DiscordSocketClient _client;
    private RCON _rcon;
    private CancellationTokenSource _cts;
    private ServerLogs? _serverLogs;

    static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

    private Task Log(LogMessage log)
    {
        return Task.CompletedTask;
    }

    public async Task HandleMessage(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        if (message.Content.StartsWith("!mc"))
        {
            var command = message.Content.Substring(4);
            try
            {
                var response = await _rcon.SendCommandAsync(command);
                await message.Channel.SendMessageAsync($"Sent to server: `{command}`\nServer said: `{response}`");
            }
            catch (Exception ex)
            {
                await message.Channel.SendMessageAsync($"Failed to send command: {ex.Message}");
            }
        }
    }

    public async Task MainAsync()
    {
        _cts = new CancellationTokenSource();

        // _client = new DiscordSocketClient(new DiscordSocketConfig
        // {
        //     GatewayIntents = GatewayIntents.Guilds |
        //                     GatewayIntents.GuildMessages |
        //                     GatewayIntents.MessageContent
        // });

        // _client.Log += Log;
        // _client.MessageReceived += HandleMessage;

        // var discordToken = EnvConfig.Get("DISCORD_TOKEN");

        // await _client.LoginAsync(TokenType.Bot, discordToken);
        // await _client.StartAsync();

        await DiscordBot.DiscordBot.StartAsync();
    }
}
