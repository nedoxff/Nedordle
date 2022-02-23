using System.Data.Entity.Core.Metadata.Edm;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Nedordle.Database;
using Nedordle.Helpers;
using Serilog;

namespace Nedordle.Core.EventHandlers;

public class GuildCreated
{
    public static async Task OnGuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        Log.Information("Joined new guild ({GuildId})", e.Guild.Id);
        
        var guild = e.Guild;
        var embed = new DiscordEmbedBuilder()
            .WithColor(SimpleDiscordEmbed.PastelYellow)
            .WithTitle("Hello!")
            .WithDescription("Give me a second while I add your server to the database..")
            .WithTimestamp(DateTime.Now);

        var channel = guild.SystemChannel == null ? guild.GetDefaultChannel() : guild.SystemChannel;
        var message = await channel.SendMessageAsync(embed);

        GuildDatabaseHelper.Add(guild.Id);

        embed = embed
            .WithColor(SimpleDiscordEmbed.PastelGreen)
            .WithTitle("Done!")
            .WithDescription("Enjoy playing Wordle! :D");
        await message.ModifyAsync(embed.Build());
    }
}