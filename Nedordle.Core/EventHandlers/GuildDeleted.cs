using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Nedordle.Database;

namespace Nedordle.Core.EventHandlers;

public class GuildDeleted
{
    public static async Task OnGuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
    {
        var guild = e.Guild;
        var embed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Gray)
            .WithTimestamp(DateTime.Now)
            .WithDescription("Goodbye!");

        var channel = guild.SystemChannel == null ? guild.GetDefaultChannel() : guild.SystemChannel;
        await channel.SendMessageAsync(embed);
        
        GuildDatabaseHelper.RemoveGuild(guild.Id);
    }
}