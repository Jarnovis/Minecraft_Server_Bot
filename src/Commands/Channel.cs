using Discord;
using Discord.WebSocket;

namespace MinecraftServerDiscordBot.Commands;

public class Channel
{
    static Channel()
    {}

    public static async Task<ITextChannel> ChannelExistens(SocketGuild guild, string channelName)
    {
        var channel = guild.TextChannels.FirstOrDefault(c => c.Name == channelName);

        if (channel != null) return channel;

        var newChannel = await guild.CreateTextChannelAsync(channelName);
        return newChannel;
    }
}