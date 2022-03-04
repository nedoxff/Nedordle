using DSharpPlus.Entities;

namespace Nedordle.Helpers.Game;

public class MultiplayerHelper
{
    public static async Task<DiscordChannel> GetPrivateChannel(SnowflakeObject user, DiscordChannel caller)
    {
        if (caller.IsPrivate) return caller;
        var member = caller.Guild.Members[user.Id];
        return await member.CreateDmChannelAsync();
    }
}