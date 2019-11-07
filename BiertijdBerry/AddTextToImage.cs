using System.Drawing;
using SixLabors.ImageSharp;
using System.IO;
using Image = SixLabors.ImageSharp.Image;

namespace BiertijdBerry
{
    public static class AddTextToImage
    {
        public static Stream AddText(Stream imageStream, params (string text, (float x, float y) position)[] texts)
        {
            var image = Image.Load(imageStream);
            SixLabors.ImageSharp.
            var bitmap = new Bitmap(image.CloneAs<Bitmap>);
            var graphics = Graphics.FromImage(bitmap);
            var drawFont = new Font("Cambria", 20);

            foreach (var (text, (x, y)) in texts)
            {
                graphics.DrawString(text, drawFont, Brushes.Black, x, y);
            }

            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}