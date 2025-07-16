
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
    private DiscordSocketClient _client { get; set; }
    private RCON _rcon { get; set; }
    private CancellationTokenSource _cts { get; set; }
    private ServerLogs? _serverLogs { get; set; }

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
        var server_ip = IPAddress.Parse(EnvConfig.Get("RCON_HOST"));
        int rcon_port = Convert.ToInt32(EnvConfig.Get("RCON_PORT"));
        var end_point = new IPEndPoint(server_ip, rcon_port);
        string rcon_password = EnvConfig.Get("RCON_PASSWORD");

        _rcon = new RCON(end_point, rcon_password);
        await _rcon.ConnectAsync();

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
