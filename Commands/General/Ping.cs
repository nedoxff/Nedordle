using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Nedordle.Helpers;

namespace Nedordle.Commands.General;

public class Ping : ExtendedCommandModule
{
    private readonly DiscordColor[] _colors =
    {
        SimpleDiscordEmbed.PastelGreen, SimpleDiscordEmbed.PastelYellow, SimpleDiscordEmbed.PastelRed
    };

    [SlashCommand("ping", "See if the bot is online.")]
    public async Task Execute(InteractionContext ctx)
    {
        var waitEmbed = SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow,
            RandomExtensions.New().NextElement(Locale.PingBeforeMessages));

        var time = DateTime.Now;
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AddEmbed(waitEmbed)
                .AsEphemeral());

        var difference = (DateTime.Now - time).Milliseconds;

        var color = DiscordColor.Black;
        for (var i = 0; i < _colors.Length; i++)
            if (difference >= 300 * i && difference <= 300 * (i + 1))
            {
                color = _colors[i];
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