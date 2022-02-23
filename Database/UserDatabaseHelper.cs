using Serilog;

namespace Nedordle.Database;

public class UserDatabaseHelper
{
    public static bool Exists(ulong id)
    {
        Log.Debug("Checking for user with ID {UserId}", id);
        return DatabaseController.ExecuteScalar<long>($"select exists(select 1 from users where id = {id})") == 1;
    }

    public static void Add(ulong id)
    {
        Log.Debug("Adding user (with default values) with ID {UserId}", id);
        DatabaseController.ExecuteNonQuery($"replace into users(id, games, level, theme) values({id}, 0, 0, 'default')");
    }

    public static string GetTheme(ulong id)
    {
        Log.Debug("Getting theme for user with ID {UserId}", id);
        return DatabaseController.ExecuteScalar<string>($"select theme from users where id = {id}")!;
    }
}