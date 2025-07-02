namespace MinecraftServerDiscordBot.BackgroundTaskss;

using System.Threading;
using CoreRCON;
using MinecraftServerDiscordBot;
using MinecraftServerDiscordBot.Data;

public class BackgroundTasks
{
    private RCON _rcon { get; set; }
    public BackgroundTasks(RCON rcon)
    {
        _rcon = rcon;
    }

    public async Task AutoSave(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
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

            await Task.Delay(900000, ct).ContinueWith(t => { });
        }
    }
}