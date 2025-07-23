using System.Drawing;
using System.Drawing.Text;
using System.Reflection;

namespace WeSupport
{
    public static class FontHelper
    {
        public static Font LoadFont(string resourcePath, float size, FontStyle style = FontStyle.Regular)
        {
            var fontCollection = new PrivateFontCollection();
            using (Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                byte[] fontData = new byte[fontStream.Length];
                fontStream.Read(fontData, 0, (int)fontStream.Length);
                unsafe
                {
                    fixed (byte* pFontData = fontData)
                    {
                        fontCollection.AddMemoryFont((IntPtr)pFontData, fontData.Length);
                    }
                }
            }
            return new Font(fontCollection.Families[0], size, style);
        }
    }
} 