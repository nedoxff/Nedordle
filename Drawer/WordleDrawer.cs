using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Nedordle.Drawer;

public class WordleDrawer
{
    private const int BlockOffset = 69;
    private const int BlockMiddleXOffset = 32;
    private const int BlockMiddleYOffset = 37;
    private const int FontSize = 32;

    public static MemoryStream Generate(string markup)
    {
        //markup = new string(markup.ToCharArray().Where(c => !char.IsPunctuation(c)).ToArray());
        var split = Split(markup);
        split = split.Where(x => !string.IsNullOrEmpty(x)).Select(s => s.Trim(' ')).ToArray();
        
        var collection = new FontCollection();
        collection.Add(IsInRange(markup, UnicodeRanges.BasicLatin)
            ? "Resources/wordle_font.ttf"
            : "Resources/noto.ttf");
        var family = collection.Families.ToArray()[0];
        var font = family.CreateFont(FontSize, FontStyle.Bold);

        var result = new MemoryStream();
        var width = Regex.Replace(markup, @"(\[.\])", "").Split("\n").Max()!.Length * BlockOffset + 5;
        var image = new Image<Rgba32>(width, 5 + BlockOffset * markup.Split("\n").Length);
        Color? currentColor = null;

        image.Mutate(i =>
        {
            i.Clear(Color.FromRgba(18, 18, 19, 255));

            var x = 5;
            var y = 5;
            foreach (var s in split)
            {
                if (string.IsNullOrEmpty(s)) continue;
                if (Regex.IsMatch(s, @"(\[.\])"))
                {
                    currentColor = s switch
                    {
                        "[c]" => Color.FromRgba(83, 141, 78, 255),
                        "[w]" => Color.FromRgba(58, 58, 60, 255),
                        "[d]" => Color.FromRgba(181, 159, 59, 255),
                        "[/]" => null,
                        _ => currentColor
                    };
                    continue;
                }

                foreach (var c in s)
                {
                    if (c == '\n')
                    {
                        x = 5;
                        y += BlockOffset;
                        continue;
                    }
                    
                    if (currentColor == null)
                        throw new Exception("Invalid input string! (detected [[/]], but no color next)");

                    var rectangle = new RectangularPolygon(x, y, 64, 64);
                    i.Fill((Color) currentColor, rectangle);
                    i.DrawText(new TextOptions(font)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Origin = new Vector2(x + BlockMiddleXOffset, y + 32)
                    }, char.ToUpper(c).ToString(), Color.White);
                    x += BlockOffset;
                }
            }
        });

        image.SaveAsPng(result);
        return result;
    }

    private static string[] Split(string markup) => Regex.Split(markup, @"(\[.\])");

    private static bool IsInRange(string input, UnicodeRange range) =>
        input.All(x => x >= range.FirstCodePoint && x <= range.FirstCodePoint + range.Length);
}