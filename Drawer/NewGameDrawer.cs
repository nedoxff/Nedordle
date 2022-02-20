using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Nedordle.Drawer;

public class NewGameDrawer
{
    private const int BlockOffset = 69;
    private const int BlockMiddleXOffset = 32;
    private const int BlockMiddleYOffset = 37;
    private const int ImageHeight = 74;
    private const int FontSize = 32;

    public static MemoryStream Generate(string id)
    {
        id = new string(id.ToCharArray().Where(c => !char.IsPunctuation(c)).ToArray());
        var collection = new FontCollection();
        collection.Add("Resources/wordle_font.ttf");
        var family = collection.Families.ToArray()[0];
        var font = family.CreateFont(FontSize, FontStyle.Bold);

        var result = new MemoryStream();
        var width = id.Length * BlockOffset + 5;
        var image = new Image<Rgba32>(width, ImageHeight);

        image.Mutate(i =>
        {
            i.Clear(Color.FromRgba(18, 18, 19, 255));

            var x = 5;
            foreach (var letter in id)
            {
                var rectangle = new RectangularPolygon(x, 5, 64, 64);
                i.Fill(Color.FromRgba(83, 141, 78, 255), rectangle);
                i.DrawText(new TextOptions(font)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Origin = new Vector2(x + BlockMiddleXOffset, BlockMiddleYOffset)
                }, char.ToUpper(letter).ToString(), Color.White);
                x += BlockOffset;
            }
        });

        image.SaveAsPng(result);
        return result;
    }
}