using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Nedordle.Database;
using Nedordle.Game;
using Nedordle.Helpers;

namespace Nedordle.Commands.Game;

public class Leave: ExtendedCommandModule
{
    [SlashCommand("leave", "Leave a Nedordle game.")]
    public async Task Execute(InteractionContext ctx)
    {
        var game = GameDatabaseHelper.GetGameFromUser(ctx.User);
        if (string.IsNullOrEmpty(game))
        {
            await ctx.CreateResponseAsync(SimpleDiscordEmbed.Error(Locale.NotInGame));
            return;
        }
        
        await ctx.CreateResponseAsync(SimpleDiscordEmbed.Warning(Locale.PleaseWait), true);
        
        if (!UserDatabaseHelper.Exists(ctx.User.Id))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(SimpleDiscordEmbed.Warning(Locale.Fixing)));
            Troubleshoot.FixUser(ctx.User.Id);
        }

        await GameHandler.Handlers[game].OnLeft(ctx.Channel, ctx.User, Locale);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(SimpleDiscordEmbed.Success(Locale.Success)));
    }
}