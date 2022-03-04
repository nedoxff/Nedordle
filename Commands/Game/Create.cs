using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Drawer;
using Nedordle.Game;
using Nedordle.Game.Handlers;
using Nedordle.Helpers;
using Nedordle.Helpers.Types;
using Serilog;

namespace Nedordle.Commands.Game;

public class Create : ExtendedCommandModule
{
    [SlashCommand("create", "Create a new wordle game.")]
    public async Task Execute(InteractionContext ctx)
    {
        await ctx.DeferAsync(true);

        if (GameDatabaseHelper.UserIsInGame(ctx.Guild.Id))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(SimpleDiscordEmbed.Error(Locale.UserInGame)));
            return;
        }

        if (!GuildDatabaseHelper.GuildExists(ctx.Guild.Id))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(SimpleDiscordEmbed.Warning(Locale.Fixing)));
            Troubleshoot.FixGuild(ctx.Guild.Id);
        }

        if (!UserDatabaseHelper.Exists(ctx.User.Id))
            Troubleshoot.FixUser(ctx.User.Id);


        var gameType = await GetGameType(ctx);
        if (string.IsNullOrEmpty(gameType)) return;

        var language = await GetLanguage(ctx);
        if (string.IsNullOrEmpty(language)) return;

        var length = 0;
        if (Locale.GameTypes[gameType].AllowDifferentLength)
            length = await GetLength(ctx, language);
        if (length == -1) return;

        var userLimit = 0;
        var multiplayer = Locale.GameTypes[gameType].IsMultiplayer;
        if (multiplayer)
            userLimit = await GetUserLimit(ctx);
        if (userLimit == -1) return;

        var attemptLimit = 0;
        if (Locale.GameTypes[gameType].AllowDifferentAttempts)
            attemptLimit = await GetAttemptLimit(ctx);
        if (attemptLimit == -2) return;

        bool dms;
        if (ctx.Channel.IsPrivate || !GuildDatabaseHelper.GetAllowCreatingChannels(ctx.Guild.Id) || multiplayer)
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
            channel = ctx.Channel.IsPrivate ? ctx.Channel : await ctx.Member.CreateDmChannelAsync();
        }
        else
        {
            var createCategory = GuildDatabaseHelper.GetCreateCategory(ctx.Guild.Id);
            channel = await ctx.Guild.CreateChannelAsync($"nedordle-{id}", ChannelType.Text,
                createCategory == 0 ? null : ctx.Guild.GetChannel(createCategory));
            await channel.AddOverwriteAsync(ctx.Guild.EveryoneRole, Permissions.None, Permissions.All,
                "Init game channel");
        }

        GameHandler? handler = gameType switch
        {
            "usual" => new CustomGameHandler
                {Channel = channel, Id = id, Language = language, Length = 5, AttemptLimit = 6, GameType = "usual"},
            "custom" => new CustomGameHandler
                {Channel = channel, Id = id, Language = language, Length = length, AttemptLimit = attemptLimit, GameType = "custom"},
            "teamrace" => new TeamRaceGameHandler
            {
                Id = id, Language = language, Length = length, UserLimit = userLimit,
                GameType = "teamrace"
            },
            "teamwork" => new TeamworkGameHandler
            {
                Id = id, Language = language, Length = length, AttemptLimit = attemptLimit, GameType = "teamwork", UserLimit = userLimit
            },
            _ => null
        };
        if (handler is null)
            throw new Exception(
                "GameHandler in the /create command was null! (was the interaction response ID invalid?)");

        Log.Information(
            "Created new game ({GameId}). Type: {GameType} | Language: {GameLanguage} | Channel: {GameChannel} | Guild: {GameGuild}",
            id,
            gameType, language, channel.Id, channel.IsPrivate ? "none (DMs)" : channel.Guild.Id);

        var stream = WordleDrawer.Generate($"[c]{id}[/]", Theme.Themes[UserDatabaseHelper.GetTheme(ctx.User.Id)]);
        stream.Seek(0, SeekOrigin.Begin);

        var builder = new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen,
                string.Format(Locale.GameTypes[gameType].IsMultiplayer ? Locale.CreateDoneMultiplayer : Locale.Done,
                    id)))
            .AddFile("id.png", stream);
        await ctx.EditResponseAsync(builder);

        GameHandler.Handlers[id] = handler;
        await handler.OnCreate(ctx.User, Locale);
        await handler.OnJoined(ctx.Channel, ctx.User, Locale);
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

    private async Task<int> GetLength(BaseContext ctx, string language)
    {
        var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent(DiscordEmoji.FromName(ctx.Client, ":point_down:"))
            .AddComponents(new DiscordSelectComponent("select_length", Locale.CreateSelectLength,
                GetLengthOptions(language))));

        var result = await message.WaitForSelectAsync(ctx.User, "select_length", TimeSpan.FromMinutes(1));
        await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        if (!result.TimedOut) return int.Parse(result.Result.Values[0]);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
        return 5;
    }

    private async Task<int> GetUserLimit(BaseContext ctx)
    {
        var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent(DiscordEmoji.FromName(ctx.Client, ":point_down:"))
            .AddComponents(new DiscordSelectComponent("select_user_limit", Locale.CreateSelectUserLimit,
                GetUserLimitOptions())));

        var result = await message.WaitForSelectAsync(ctx.User, "select_user_limit", TimeSpan.FromMinutes(1));
        await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        if (!result.TimedOut) return int.Parse(result.Result.Values[0]);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
        return -1;
    }

    private async Task<int> GetAttemptLimit(BaseContext ctx)
    {
        var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent(DiscordEmoji.FromName(ctx.Client, ":point_down:"))
            .AddComponents(new DiscordSelectComponent("select_attempt_limit", Locale.CreateSelectAttemptLimit,
                GetAttemptLimitOptions())));
        
        var result = await message.WaitForSelectAsync(ctx.User, "select_attempt_limit", TimeSpan.FromMinutes(1));
        await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        if (!result.TimedOut)
        {
            var id = result.Result.Values[0];
            switch (id)
            {
                case "unlimited":
                    return -1;
                case "custom":
                    var interaction = ctx.Client.GetInteractivity();
                    message = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(Locale.CreateEnterAttemptLimit));
                    var messageResult = await interaction.WaitForMessageAsync(m =>
                        m.Author.Id == ctx.User.Id && m.Channel.Id == message.Channel.Id && int.TryParse(m.Content, out var l) && l > 0, TimeSpan.FromMinutes(1));
                    
                    if (!messageResult.TimedOut) return int.Parse(messageResult.Result.Content);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
                    return -2;

                default:
                    return int.Parse(result.Result.Values[0]);
            }
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Error(Locale.TimedOut)));
        return -2;
    }

    private DiscordSelectComponentOption SimpleOption(string value)
    {
        return new(value, value.ToLower());
    }

    private IEnumerable<DiscordSelectComponentOption> GetUserLimitOptions()
    {
        var list = new List<DiscordSelectComponentOption>();
        for (var i = 2; i <= 10; i++)
            list.Add(SimpleOption(i.ToString()));
        return list;
    }

    private IEnumerable<DiscordSelectComponentOption> GetAttemptLimitOptions()
    {
        var list = new List<DiscordSelectComponentOption>();
        list.Add(SimpleOption(Locale.Custom));
        list.Add(SimpleOption(Locale.Unlimited));
        for(var i = 2; i <= 15; i++)
            list.Add(SimpleOption(i.ToString()));
        return list;
    }

    private IEnumerable<DiscordSelectComponentOption> GetLengthOptions(string language)
    {
        var options = LanguageDatabaseHelper.GetLengthOptions(language);
        return options.Select(pair => new DiscordSelectComponentOption(pair.Key.ToString(), pair.Key.ToString(),
            string.Format(Locale.CreateLengthDescription, pair.Value, pair.Key)));
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