using DSharpPlus;
using DSharpPlus.EventArgs;
using Nedordle.Database;
using Nedordle.Game;
using Serilog;

namespace Nedordle.Core.EventHandlers;

public static class MessageCreated
{
    public static async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        var game = GameDatabaseHelper.GetGameFromInput(e.Channel, e.Author);
        if (!string.IsNullOrEmpty(game))
        {
            Log.Debug("Detected input for game with ID {GameId} from {UserId} ({Username})", game, e.Author.Id,
                e.Author.Username);
            if (!GameHandler.Handlers.ContainsKey(game))
                Log.Error("Could not find game with ID {GameId} in GameHandler.Handlers! (did the bot restart?)", game);
            else
                new Task(() =>
                        GameHandler.Handlers[game].OnInput(e.Author, e.Message.Content.ToLower()).GetAwaiter()
                            .GetResult())
                    .Start();
        }
    }
}