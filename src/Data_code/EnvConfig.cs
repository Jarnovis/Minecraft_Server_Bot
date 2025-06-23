namespace MinecraftServerDiscordBot.Data;

using dotenv.net;
using dotenv.net.Utilities;

public class EnvConfig
{
    static EnvConfig()
    {
        DotEnv.Load();
    }

    public static string Get(string key)
    {
        var value = EnvReader.GetStringValue(key);

        return value;
    }
}