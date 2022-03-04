using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using Nedordle.Database;
using Nedordle.Helpers;
using Nedordle.Helpers.Types;
using Newtonsoft.Json;
using Spectre.Console;

namespace Nedordle.Cli.DatabaseConfiguration;

public static class InteractiveConfigure
{
    public const string DataPath = "https://github.com/NedoProgrammer/NedordleData/archive/refs/heads/main.zip";

    private static string _tempFolder = "";
    private static string _dataFolder = "";

    public static void Configure()
    {
        Preconfigure();

        ConfigureDatabase();
        DownloadRepository();
        CopyResources();
        ConfigureThemes();
        ConfigureLocales();
        ConfigureLanguages();

        Cleanup();
    }

    private static void CopyResources()
    {
        AnsiConsole.Status().Spinner(Spinner.Known.Dots2).Start("Creating Resources folder..", ctx =>
        {
            if (Directory.Exists("Resources"))
                DirectoryExtensions.DeleteWithFiles("Resources");
            Directory.CreateDirectory("Resources");
            var resourcesFolder = Path.Combine(_dataFolder, "Resources");
            foreach (var file in Directory.GetFiles(resourcesFolder))
            {
                var filename = Path.GetFileName(file);
                ctx.Status($"Copying {filename}..");
                File.Copy(file, Path.Combine(Environment.CurrentDirectory, "Resources", filename));
            }
        });
    }

    private static void ConfigureThemes()
    {
        AnsiConsole.Status().Spinner(Spinner.Known.Dots2).Start("Importing themes..", ctx =>
        {
            var themesFile = Path.Combine(_dataFolder, "Themes.json");
            if (!File.Exists(themesFile))
                throw new Exception("NedordleData did not contain a Themes.json file!");
            var themes = JsonConvert.DeserializeObject<List<Theme>>(File.ReadAllText(themesFile));
            foreach (var theme in themes!)
            {
                var jsonData = JsonConvert.SerializeObject(theme).Replace("'", "''");
                DatabaseController.ExecuteNonQuery($"insert into themes(id, data) values('{theme.Id}', '{jsonData}')");
            }
        });
    }

    private static void Preconfigure()
    {
        _tempFolder = Path.Combine(Path.GetTempPath(),
            "Nedordle_DatabaseCreator_" + RandomExtensions.New().NextString(10));
        Directory.CreateDirectory(_tempFolder);
    }

    private static void Cleanup()
    {
        DirectoryExtensions.DeleteWithFiles(_tempFolder);
    }

