using Serilog;

namespace Nedordle.Database;

public class GuildDatabaseHelper
{
    public static void Add(ulong id)
    {
        Log.Debug("Adding guild (with default values) with ID {GuildId}", id);
        DatabaseController.ExecuteNonQuery(
            $"replace into guilds(id, games, create_category) values({id}, 0, 0)");
    }

    public static void RemoveGuild(ulong id)
    {
        Log.Debug("Removing guild with ID {GuildId}", id);
        DatabaseController.ExecuteNonQuery($"delete from guilds where id = {id}");
    }

    public static bool GuildExists(ulong id)
    {
        Log.Debug("Checking for guild with ID {GuildId}", id);
        return DatabaseController.ExecuteScalar<long>($"select exists(select 1 from guilds where id = {id})") == 1;
    }

    public static ulong GetCreateCategory(ulong id)
    {
        Log.Debug("Checking \"CreateCategory\" for guild with ID {GuildId}", id);
        return (ulong) DatabaseController.ExecuteScalar<long>($"select create_category from guilds where id = {id}");
    }

    public static bool GetAllowCreatingChannels(ulong id)
    {
        Log.Debug("Checking \"AllowCreatingChannels\" for guild with ID {GuildId}", id);
        return DatabaseController.ExecuteScalar<long>($"select allow_creating_channels from guilds where id = {id}") ==
               1;
    }

    public static void SetCreateCategory(ulong id, ulong categoryId)
    {
        Log.Debug("Setting \"CreateCategory\" for guild with ID {GuildId} to {CategoryId}", id, categoryId);
        DatabaseController.ExecuteNonQuery($"update guilds set create_category = {categoryId} where id = {id}");
    }

    public static void SetAllowCreatingChannels(ulong id, bool allow)
    {
        var action = allow ? "Allowing" : "Disallowing";
        Log.Debug("{Action} to create channels for guild with ID {GuildId}", action, id);
        DatabaseController.ExecuteNonQuery(
            $"update guilds set allow_creating_channels = {(allow ? 1 : 0)} where id = {id}");
    }
}