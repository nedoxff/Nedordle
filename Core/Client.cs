using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Nedordle.Commands.General;
using Nedordle.Core.EventHandlers;
using Nedordle.Database;
using Nedordle.UptimeServer;
using Serilog;
using Serilog.Events;

namespace Nedordle.Core;

public class Client
{
    private const string DefaultServerResponseString = "I don't know what you expect to see here.";


    private const string SerilogOutputFormat = "({Timestamp:HH:mm:ss}) [{Level}] {Message:lj}{NewLine}{Exception}";
    private static DiscordClient _client = null!;
    private static readonly string[] Files = {"latest_log.txt", "latest_log_debug.txt"};

    public static DiscordUser User => _client.CurrentUser;

    public static IReadOnlyDictionary<ulong, DiscordGuild> Guilds => _client.Guilds;

    //TODO: add command handlers
    public static async Task Start(Config config)
    {
        InitializeLogger();
        CheckResources();

        UptimeListener.Start(config.Contains("SERVER_RESPONSE_STRING")
            ? config["SERVER_RESPONSE_STRING"]
            : DefaultServerResponseString);

        var logFactory = new LoggerFactory().AddSerilog();
        _client = new DiscordClient(new DiscordConfiguration
        {
            Token = config["TOKEN"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.Guilds | DiscordIntents.DirectMessages | DiscordIntents.GuildMessages,
            LoggerFactory = logFactory,
            MinimumLogLevel = LogLevel.Debug
        });
        _client.GuildCreated += GuildCreated.OnGuildCreated;
        _client.GuildDeleted += GuildDeleted.OnGuildDeleted;
        _client.MessageCreated += MessageCreated.OnMessageCreated;

        var slash = _client.UseSlashCommands();
        slash.SlashCommandErrored += SlashCommandErrored.OnSlashCommandErrored;
        slash.RegisterCommands(Assembly.GetAssembly(typeof(Ping)), ulong.Parse(config["GUILD_ID"]));
        Log.Information("Initialized slash commands");

        _client.UseInteractivity();
        Log.Information("Initialized interactivity");


        DatabaseController.Open("database.db");
        Log.Debug("Opened connection with \"database.db\"");

        LocaleDatabaseHelper.LoadLocales();
        ThemeDatabaseHelper.LoadThemes();

        await _client.ConnectAsync();
        await Task.Delay(-1);
    }

    private static void InitializeLogger()
    {
        foreach (var file in Files)
            if (File.Exists(file))
                File.Delete(file);
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .WriteTo.Console(LogEventLevel.Debug, SerilogOutputFormat)
#else
            .WriteTo.Console(LogEventLevel.Information, SerilogOutputFormat)
#endif
            .WriteTo.File("latest_log.txt", LogEventLevel.Information, SerilogOutputFormat)
            .WriteTo.File("latest_log_debug.txt", LogEventLevel.Debug, SerilogOutputFormat)
            .MinimumLevel.Debug()
            .CreateLogger();
        Log.Information("Logger initialized");
    }

    private static void CheckResources()
    {
        Log.Information("Checking resources..");
        ThrowIfDoesNotExist("database.db");
        ThrowIfDoesNotExist("Resources");
        ThrowIfDoesNotExist("Resources/wordle_font.ttf");
        ThrowIfDoesNotExist("Resources/noto.ttf");
        Log.Information("All fine!");
    }

    private static void ThrowIfDoesNotExist(string file)
    {
        if (!Directory.Exists(file) && !File.Exists(file))
            throw new FileNotFoundException($"Requested resource \"{file}\" was not found.");
    }
}