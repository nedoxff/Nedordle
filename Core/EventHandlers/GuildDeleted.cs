using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Nedordle.Database;
using Nedordle.Helpers;
using Serilog;

namespace Nedordle.Core.EventHandlers;

public class GuildDeleted
{
    public static async Task OnGuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
    {
        Log.Information("Kicked/banned/left from guild ({GuildId})", e.Guild.Id);
        
        var guild = e.Guild;
        var embed = new DiscordEmbedBuilder()
            .WithColor(SimpleDiscordEmbed.PastelGreen)
            .WithTimestamp(DateTime.Now)
            .WithDescription("Goodbye!");

        var channel = guild.SystemChannel == null ? guild.GetDefaultChannel() : guild.SystemChannel;
        await channel.SendMessageAsync(embed);

        GuildDatabaseHelper.RemoveGuild(guild.Id);
    }
}