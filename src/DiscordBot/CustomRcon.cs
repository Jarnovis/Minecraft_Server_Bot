using System.Net;
using CoreRCON;
using MinecraftServerDiscordBot.Data;

namespace Discord;

public class CustomRcon
{
    public static RCON rcon { private set; get; }

    public async static Task SetRecon()
    {
        while (rcon == null || !rcon.Connected)
        {
            try
            {
                var server_ip = IPAddress.Parse(EnvConfig.Get("RCON_HOST"));
                int rcon_port = Convert.ToInt32(EnvConfig.Get("RCON_PORT"));
                var end_point = new IPEndPoint(server_ip, rcon_port);
                string rcon_password = EnvConfig.Get("RCON_PASSWORD");

                rcon = new RCON(end_point, rcon_password);
                await rcon.ConnectAsync();

                Console.WriteLine("RCON Connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RCON connect failed: {ex.Message}");
                await Task.Delay(5000);
            }
        }
    }
}