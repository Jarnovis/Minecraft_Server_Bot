namespace MinecraftServerDiscordBot.BackgroundTaskss;

using System.Drawing;
using System.Threading;
using CoreRCON;
using MinecraftServerDiscordBot;
using MinecraftServerDiscordBot.Commands;
using MinecraftServerDiscordBot.Data;

public class BackgroundTasks
{
    private static RCON _rcon { get; set; }
    public BackgroundTasks(RCON rcon)
    {
        _rcon = rcon;
    }

    public static async Task Save()
    {
        try
        {
            await DiscordBot.DiscordBot.SendDiscordMessage($"💾 The Minecraft server ({EnvConfig.Get("PUBLIC_SERVER_IP")}:{EnvConfig.Get("PUBLIC_SERVER_PORT")}) has just saved!");
            await _rcon.SendCommandAsync("save-all");
        }
        catch (Exception ex)
        {
            await DiscordBot.DiscordBot.SendDiscordMessage($"Error saving the game:\n{ex.Message}");
        }
    }

    public async Task AutoSave(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var response = await _rcon.SendCommandAsync("list");
            IList<string> currentPlayers = ServerLogs.ParsePlayerList(response);

            if (currentPlayers.Count > 0) await Save();

            await Task.Delay(900000, ct).ContinueWith(t => { });
        }
    }
}