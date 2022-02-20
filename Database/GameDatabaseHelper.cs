using Nedordle.Game;
using Newtonsoft.Json;
using Serilog;

namespace Nedordle.Database;

public class GameDatabaseHelper
{
    public static void UpdateGame(GameHandler handler)
    {
        Log.Debug("Updating game with ID {GameId}", handler.Id);
        var players = JsonConvert.SerializeObject(handler.Players.Select(x => x.Id).ToList());
        var info = JsonConvert.SerializeObject(handler.SpecificData);
        DatabaseController.ExecuteNonQuery(
            "replace into games(id, channel, guild, type, players, language, locale, specific_info) values('{0}', {1}, {2}, '{3}', '{4}', '{5}', '{6}', '{7}')",
            handler.Id, handler.Channel.Id, handler.Channel.GuildId ?? 0, handler.GameType, players, handler.Language,
            handler.Locale, info);
    }
}