using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Game;
using Nedordle.Helpers;

namespace Nedordle.Commands.Game;

public class Join : ExtendedCommandModule
{
    [SlashCommand("join", "Join a Nedordle game.")]
    public async Task Execute(InteractionContext ctx, [Option("ID", "ID of the Nedordle game.")] string id)
    {
        if (GameDatabaseHelper.UserIsInGame(ctx.User.Id))
        {
            await ctx.CreateResponseAsync(SimpleDiscordEmbed.Error(Locale.UserInGame));
            return;
        }

        await ctx.CreateResponseAsync(SimpleDiscordEmbed.Warning(Locale.PleaseWait), true);

        if (!UserDatabaseHelper.Exists(ctx.User.Id))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(SimpleDiscordEmbed.Warning(Locale.Fixing)));
            Troubleshoot.FixUser(ctx.User.Id);
        }

        if (!GameDatabaseHelper.Exists(id))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(SimpleDiscordEmbed.Error(string.Format(Locale.GameNotFound, id))));
            return;
        }

        await GameHandler.Handlers[id].OnJoined(ctx.Channel, ctx.User, Locale);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Success(Locale.Success)));
    }
}