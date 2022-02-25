using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Helpers;
using Nedordle.Helpers.Types;

namespace Nedordle.Commands.Settings;

public partial class Settings : ExtendedCommandModule
{
    private string _theme;

    [SlashCommand("user", "Change personal settings.")]
    public async Task ExecuteUser(InteractionContext ctx)
    {
        var first = true;

        if (GameDatabaseHelper.UserIsInGame(ctx.User.Id))
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .AddEmbed(SimpleDiscordEmbed.Error("LOCALE_USER_IN_GAME"))
                .AsEphemeral());
            return;
        }

        if (!UserDatabaseHelper.Exists(ctx.User.Id))
        {
            await ctx.CreateResponseAsync(SimpleDiscordEmbed.Warning(Locale.Fixing), true);
            Troubleshoot.FixUser(ctx.User.Id);
            first = false;
        }

        InitUser(ctx.User.Id);

        while (true)
        {
            var (embed, buttons) = GetUserData(ctx.User);
            DiscordMessage message;
            if (first)
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AddComponents(buttons)
                    .AsEphemeral());
                message = await ctx.GetOriginalResponseAsync();
                first = false;
            }
            else
            {
                message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(embed)
                    .AddComponents(buttons));
            }

            var result = await message.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(1));
            if (result.TimedOut)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
                break;
            }

            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
            var action = result.Result.Id;

            if (await HandleUserAction(ctx, message, action))
                break;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow, Locale.ApplyingChanges)));

        UserDatabaseHelper.SetTheme(ctx.User.Id, _theme);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, Locale.Done)));
    }

    private void InitUser(ulong id)
    {
        _theme = UserDatabaseHelper.GetTheme(id);
    }

    private async Task<bool> HandleUserAction(InteractionContext ctx, DiscordMessage message, string action)
    {
        switch (action)
        {
            case "selectTheme":
            {
                var themes = GetThemes();
                message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent(DiscordEmoji.FromName(ctx.Client, ":point_down:"))
                    .AddComponents(new DiscordSelectComponent("select_theme", Locale.UserSettingsSelectThemePlaceholder,
                        themes)));
                var response = await message.WaitForSelectAsync(ctx.User, "select_theme", TimeSpan.FromMinutes(1));
                if (response.TimedOut)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
                    return true;
                }

                await response.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                _theme = response.Result.Values[0];
                return false;
            }
            case "apply":
                return true;
            default:
                return false;
        }
    }

    private (DiscordEmbed, List<DiscordButtonComponent>) GetUserData(DiscordUser user)
    {
        var buttons = new List<DiscordButtonComponent>
        {
            new(ButtonStyle.Secondary, "selectTheme", Locale.UserSettingsChangeTheme),
            new(ButtonStyle.Success, "apply", Locale.Apply)
        };

        var description = $@"
{Locale.UserSettingsTheme}: `{_theme}`
";

        var embed = new DiscordEmbedBuilder()
            .WithTitle(Locale.UserSettingsTitle)
            .WithColor(SimpleDiscordEmbed.PastelYellow)
            .WithDescription(description)
            .WithTimestamp(DateTime.Now)
            .Build();

        return (embed, buttons);
    }

    private IEnumerable<DiscordSelectComponentOption> GetThemes()
    {
        return Theme.Themes.Select(x => new DiscordSelectComponentOption(Locale.Themes[x.Key], x.Key, null, false,
            new DiscordComponentEmoji(DiscordEmoji.FromUnicode(x.Value.CorrectEmoji))));
    }
}