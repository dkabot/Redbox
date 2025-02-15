using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Redbox.DirectShow;

public static class Image
{
    public static bool IsGrayscale(Bitmap image)
    {
        var flag = false;
        if (image.PixelFormat == PixelFormat.Format8bppIndexed)
        {
            flag = true;
            var palette = image.Palette;
            for (var index = 0; index < 256; ++index)
            {
                var entry = palette.Entries[index];
                if (entry.R != index || entry.G != index || entry.B != index)
                {
                    flag = false;
                    break;
                }
            }
        }

        return flag;
    }

    public static Bitmap CreateGrayscaleImage(int width, int height)
    {
        var image = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
        SetGrayscalePalette(image);
        return image;
    }

    public static void SetGrayscalePalette(Bitmap image)
    {
        var colorPalette = image.PixelFormat == PixelFormat.Format8bppIndexed
            ? image.Palette
            : throw new UnsupportedImageFormatException("Source image is not 8 bpp image.");
        for (var index = 0; index < 256; ++index)
            colorPalette.Entries[index] = Color.FromArgb(index, index, index);
        image.Palette = colorPalette;
    }

    public static Bitmap Clone(Bitmap source, PixelFormat format)
    {
        if (source.PixelFormat == format)
            return Clone(source);
        var width = source.Width;
        var height = source.Height;
        var bitmap = new Bitmap(width, height, format);
        var graphics = Graphics.FromImage(bitmap);
        graphics.DrawImage(source, 0, 0, width, height);
        graphics.Dispose();
        return bitmap;
    }

    public static Bitmap Clone(Bitmap source)
    {
        var bitmapData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly,
            source.PixelFormat);
        var bitmap = Clone(bitmapData);
        source.UnlockBits(bitmapData);
        if (source.PixelFormat == PixelFormat.Format1bppIndexed ||
            source.PixelFormat == PixelFormat.Format4bppIndexed ||
            source.PixelFormat == PixelFormat.Format8bppIndexed || source.PixelFormat == PixelFormat.Indexed)
        {
            var palette1 = source.Palette;
            var palette2 = bitmap.Palette;
            var length = palette1.Entries.Length;
            for (var index = 0; index < length; ++index)
                palette2.Entries[index] = palette1.Entries[index];
            bitmap.Palette = palette2;
        }

        return bitmap;
    }

    public static Bitmap Clone(BitmapData sourceData)
    {
        var width = sourceData.Width;
        var height = sourceData.Height;
        var bitmap = new Bitmap(width, height, sourceData.PixelFormat);
        var bitmapdata =
            bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        SystemTools.CopyUnmanagedMemory(bitmapdata.Scan0, sourceData.Scan0, height * sourceData.Stride);
        bitmap.UnlockBits(bitmapdata);
        return bitmap;
    }

    public static Bitmap FromFile(string fileName)
    {
        var fileStream = (FileStream)null;
        try
        {
            fileStream = File.OpenRead(fileName);
            var memoryStream = new MemoryStream();
            var buffer = new byte[10000];
            while (true)
            {
                var count = fileStream.Read(buffer, 0, 10000);
                if (count != 0)
                    memoryStream.Write(buffer, 0, count);
                else
                    break;
            }

            return (Bitmap)System.Drawing.Image.FromStream(memoryStream);
        }
        finally
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream.Dispose();
            }
        }
    }

    public static unsafe Bitmap Convert16bppTo8bpp(Bitmap bimap)
    {
        var width = bimap.Width;
        var height = bimap.Height;
        Bitmap bitmap;
        int num1;
        switch (bimap.PixelFormat)
        {
            case PixelFormat.Format16bppGrayScale:
                bitmap = CreateGrayscaleImage(width, height);
                num1 = 1;
                break;
            case PixelFormat.Format48bppRgb:
                bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                num1 = 3;
                break;
            case PixelFormat.Format64bppPArgb:
                bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                num1 = 4;
                break;
            case PixelFormat.Format64bppArgb:
                bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                num1 = 4;
                break;
            default:
                throw new UnsupportedImageFormatException("Invalid pixel format of the source image.");
        }

        var bitmapdata1 = bimap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bimap.PixelFormat);
        var bitmapdata2 =
            bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        var scan0 = bitmapdata1.Scan0;
        var pointer1 = (byte*)scan0.ToPointer();
        scan0 = bitmapdata2.Scan0;
        var pointer2 = (byte*)scan0.ToPointer();
        var stride1 = bitmapdata1.Stride;
        var stride2 = bitmapdata2.Stride;
        for (var index = 0; index < height; ++index)
        {
            var numPtr1 = (ushort*)(pointer1 + index * stride1);
            var numPtr2 = pointer2 + index * stride2;
            var num2 = 0;
            var num3 = width * num1;
            while (num2 < num3)
            {
                *numPtr2 = (byte)((uint)*numPtr1 >> 8);
                ++num2;
                ++numPtr1;
                ++numPtr2;
            }
        }

        bimap.UnlockBits(bitmapdata1);
        bitmap.UnlockBits(bitmapdata2);
        return bitmap;
    }

    public static unsafe Bitmap Convert8bppTo16bpp(Bitmap bimap)
    {
        var width = bimap.Width;
        var height = bimap.Height;
        Bitmap bitmap;
        int num1;
        switch (bimap.PixelFormat)
        {
            case PixelFormat.Format24bppRgb:
                bitmap = new Bitmap(width, height, PixelFormat.Format48bppRgb);
                num1 = 3;
                break;
            case PixelFormat.Format8bppIndexed:
                bitmap = new Bitmap(width, height, PixelFormat.Format16bppGrayScale);
                num1 = 1;
                break;
            case PixelFormat.Format32bppPArgb:
                bitmap = new Bitmap(width, height, PixelFormat.Format64bppPArgb);
                num1 = 4;
                break;
            case PixelFormat.Format32bppArgb:
                bitmap = new Bitmap(width, height, PixelFormat.Format64bppArgb);
                num1 = 4;
                break;
            default:
                throw new UnsupportedImageFormatException("Invalid pixel format of the source image.");
        }

        var bitmapdata1 = bimap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bimap.PixelFormat);
        var bitmapdata2 =
            bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        var scan0 = bitmapdata1.Scan0;
        var pointer1 = (byte*)scan0.ToPointer();
        scan0 = bitmapdata2.Scan0;
        var pointer2 = (byte*)scan0.ToPointer();
        var stride1 = bitmapdata1.Stride;
        var stride2 = bitmapdata2.Stride;
        for (var index = 0; index < height; ++index)
        {
            var numPtr1 = pointer1 + index * stride1;
            var numPtr2 = (ushort*)(pointer2 + index * stride2);
            var num2 = 0;
            var num3 = width * num1;
            while (num2 < num3)
            {
                *numPtr2 = (ushort)((uint)*numPtr1 << 8);
                ++num2;
                ++numPtr1;
                ++numPtr2;
            }
        }

        bimap.UnlockBits(bitmapdata1);
        bitmap.UnlockBits(bitmapdata2);
        return bitmap;
    }
}