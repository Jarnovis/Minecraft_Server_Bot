namespace MinecraftServerDiscordBot.BackgroundTaskss;

using System.Collections.Specialized;
using System.Drawing;
using System.Threading;
using CoreRCON;
using Discord;
using MinecraftServerDiscordBot;
using MinecraftServerDiscordBot.Commands;
using MinecraftServerDiscordBot.Data;

public class BackgroundTasks
{
    private static CancellationToken _ct { get; set; }
    public BackgroundTasks(CancellationToken ct)
    {
        _ct = ct;

        _ = AutoSave();
        _ = CloseServer();
    }

    public static async Task Save()
    {
        try
        {
            await DiscordBot.DiscordBot.SendDiscordMessage($"💾 The Minecraft server ({EnvConfig.Get("PUBLIC_SERVER_IP")}:{EnvConfig.Get("PUBLIC_SERVER_PORT")}) has just saved!");
            await CustomRcon.rcon.SendCommandAsync("save-all");
        }
        catch (Exception ex)
        {
            await DiscordBot.DiscordBot.SendDiscordMessage($"Error saving the game:\n{ex.Message}");
        }
    }

    private static async Task AutoSave()
    {
        while (!_ct.IsCancellationRequested)
        {
            var response = await CustomRcon.rcon.SendCommandAsync("list");
            IList<string> currentPlayers = ServerLogs.ParsePlayerList(response);

            if (currentPlayers.Count > 0) await Save();

            await Task.Delay(900000, _ct).ContinueWith(t => { });
        }
    }

    private static async Task CloseServer()
    {
        TimeSpan time = DateTime.Now.TimeOfDay;
        TimeSpan startTime = TimeSpan.Parse(EnvConfig.Get("SERVER_OPENINGS_TIME"));
        TimeSpan closingTime = TimeSpan.Parse(EnvConfig.Get("SERVER_CLOSING_TIME"));

        TimeSpan[] reminders = [
            TimeSpan.FromMinutes(59),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(1)
        ];

        Dictionary<int, string> timeReminders = new Dictionary<int, string>
        {
            { 59, "5️⃣9️⃣" },
            { 30, "3️⃣0️⃣" },
            { 15, " 1️⃣5️⃣" },
            { 5, "5️⃣" },
            { 1, " 1️⃣" }

        };

        while (!_ct.IsCancellationRequested)
        {
            TimeSpan timeNow = DateTime.Now.TimeOfDay;
            timeNow = new TimeSpan(timeNow.Hours, timeNow.Minutes, 0);

            foreach (TimeSpan reminder in reminders)
            {
                if (timeNow == (closingTime - reminder))
                {
                    string message = $"Server closing in {reminder.Minutes} Minutes";
                    string discordMessage = $"Server closing in {timeReminders[reminder.Minutes]} minutes";

                    await CustomRcon.rcon.SendCommandAsync($"say {message}");
                    await DiscordBot.DiscordBot.SendDiscordMessage(discordMessage);
                }

                if (timeNow == closingTime)
                {
                    await CustomRcon.rcon.SendCommandAsync($"Server will be open tomorrow at {startTime}");
                    await ServerLogs.HandleShutdownAsync();
                    await DiscordBot.DiscordBot.SendDiscordMessage($"Server will be open tomorrow at {startTime}");
                }
            }

            await Task.Delay(60000, _ct).ContinueWith(t => { });
        }
    }
}