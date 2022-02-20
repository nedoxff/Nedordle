using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Drawer;
using Nedordle.Game;
using Nedordle.Game.Handlers;
using Nedordle.Helpers;
using Serilog;

namespace Nedordle.Commands.Game;

public class Create : ExtendedCommandModule
{
    [SlashCommand("create", "Create a new wordle game.")]
    public async Task Execute(InteractionContext ctx)
    {
        if (!GuildDatabaseHelper.GuildExists(ctx.Guild.Id))
        {
            await ctx.CreateResponseAsync(
                SimpleDiscordEmbed.Error("Your server is not in the database! Please run `/troubleshoot`."));
            return;
        }

        await ctx.DeferAsync(true);

        var gameType = await GetGameType(ctx);
        if (string.IsNullOrEmpty(gameType)) return;

        var language = await GetLanguage(ctx);

        bool dms;
        if (ctx.Channel.IsPrivate)
        {
            dms = true;
        }
        else
        {
            var resultDms = await GetPlayInDMs(ctx);
            if (resultDms == null) return;
            dms = (bool) resultDms;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, Locale.CreateCreating)));

        var id = RandomExtensions.New().NextUpperString(10);

        DiscordChannel channel;
        if (dms || !GuildDatabaseHelper.GetAllowCreatingChannels(ctx.Guild.Id))
        {
            channel = ctx.Channel;
        }
        else
        {
            var createCategory = GuildDatabaseHelper.GetCreateCategory(ctx.Guild.Id);
            channel = await ctx.Guild.CreateChannelAsync($"nedordle-{id}", ChannelType.Text,
                createCategory == 0 ? null : ctx.Guild.GetChannel(createCategory));
        }

        GameHandler? handler = gameType switch
        {
            "usual" => new RegularGameHandler
                {Channel = channel, Id = id, Locale = Locale.ShortName, Language = language},
            _ => null
        };
        Log.Information(
            "Created new game ({GameId}). Type: {GameType} | Language: {GameLanguage} | Channel: {GameChannel}", id,
            gameType, language, channel.Id);
        handler?.Update();

        var stream = NewGameDrawer.Generate(id);
        stream.Seek(0, SeekOrigin.Begin);

        var builder = new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, $"Here is your game ID: `{id}`"))
            .AddFile("id.png", stream);
        await ctx.EditResponseAsync(builder);
    }

    private async Task<string> GetGameType(BaseContext ctx)
    {
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow, Locale.PleaseWait)));
        var interactivity = ctx.Client.GetInteractivity();

        var builder = new DiscordWebhookBuilder()
            .WithContent(DiscordEmoji.FromName(ctx.Client, ":point_down:"))
            .AddComponents(new DiscordSelectComponent("game_type", Locale.CreateSelectGameType, GetGameTypeOptions()));

        var response = await ctx.EditResponseAsync(builder);
        var result = await interactivity.WaitForSelectAsync(response, ctx.User, "game_type", TimeSpan.FromMinutes(1));
        await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        if (!result.TimedOut) return result.Result.Values[0];
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
        return "";
    }

    private async Task<bool?> GetPlayInDMs(BaseContext ctx)
    {
        var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow, Locale.CreateDms))
            .AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "yes", Locale.Yes),
                new DiscordButtonComponent(ButtonStyle.Danger, "no", Locale.No)));

        var result = await message.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(1));
        await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        if (!result.TimedOut) return result.Result.Id == "yes";
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
        return null;
    }

    private async Task<string> GetLanguage(BaseContext ctx)
    {
        var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent(DiscordEmoji.FromName(ctx.Client, ":point_down:"))
            .AddComponents(new DiscordSelectComponent("select_language", Locale.CreateSelectLanguage,
                GetLanguageOptions())));

        var result = await message.WaitForSelectAsync(ctx.User, "select_language", TimeSpan.FromMinutes(1));
        await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        if (!result.TimedOut) return result.Result.Values[0];
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
        return "";
    }

    private IEnumerable<DiscordSelectComponentOption> GetGameTypeOptions()
    {
        return Locale.GameTypes.Select(x => new DiscordSelectComponentOption(x.Value.Name, x.Key,
            x.Value.ShortDescription, false, new DiscordComponentEmoji(x.Value.Emoji))).ToList();
    }

    private IEnumerable<DiscordSelectComponentOption> GetLanguageOptions()
    {
        return LanguageDatabaseHelper.GetLanguages()
            .Select(x => new DiscordSelectComponentOption($"{x.Native} ({x.Full})", x.Short,
                string.Format(Locale.CreateLanguageDescription, x.Native, x.Full, x.WordCount), false,
                new DiscordComponentEmoji(DiscordEmoji.FromUnicode(x.Flag))));
    }
}