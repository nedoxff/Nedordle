using System.Data.SQLite;

namespace Nedordle.Database;

public class DatabaseController
{
    public static SQLiteConnection? Connection;

    public static void Create(string filename)
    {
        SQLiteConnection.CreateFile(filename);
    }

    public static void Open(string file)
    {
        Connection = new SQLiteConnection($"Data Source = {file}");
    }

    private static SQLiteCommand CreateCommand(string query, params object[] args)
    {
        Connection.Open();
        var command = Connection.CreateCommand();
        command.CommandText = string.Format(query, args);
        return command;
    }

    public static int ExecuteNonQuery(string query, params object[] args)
    {
        var command = CreateCommand(query, args);
        var changed = command.ExecuteNonQuery();
        Connection.Close();
        return changed;
    }

    public static T? ExecuteScalar<T>(string query, params object[] args)
    {
        var command = CreateCommand(query, args);
        var obj = command.ExecuteScalar();
        Connection.Close();
        return (T?) obj;
    }

    public static SQLiteDataReader ExecuteReader(string query, params object[] args)
    {
        var command = CreateCommand(query, args);
        var reader = command.ExecuteReader();
        return reader;
    }

    public static List<string> GetTables()
    {
        var result = new List<string>();
        var reader = ExecuteReader("select name from sqlite_master where type = 'table' order by 1");
        while (reader.Read())
            result.Add(reader.GetString(0));
        reader.Close();
        Connection.Close();
        return result;
    }

    public static void DropTable(string name)
    {
        ExecuteNonQuery($"drop table if exists '{name}'");
    }
}