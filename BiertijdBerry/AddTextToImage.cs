
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace BiertijdBerry
{
    public static class AddTextToImage
    {
        public static Stream AddText(Stream imageStream, string text, int x, int y)
        {
            var image = Image.Load(imageStream);

            image.Mutate(img => { img.DrawText(text, SystemFonts.CreateFont("Verdana", 20), Rgba32.Black, new PointF(x, y)); }) ;

            var ms = new MemoryStream();
            image.SaveAsPng(ms);

            ms.Position = 0;

            return ms;

        }
    }
}