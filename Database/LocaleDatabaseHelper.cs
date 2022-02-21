using Nedordle.Helpers.Types;
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
            Log.Debug("Opened connection with \"database.db\"");
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
            Log.Debug("Loaded locale \"{Name}\"", name);
        }

        reader.Close();
        DatabaseController.Connection!.Close();
    }
}