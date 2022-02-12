using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Nedordle.Helpers;

namespace Nedordle.Commands.General;

public class Ping: ExtendedCommandModule
{
    private DiscordColor[] Colors =
        {DiscordColor.Green, DiscordColor.DarkGreen, DiscordColor.Yellow, DiscordColor.Orange, DiscordColor.Red, DiscordColor.DarkRed};
    
    [SlashCommand("ping", "See if the bot is online.")]
    public async Task Execute(InteractionContext ctx)
    {
        var waitEmbed = new DiscordEmbedBuilder()
            .WithDescription(RandomExtensions.New().NextElement(Locale.PingBeforeMessages))
            .WithColor(DiscordColor.Gold)
            .Build();
        
        var time = DateTime.Now;
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AddEmbed(waitEmbed));
        var difference = (DateTime.Now - time).Milliseconds;

        var color = DiscordColor.Black;
        for(var i = 0; i < Colors.Length; i++)
            if (difference >= 150 * i && difference <= 150 * (i + 1))
            {
                color = Colors[i];
                break;
            }

        var resultEmbed = new DiscordEmbedBuilder()
            .WithTitle(Locale.Ping)
            .WithColor(color)
            .WithDescription(
                $"**{RandomExtensions.New().NextElement(Locale.PingAfterMessages)}**\n{Locale.PingClientLatency}: `{ctx.Client.Ping}{Locale.Milliseconds}`\n{Locale.PingMessageLatency}: `{difference}{Locale.Milliseconds}`")
            .Build();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(resultEmbed));
    }
}