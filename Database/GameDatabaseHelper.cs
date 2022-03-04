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
        var players = JsonConvert.SerializeObject(handler.Players.Select(x => x.Key).ToList()).Replace("'", "''");
        var playerData = JsonConvert.SerializeObject(handler.Players).Replace("'", "''");
        var info = JsonConvert.SerializeObject(handler.Info).Replace("'", "''");
        DatabaseController.ExecuteNonQuery(
            "replace into games(id, type, players, player_data, language, info) values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
            handler.Id, handler.GameType, players, playerData,
            handler.Language, info);
    }

    public static string GetGameFromUser(DiscordUser user)
    {
        var reader =
            DatabaseController.ExecuteReader("select distinct id, players from games");
        if (!reader.Read())
        {
            reader.Close();
            return "";
        }

        var id = reader.GetString(0);
        var players = reader.GetString(1);

        reader.Close();
        return (JsonConvert.DeserializeObject<ulong[]>(players) ?? throw new InvalidOperationException())
            .Contains(user.Id)
                ? id
                : "";
    }

    public static void RemoveGame(string id)
    {
        Log.Information("Removing game with ID {GameId}", id);
        DatabaseController.ExecuteNonQuery($"delete from games where id = '{id}'");
        GameHandler.Handlers.Remove(id);
    }

    public static bool UserIsInGame(ulong id)
    {
        Log.Debug("Searching in games for user with ID {UserId}", id);
        var reader = DatabaseController.ExecuteReader("select players from games");
        while (reader.Read())
        {
            var data = reader.GetString(0);
            if (!JsonConvert.DeserializeObject<ulong[]>(data)!.Contains(id)) continue;
            reader.Close();
            return true;
        }

        reader.Close();
        return false;
    }

    public static bool Exists(string id)
    {
        return DatabaseController.ExecuteScalar<long>($"select exists(select 1 from games where id = '{id}')") == 1;
    }
}