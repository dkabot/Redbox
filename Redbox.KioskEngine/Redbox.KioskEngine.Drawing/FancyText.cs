using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Redbox.KioskEngine.Drawing
{
    public static class FancyText
    {
        private const int BlurAmount = 6;

        public static Image ImageFromText(string strText, Font fnt, Color clrFore, Color clrBack)
        {
            Bitmap bitmap1;
            using (var graphics1 = Graphics.FromHwnd(IntPtr.Zero))
            {
                var sizeF = graphics1.MeasureString(strText, fnt);
                using (var bitmap2 = new Bitmap((int)sizeF.Width, (int)sizeF.Height))
                {
                    using (var graphics2 = Graphics.FromImage(bitmap2))
                    {
                        using (var solidBrush1 = new SolidBrush(Color.FromArgb(16, clrBack.R, clrBack.G, clrBack.B)))
                        {
                            using (var solidBrush2 = new SolidBrush(clrFore))
                            {
                                graphics2.SmoothingMode = SmoothingMode.HighQuality;
                                graphics2.InterpolationMode = InterpolationMode.HighQualityBilinear;
                                graphics2.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                                graphics2.DrawString(strText, fnt, solidBrush1, 0.0f, 0.0f);
                                bitmap1 = new Bitmap(bitmap2.Width + 6, bitmap2.Height + 6);
                                using (var graphics3 = Graphics.FromImage(bitmap1))
                                {
                                    graphics3.SmoothingMode = SmoothingMode.HighQuality;
                                    graphics3.InterpolationMode = InterpolationMode.HighQualityBilinear;
                                    graphics3.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                                    for (var x = 0; x <= 6; ++x)
                                    for (var y = 0; y <= 6; ++y)
                                        graphics3.DrawImageUnscaled(bitmap2, x, y);
                                    graphics3.DrawString(strText, fnt, solidBrush2, 3f, 3f);
                                }
                            }
                        }
                    }
                }
            }

            return bitmap1;
        }
    }
}