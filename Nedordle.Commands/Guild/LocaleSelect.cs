using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Helpers;

namespace Nedordle.Commands.Guild;

public class LocaleSelect: ExtendedCommandModule
{
    [SlashCommand("locale", "Set the language of the bot on this server.")]
    public async Task Execute(InteractionContext ctx)
    {
        if (!GuildDatabaseHelper.GuildExists(ctx.Guild.Id))
        {
            await ctx.CreateResponseAsync(SimpleDiscordEmbed.Error("Your server is not in the database! Please run `/troubleshoot`."));
            return;
        }
        
        await ctx.CreateResponseAsync(SimpleDiscordEmbed.Colored(DiscordColor.Gold, "Select the new locale in the dropdown menu below."));
        var builder = new DiscordFollowupMessageBuilder()
            .WithContent(DiscordEmoji.FromName(ctx.Client, ":point_down:"))
            .AddComponents(new DiscordSelectComponent("locale_select", "Select the new language..", GetLocales()));
        var message = await ctx.FollowUpAsync(builder);

        var result = await message.WaitForSelectAsync(ctx.User, "locale_select");
        if (result.TimedOut)
        {
            await message.DeleteAsync();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(SimpleDiscordEmbed.Error("Timed out!")));
        }
        else
        {
            await message.DeleteAsync();
            GuildDatabaseHelper.SetLocale(ctx.Guild.Id, result.Result.Values[0]);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(SimpleDiscordEmbed.Success($"Successfully changed the server's language to `{result.Result.Values[0]}`.")));
        }
    }

    private static List<DiscordSelectComponentOption> GetLocales() => Helpers.Types.Locale.Locales.Select(locale => new DiscordSelectComponentOption($"{locale.Value.FullName} ({locale.Value.NativeName})", locale.Value.ShortName, null, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode(locale.Value.Flag)))).ToList();
}