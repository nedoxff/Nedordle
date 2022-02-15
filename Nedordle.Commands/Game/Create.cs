using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Drawer;
using Nedordle.Game;
using Nedordle.Game.Handlers;
using Nedordle.Helpers;

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

        var gameType = await GetGameType(ctx);
        if (string.IsNullOrEmpty(gameType)) return;

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
            .WithContent("Creating the game.."));

        var id = RandomExtensions.New().NextUpperString(10);

        DiscordChannel channel;
        if (dms)
        {
            channel = ctx.Channel;
        }
        else
        {
            var createCategory = GuildDatabaseHelper.GetCreateCategory(ctx.Guild.Id);
            channel = await ctx.Guild.CreateChannelAsync($"nedordle-{id}", ChannelType.Text,
                createCategory == 0 ? null : ctx.Guild.GetChannel(createCategory));
        }

        switch (gameType)
        {
            case "usual":
                var handler = new RegularGameHandler
                {
                    Channel = channel,
                    Id = id
                };
                break;
        }

        var stream = NewGameDrawer.Generate(id);
        stream.Seek(0, SeekOrigin.Begin);

        var builder = new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelGreen, $"Here is your game ID: `{id}`"))
            .AddFile("id.png", stream);
        await ctx.EditResponseAsync(builder);
    }

    private async Task<string> GetGameType(BaseContext ctx)
    {
        await ctx.CreateResponseAsync("Please wait..");

        var interactivity = ctx.Client.GetInteractivity();

        var builder = new DiscordFollowupMessageBuilder()
            .WithContent("​")
            .AddComponents(new DiscordSelectComponent("game_type", "Select the game type..", GetGameTypeOptions()));

        var followUp = await ctx.FollowUpAsync(builder);
        var result = await interactivity.WaitForSelectAsync(followUp, ctx.User, "game_type", TimeSpan.FromMinutes(1));
        await followUp.DeleteAsync();
        if (!result.TimedOut) return result.Result.Values[0];
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Error("Timed out!")));
        return "";
    }

    private async Task<bool?> GetPlayInDMs(BaseContext ctx)
    {
        var builder = new DiscordFollowupMessageBuilder()
            .AddEmbed(SimpleDiscordEmbed.Colored(SimpleDiscordEmbed.PastelYellow, "Would you like to play in DMs?"))
            .AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "yes", "Yes"),
                new DiscordButtonComponent(ButtonStyle.Danger, "no", "No"));

        var followUp = await ctx.FollowUpAsync(builder);

        var result = await followUp.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(1));
        await followUp.DeleteAsync();
        if (!result.TimedOut) return result.Result.Id == "yes";
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Error("Timed out!")));
        return null;
    }

    private static IEnumerable<DiscordSelectComponentOption> GetGameTypeOptions()
    {
        return GameTypes.Types.Select(x => new DiscordSelectComponentOption(x.Value.Name, x.Key,
            x.Value.ShortDescription, false, new DiscordComponentEmoji(x.Value.Emoji))).ToList();
    }
}