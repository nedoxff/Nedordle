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
        Connection.Open();
    }

    public static void ThrowIfNull()
    {
        if (Connection == null)
            throw new Exception("The connection with the database was not opened!");
    }

    private static SQLiteCommand CreateCommand(string query, params object[] args)
    {
        ThrowIfNull();
        var command = Connection.CreateCommand();
        command.CommandText = string.Format(query, args);
        return command;
    }

    public static int ExecuteNonQuery(string query, params object[] args)
    {
        ThrowIfNull();
        var command = CreateCommand(query, args);
        var changed = command.ExecuteNonQuery();
        return changed;
    }

    public static T? ExecuteScalar<T>(string query, params object[] args)
    {
        ThrowIfNull();
        var command = CreateCommand(query, args);
        var obj = command.ExecuteScalar();
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
        ThrowIfNull();
        var result = new List<string>();
        var reader = ExecuteReader("select name from sqlite_master where type = 'table' order by 1");
        while (reader.Read())
            result.Add(reader.GetString(0));
        reader.Close();
        return result;
    }

    public static void DropTable(string name)
    {
        ExecuteNonQuery($"drop table if exists '{name}'");
    }
}