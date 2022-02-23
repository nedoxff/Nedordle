using Serilog;

namespace Nedordle.Database;

public struct LanguageInfo
{
    public string Short;
    public string Full;
    public string Native;
    public string Flag;
    public int WordCount;
}

public class LanguageDatabaseHelper
{
    public static IEnumerable<LanguageInfo> GetLanguages()
    {
        Log.Debug("Getting available languages");
        var reader = DatabaseController.ExecuteReader("select * from languages");

        var list = new List<LanguageInfo>();
        while (reader.Read())
            list.Add(new LanguageInfo
            {
                Short = reader.GetString(0),
                Full = reader.GetString(1),
                Native = reader.GetString(2),
                Flag = reader.GetString(3),
                WordCount = reader.GetInt32(4)
            });

        reader.Close();
        return list;
    }
}