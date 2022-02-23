using DSharpPlus.Entities;
using Nedordle.Game;
using Newtonsoft.Json;
using Serilog;

namespace Nedordle.Database;

public class GameDatabaseHelper
{
    public static void UpdateGame(GameHandler handler)
    {
        Log.Debug("Updating game with ID {GameId}", handler.Id);
        var players = JsonConvert.SerializeObject(handler.Players.Select(x => x.UserId).ToList());
        var playerData = JsonConvert.SerializeObject(handler.Players.ToDictionary(x => x.UserId, x => x));
        var info = JsonConvert.SerializeObject(handler.Info);
        DatabaseController.ExecuteNonQuery(
            "replace into games(id, channel, guild, type, players, player_data, language, locale, info) values('{0}', {1}, {2}, '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')",
            handler.Id, handler.Channel.Id, handler.Channel.GuildId ?? 0, handler.GameType, players, playerData, handler.Language,
            handler.LocaleString, info);
    }

    public static string GetGameFromInput(DiscordChannel channel, DiscordUser user)
    {
        var reader = DatabaseController.ExecuteReader($"select distinct id, players from games where channel = {channel.Id}");
        if (!reader.Read())
        {
            reader.Close();
            return "";
        }
        
        var id = reader.GetString(0);
        var players = reader.GetString(1);

        reader.Close();
        return (JsonConvert.DeserializeObject<ulong[]>(players) ?? throw new InvalidOperationException()).Contains(user.Id) ? id : "";
    }

    public static void RemoveGame(string id)
    {
        Log.Information("Removing game with ID {GameId}", id);
        DatabaseController.ExecuteNonQuery($"delete from games where id = '{id}'");
        GameHandler.Handlers.Remove(id);
    }
}