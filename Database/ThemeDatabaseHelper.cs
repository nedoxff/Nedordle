using Nedordle.Helpers.Types;
using Newtonsoft.Json;
using Serilog;

namespace Nedordle.Database;

public class ThemeDatabaseHelper
{
    public static void LoadThemes()
    {
        Log.Information("Loading themes");

        var reader = DatabaseController.ExecuteReader("select * from themes");
        while (reader.Read())
        {
            var shortName = reader.GetString(0);
            var data = reader.GetString(1);
            Theme.Themes[shortName] =
                JsonConvert.DeserializeObject<Theme>(data) ?? throw new InvalidOperationException();
            Log.Debug("Loaded theme \"{Theme}\"", shortName);
        }

        reader.Close();
    }
}