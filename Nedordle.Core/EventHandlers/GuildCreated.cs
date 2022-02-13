using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Nedordle.Database;

namespace Nedordle.Core.EventHandlers;

public class GuildCreated
{
    public static async Task OnGuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        var guild = e.Guild;
        var embed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Gold)
            .WithTitle("Hello!")
            .WithDescription("Give me a second while I add your server to the database..")
            .WithTimestamp(DateTime.Now);

        var channel = guild.SystemChannel == null ? guild.GetDefaultChannel(): guild.SystemChannel;
        var message = await channel.SendMessageAsync(embed);
        
        GuildDatabaseHelper.AddGuild(guild.Id);

        embed = embed
            .WithColor(DiscordColor.Green)
            .WithTitle("Done!")
            .WithDescription("Enjoy playing Wordle! :D");
        await message.ModifyAsync(embed.Build());
    }
}