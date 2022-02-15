namespace Nedordle.Database;

public class GuildDatabaseHelper
{
    public static void AddGuild(ulong id)
    {
        DatabaseController.ExecuteNonQuery(
            $"replace into guilds(id, games, create_category, primary_language) values({id}, 0, 0, 'en')");
    }

    public static void RemoveGuild(ulong id)
    {
        DatabaseController.ExecuteNonQuery($"delete from guilds where id = {id}");
    }

    public static bool GuildExists(ulong id)
    {
        return DatabaseController.ExecuteScalar<long>($"select exists(select 1 from guilds where id = {id})") == 1;
    }

    public static void SetLocale(ulong id, string locale)
    {
        DatabaseController.ExecuteNonQuery($"update guilds set primary_language = '{locale}' where id = {id}");
    }

    public static ulong GetCreateCategory(ulong id)
    {
        return (ulong) DatabaseController.ExecuteScalar<long>($"select create_category from guilds where id = {id}");
    }
}