namespace Nedordle.Database;

public class DictionaryDatabaseHelper
{
    public static string GetWord(string language, int length) => DatabaseController.ExecuteScalar<string>($"select word from dictionary_{language} where letter_count = {length} order by random() limit 1") ?? throw new InvalidOperationException();

    public static bool Exists(string language, string word) =>
        DatabaseController.ExecuteScalar<long>(
            $"select exists(select 1 from dictionary_{language} where word = '{word}')") == 1;
}