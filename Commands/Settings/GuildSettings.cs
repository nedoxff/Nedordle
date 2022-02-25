using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Helpers;

namespace Nedordle.Commands.Settings;

[SlashCommandGroup("settings", "Change personal & server settings!")]
public partial class Settings : ExtendedCommandModule
{
    private static bool _allowCreatingChannels;
    private static ulong _createCategory;

    [SlashCommand("server", "Change settings of the server.")]
    public async Task ExecuteGuild(InteractionContext ctx)
    {
        if (GameDatabaseHelper.UserIsInGame(ctx.User.Id))
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .AddEmbed(SimpleDiscordEmbed.Error("LOCALE_USER_IN_GAME"))
                .AsEphemeral());
            return;
        }

        if (ctx.Member == null)
        {
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                .AddEmbed(SimpleDiscordEmbed.Error(Locale.ServerSettingsDms))
                .AsEphemeral());
            return;
        }

        var first = true;

        if (!GuildDatabaseHelper.GuildExists(ctx.Guild.Id))
        {
            await ctx.CreateResponseAsync(
                SimpleDiscordEmbed.Warning(Locale.Fixing), true);
            Troubleshoot.FixGuild(ctx.Guild.Id);
            first = false;
        }

        if ((ctx.Member.Permissions & Permissions.Administrator) == 0)
        {
            if (first)
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
                    .AddEmbed(SimpleDiscordEmbed.Error(Locale.NotPermitted))
                    .AsEphemeral());
            else
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(SimpleDiscordEmbed.Error(Locale.NotPermitted)));
            return;
        }

        InitGuild(ctx.Guild.Id);

        while (true)
        {
            var (embed, buttons) = GetGuildData(ctx.Guild);
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

            if (await HandleGuildAction(ctx, message, action))
                break;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow, Locale.ApplyingChanges)));

        GuildDatabaseHelper.SetAllowCreatingChannels(ctx.Guild.Id, _allowCreatingChannels);
        GuildDatabaseHelper.SetCreateCategory(ctx.Guild.Id, _createCategory);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, Locale.Done)));
    }

    private (DiscordEmbed, List<DiscordButtonComponent>) GetGuildData(DiscordGuild guild)
    {
        var categoryName = _createCategory == 0 ? Locale.None : guild.GetChannel(_createCategory).Name;

        var buttons = new List<DiscordButtonComponent>
        {
            new(ButtonStyle.Secondary, "allowCreatingChannels",
                _allowCreatingChannels
                    ? Locale.ServerSettingsDisallowCreatingChannels
                    : Locale.ServerSettingsAllowCreatingChannels),
            new(ButtonStyle.Secondary, "selectCreateCategory",
                Locale.ServerSettingsChangeCreateCategory),
            new(ButtonStyle.Success, "apply", Locale.Apply)
        };

        var description = @$"
{Locale.ServerSettingsInfoAllowCreatingChannels}: {(_allowCreatingChannels ? '✅' : '❌')}
{Locale.ServerSettingsInfoCreateCategory}: `{categoryName} ({_createCategory})`
";

        var embed = new DiscordEmbedBuilder()
            .WithTitle(Locale.ServerSettingsTitle)
            .WithColor(SimpleDiscordEmbed.PastelYellow)
            .WithDescription(description)
            .WithTimestamp(DateTime.Now)
            .Build();

        return (embed, buttons);
    }

    private void InitGuild(ulong guild)
    {
        _allowCreatingChannels = GuildDatabaseHelper.GetAllowCreatingChannels(guild);
        _createCategory = GuildDatabaseHelper.GetCreateCategory(guild);
    }

    private async Task<bool> HandleGuildAction(InteractionContext ctx, DiscordMessage message, string actionId)
    {
        switch (actionId)
        {
            case "allowCreatingChannels":
            {
                _allowCreatingChannels = !_allowCreatingChannels;
                return false;
            }
            case "selectCreateCategory":
            {
                var categories = await GetCategories(ctx.Guild);
                message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent(DiscordEmoji.FromName(ctx.Client, ":point_down:"))
                    .AddComponents(new DiscordSelectComponent("category_select",
                        Locale.ServerSettingsSelectCreateCategoryPlaceholder,
                        categories)));

                var response = await message.WaitForSelectAsync(ctx.User, "category_select", TimeSpan.FromMinutes(1));
                if (response.TimedOut)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
                    return true;
                }

                await response.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                _createCategory = ulong.Parse(response.Result.Values[0]);
                return false;
            }
            case "apply":
            {
                return true;
            }
            default:
                return false;
        }
    }

    private async Task<List<DiscordSelectComponentOption>> GetCategories(DiscordGuild guild)
    {
        var list = new List<DiscordSelectComponentOption> {new(Locale.None, "0")};
        list.AddRange((await guild.GetChannelsAsync()).Where(x => x.IsCategory)
            .Select(x => new DiscordSelectComponentOption($"#{x.Name}", x.Id.ToString(), $"ID: {x.Id}")).ToList());
        return list;
    }
}