    private static void ConfigureDatabase()
    {
        var exists = File.Exists("database.db");
        if (exists)
        {
            var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("The database already exists. What would you like to do?")
                .AddChoices("Only update the languages (i.e. dictionaries & locales) tables")
                .AddChoices("Recreate the database entirely"));
            if (action == "Recreate the database entirely")
            {
                if (DatabaseController.Connection != null)
                    DatabaseController.Connection.Close();
                File.Delete("database.db");
                exists = false;
            }

            AnsiConsole.Clear();
        }

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .Start("Configuring database file..", ctx =>
            {
                if (!exists)
                    DatabaseController.Create("database.db");

                DatabaseController.Open("database.db");

                if (exists)
                {
                    ctx.Status("Removing tables to be replaced..");
                    var tables = DatabaseController.GetTables();
                    DatabaseController.DropTable("locales");
                    DatabaseController.DropTable("languages");
                    DatabaseController.DropTable("themes");
                    foreach (var table in tables.Where(x => x.StartsWith("dictionary_")))
                        DatabaseController.DropTable(table);
                    foreach (var table in tables.Where(x => x.StartsWith("info_")))
                        DatabaseController.DropTable(table);
                    CreateLanguagesTable();
                    CreateLocalesTable();
                    CreateThemesTable();
                }
                else
                {
                    ctx.Status("Creating required tables..");
                    CreateGuildsTable();
                    CreateUsersTable();
                    CreateLanguagesTable();
                    CreateLocalesTable();
                    CreateGamesTable();
                    CreateThemesTable();
                }
            });
    }

    private static void ConfigureLocales()
    {
        AnsiConsole.Status().Spinner(Spinner.Known.Dots2).Start("Preparing to import locales..", ctx =>
        {
            var localesFolder = Path.Combine(_dataFolder, "Locales");
            if (!Directory.Exists(localesFolder))
                throw new Exception(
                    "NedoProgrammer/NedordleData repository did not contain a \"Locales\" folder!");
            foreach (var file in Directory.GetFiles(localesFolder, "*.json", SearchOption.AllDirectories))
            {
                ctx.Status($"Importing [italic yellow]{Path.GetFileNameWithoutExtension(file)}.json[/]..");
                var locale = JsonConvert.DeserializeObject<Locale>(File.ReadAllText(file));
                var noIndentation = JsonConvert.SerializeObject(locale, Formatting.None);
                DatabaseController.ExecuteNonQuery("insert into locales(id, data) values('{0}', '{1}')",
                    Path.GetFileNameWithoutExtension(file), noIndentation.Replace("'", "''"));
            }
        });
    }

    private static void DownloadRepository()
    {
        AnsiConsole.Status().Spinner(Spinner.Known.Dots2).Start("Downloading the NedordleData Github repository..",
            ctx =>
            {
                var dataPath = Path.Combine(_tempFolder, "data.zip");
                FileDownloader.DownloadFile(DataPath, dataPath);
                if (!File.Exists(dataPath))
                    throw new Exception("Failed to download the repository.");

                ctx.Status("Extracting the archive..");
                _dataFolder = Path.Combine(_tempFolder, "data", "NedordleData-main");
                ZipFile.ExtractToDirectory(dataPath, Path.Combine(_tempFolder, "data"));
            });
    }

    [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
    private static void ConfigureLanguages()
    {
        IReadOnlyList<string> files;

        files = Directory.GetFiles(_dataFolder, "*.*", SearchOption.AllDirectories).ToList();
        var supportedLanguagesFile = files.FirstOrDefault(x => Path.GetFileName(x) == "SupportedLanguages.json");
        if (string.IsNullOrEmpty(supportedLanguagesFile))
            throw new Exception("The repository did not contain a SupportedLanguages.json file!");


        var supportedLanguages =
            JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(supportedLanguagesFile));
        if (supportedLanguages == null)
            throw new Exception("There were no supported languages!");

        var selectPrompt = new MultiSelectionPrompt<string>()
            .PageSize(10)
            .Title("What languages would you like to add?")
            .Required(false)
            .MoreChoicesText("Scroll down (press the down arrow) to see more languages.");
        foreach (var (key, value) in supportedLanguages)
            selectPrompt.AddChoice($"{key} ({value})");

        var selected = AnsiConsole.Prompt(selectPrompt);

        LanguageInfo info = null!;
        foreach (var s in selected)
        {
            var shortName = s.Split("(")[0].Trim();
            var dictionaryFile = files.FirstOrDefault(x => Path.GetFileName(x) == $"{shortName}.zip");
            if (dictionaryFile == null)
                throw new Exception(
                    $"NedoProgrammer/NedordleData repository did not contain a \"{shortName}.zip\" file!");

            var languageDir = Path.Combine(_tempFolder, shortName);
            var words = new Dictionary<string, int>();

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2).Start($"Extracting [italic yellow]\"{shortName}.zip\"[/]..", ctx =>
                {
                    ZipFile.ExtractToDirectory(Path.Combine(_dataFolder, "Dictionaries", shortName + ".zip"),
                        languageDir);

                    ctx.Status("Checking for language information..");
                    var infoPath = Path.Combine(languageDir, "info.json");
                    if (!File.Exists(infoPath))
                        throw new Exception("Invalid or corrupted language archive. \"info.json\" was not found.");
                    info = JsonConvert.DeserializeObject<LanguageInfo>(File.ReadAllText(infoPath)) ??
                           throw new InvalidOperationException("Failed to parse \"info.json\".");

                    ctx.Status("Reading words..");
                    words = JsonConvert.DeserializeObject<Dictionary<string, int>>(
                        File.ReadAllText(Path.Combine(languageDir, info.DictionaryFile)));
                });

            AnsiConsole.Progress().Start(ctx =>
            {
                var indexing = ctx.AddTask($"Importing words ({info.ShortName})..");
                indexing.MaxValue = words.Count;
                indexing.Value = 0;

                CreateLanguageTable(info.ShortName);
                DatabaseController.ExecuteNonQuery(
                    $"insert into languages(short, \"full\", native, flag, word_count) values(\"{info.ShortName}\", \"{info.FullName}\", \"{info.NativeName}\", \"{info.Flag}\", {words.Count})");

                var transaction = DatabaseController.Connection.BeginTransaction();
                var command = DatabaseController.Connection.CreateCommand();
                command.CommandText =
                    $"insert into dictionary_{info.ShortName}(word, letter_count) values($word, $length)";

                var wordParameter = command.CreateParameter();
                wordParameter.ParameterName = "$word";
                command.Parameters.Add(wordParameter);

                var lengthParameter = command.CreateParameter();
                lengthParameter.ParameterName = "$length";
                command.Parameters.Add(lengthParameter);

                foreach (var word in words)
                {
                    wordParameter.Value = word.Key;
                    lengthParameter.Value = word.Value;
                    command.ExecuteNonQuery();
                    indexing.Value++;
                }

                transaction.Commit();

                AnsiConsole.MarkupLine($"[yellow italic]Creating language info table for {info.ShortName}..[/]");
                CreateLanguageInfoTable(info.ShortName);

                foreach (var pair in info.LetterCount)
                    DatabaseController.ExecuteNonQuery(
                        $"insert into info_{info.ShortName}(letter_count, word_count) values({pair.Key}, {pair.Value})");


                AnsiConsole.MarkupLine($"[green italic]{indexing.Value}[/] words were successfully imported.");
            });
        }

        AnsiConsole.MarkupLine("[bold green]Done![/]");
    }

    private static void CreateLanguageTable(string shortName)
    {
        DatabaseController.ExecuteNonQuery($@"create table dictionary_{shortName}
(
    word         text    not null
        constraint dictionary_{shortName}_pk
            primary key,
    letter_count integer not null
);

create unique index dictionary_{shortName}_word_uindex
    on dictionary_{shortName} (word);");
    }

    private static void CreateUsersTable()
    {
        DatabaseController.ExecuteNonQuery(@"create table users
(
    id    integer not null
        constraint users_pk
            primary key,
    games integer default 0 not null,
    level integer default 0 not null,
    theme text    not null
);

create unique index users_id_uindex
    on users (id);");
    }

    private static void CreateGuildsTable()
    {
        DatabaseController.ExecuteNonQuery(@"create table guilds
(
    id               integer not null
        constraint guilds_pk
            primary key,
    games            integer default 0 not null,
    create_category integer default 0 not null,
    allow_creating_channels integer default 0 not null
);

create unique index guilds_id_uindex
    on guilds (id);");
    }

    private static void CreateLanguagesTable()
    {
        DatabaseController.ExecuteNonQuery(@"create table languages
(
    short  text not null
        constraint languages_pk
            primary key,
    ""full"" text not null,
        native text not null,
        flag text not null,
        word_count integer not null
            );

        create unique index languages_full_uindex
        on languages (""full"");

        create unique index languages_native_uindex
        on languages (native);

        create unique index languages_short_uindex
        on languages (short);
        
        create unique index languages_flag_uindex
        on languages (flag);");
    }

    private static void CreateLocalesTable()
    {
        DatabaseController.ExecuteNonQuery(@"create table locales
(
    id   text not null
        constraint locales_pk
            primary key,
    data text default '' not null
);

create unique index locales_id_uindex
    on locales (id);");
    }

    private static void CreateGamesTable()
    {
        DatabaseController.ExecuteNonQuery(@"create table games
(
    id            text    not null
        constraint games_pk
            primary key,
    type          text    not null,
    players       text    not null,
    player_data   text    not null,
    language      text    not null,
    info text
);

create unique index games_id_uindex
    on games (id);");
    }

    private static void CreateLanguageInfoTable(string language)
    {
        DatabaseController.ExecuteNonQuery($@"create table info_{language}
(
    letter_count integer not null
        constraint info_{language}_pk
            primary key,
    word_count   integer not null
);

create unique index info_{language}_letter_count_uindex
    on info_{language} (letter_count);");
    }

    private static void CreateThemesTable()
    {
        DatabaseController.ExecuteNonQuery(@"create table themes
(
    id   text not null
        constraint themes_pk
            primary key,
    data text not null
);

create unique index themes_id_uindex
    on themes (id);");
    }

    // ReSharper disable FieldCanBeMadeReadOnly.Local
    private class LanguageInfo
    {
        public string DictionaryFile = null!;
        public string Flag = null!;
        public string FullName = null!;
        public Dictionary<int, int> LetterCount = new();
        public string NativeName = null!;
        public string ShortName = null!;
    }
    // ReSharper restore FieldCanBeMadeReadOnly.Local
}