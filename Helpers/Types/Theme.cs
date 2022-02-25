namespace Nedordle.Helpers.Types;

public class Theme
{
    public static Dictionary<string, Theme> Themes = new();

    public string BackgroundColor;
    public string CloseColor;
    public string CloseEmoji;
    public string CorrectColor;

    public string CorrectEmoji;

    public string Id;
    public string WrongColor;
    public string WrongEmoji;
}