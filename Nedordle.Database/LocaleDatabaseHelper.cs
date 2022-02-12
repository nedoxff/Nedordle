using System.Data.SQLite;
using Nedordle.Core.Types;
using Newtonsoft.Json;
using Serilog;

namespace Nedordle.Database;

public class LocaleDatabaseHelper
{
    public static void LoadLocales()
    {
        if (DatabaseController.Connection == null)
        { 
            DatabaseController.Open("database.db");
            Log.Debug("Opened connection with \"database.db\".");
        }

        var reader = DatabaseController.ExecuteReader("select * from locales");
        while (reader.Read())
        {
            var name = reader.GetString(0);
            var data = reader.GetString(1);
            var locale = JsonConvert.DeserializeObject<Locale>(data);
            if (locale == null)
                throw new Exception($"Failed to parse locale \"{name}\".");
            Locale.Locales[name] = locale;
            Log.Debug($"Loaded locale \"{name}\".");
        }
        reader.Close();
        DatabaseController.Connection!.Close();
    }

    public static string? GetLocale(ulong guild)
    {
        var obj = DatabaseController.ExecuteScalar<string>($"select primary_language from guilds where id = {guild}");
        return obj;
    }
}