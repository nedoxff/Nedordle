using System.Text;
using dotenv.net;

namespace Nedordle.Core;

/// <summary>
/// Config class which contains required information to start the bot.
/// </summary>
public class Config
{
    /// <summary>
    /// The dictionary containing the names and values of variables.
    /// </summary>
    private Dictionary<string, string> _objects = new();

    /// <summary>
    /// Load variables from the .env file.
    /// <example>
    /// var config = new Config();
    /// config.LoadFromEnvironment();
    /// </example>
    /// </summary>
    public void LoadFromEnvironment()
    {
        DotEnv.Fluent()
            .WithEncoding(Encoding.UTF8)
            .WithExceptions()
            .WithTrimValues()
            .WithOverwriteExistingVars()
            .Load();
        _objects = (Dictionary<string, string>) DotEnv.Read();
    }

    /// <summary>
    /// Get a variable with the [] syntax.
    /// <example>config["TOKEN"]</example>
    /// </summary>
    /// <param name="name">Name of the environment variable</param>
    public string this[string name]
    {
        get => _objects[name];
        set => _objects[name] = value;
    }
}