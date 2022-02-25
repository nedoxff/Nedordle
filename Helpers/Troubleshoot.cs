using Nedordle.Database;

namespace Nedordle.Helpers;

public class Troubleshoot
{
    public static void FixUser(ulong id)
    {
        UserDatabaseHelper.Add(id);
    }

    public static void FixGuild(ulong id)
    {
        GuildDatabaseHelper.Add(id);
    }
}