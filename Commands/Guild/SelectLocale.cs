using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Nedordle.Database;
using Nedordle.Helpers;
using Nedordle.Helpers.Types;

namespace Nedordle.Commands.Guild;

public class SelectLocale : ExtendedCommandModule
{
    [SlashCommand("locale", "Set the language of the bot on this server.")]
    [SlashRequirePermissions(Permissions.Administrator)]
    public async Task Execute(InteractionContext ctx)
    {
        if (!GuildDatabaseHelper.GuildExists(ctx.Guild.Id))
        {
            await ctx.CreateResponseAsync(
                SimpleDiscordEmbed.Error("Your server is not in the database! Please run `/troubleshoot`."), true);
            return;
        }

        var builder = new DiscordInteractionResponseBuilder()
            .WithContent(DiscordEmoji.FromName(ctx.Client, ":point_down:"))
            .AddComponents(new DiscordSelectComponent("locale_select", Locale.SelectLocalePlaceholder, GetLocales()))
            .AsEphemeral();
        await ctx.CreateResponseAsync(builder);
        var message = await ctx.Interaction.GetOriginalResponseAsync();

        var result = await message.WaitForSelectAsync(ctx.User, "locale_select", TimeSpan.FromMinutes(1));
        if (result.TimedOut)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(SimpleDiscordEmbed.Error("Timed out!")));
        }
        else
        {
            GuildDatabaseHelper.SetLocale(ctx.Guild.Id, result.Result.Values[0]);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(
                    SimpleDiscordEmbed.Success(string.Format(Locale.SelectLocaleSuccess, result.Result.Values[0]))));
        }
    }

    private static List<DiscordSelectComponentOption> GetLocales()
    {
        return Locale.Locales.Select(locale =>
            new DiscordSelectComponentOption($"{locale.Value.FullName} ({locale.Value.NativeName})",
                locale.Value.ShortName, null, false,
                new DiscordComponentEmoji(DiscordEmoji.FromUnicode(locale.Value.Flag)))).ToList();
    }
}