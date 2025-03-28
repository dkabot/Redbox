using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Redbox.KioskEngine.Environment
{
  public class BitmapCacheService : IBitmapCacheService, IDisposable
  {
    private IDictionary<string, BitmapCacheService.ImageTracker> m_images;
    private IDictionary<string, BitmapImage> _bitmapImages = (IDictionary<string, BitmapImage>) new Dictionary<string, BitmapImage>();
    private readonly SortedDictionary<string, int> m_usageCounts = new SortedDictionary<string, int>();
    private Dictionary<string, int> _bitmapImageUsageCount = new Dictionary<string, int>();

    public static BitmapCacheService Instance => Singleton<BitmapCacheService>.Instance;

    public ErrorList Initialize()
    {
      LogHelper.Instance.Log("Initialize bitmap cache service.");
      ServiceLocator.Instance.AddService(typeof (IBitmapCacheService), (object) BitmapCacheService.Instance);
      return new ErrorList();
    }

    public Image GetImage(string name)
    {
      if (!this.Images.ContainsKey(name))
        return (Image) null;
      BitmapCacheService.ImageTracker image = this.Images[name];
      int num = (this.m_usageCounts.ContainsKey(name) ? this.m_usageCounts[name] : 0) + 1;
      if (num > 65536)
        num = 1;
      this.m_usageCounts[name] = num;
      return image.Instance;
    }

    public Image RegisterImage(string name, byte[] data)
    {
      Image image = this.GetImage(name);
      if (image != null)
        return image;
      if (data != null)
      {
        if (data.Length != 0)
        {
          try
          {
            MemoryStream memoryStream = new MemoryStream(data);
            image = Image.FromStream((Stream) memoryStream);
            this.Images[name] = new BitmapCacheService.ImageTracker()
            {
              Instance = image,
              UnderlyingStream = (Stream) memoryStream
            };
          }
          catch (Exception ex)
          {
            LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterImage from stream.", ex);
          }
          return image;
        }
      }
      return (Image) null;
    }

    public BitmapImage RegisterBitmapImage(string name, byte[] data)
    {
      BitmapImage bitmapImage = this.GetBitmapImage(name);
      if (bitmapImage == null && data != null)
      {
        if (data.Length != 0)
        {
          try
          {
            MemoryStream memoryStream = new MemoryStream(data);
            bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = (Stream) memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            this.BitmapImages.Add(name, bitmapImage);
          }
          catch (Exception ex)
          {
            LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterBitmapImage from stream.", ex);
          }
        }
      }
      return bitmapImage;
    }

    public Image RegisterImage(string name, string path)
    {
      Image image = this.GetImage(name);
      if (image == null)
      {
        try
        {
          image = Image.FromFile(path);
          this.Images[name] = new BitmapCacheService.ImageTracker()
          {
            Instance = image
          };
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterImage from file.", ex);
        }
      }
      return image;
    }

    public Image RegisterThumbnailImage(string name, byte[] data, int width, int height)
    {
      Image image1 = this.GetImage(name);
      if (image1 != null)
        return image1;
      if (data != null)
      {
        if (data.Length != 0)
        {
          try
          {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
              using (Image image2 = Image.FromStream((Stream) memoryStream))
              {
                image1 = image2.GetThumbnailImage(width, height, (Image.GetThumbnailImageAbort) (() => false), IntPtr.Zero);
                this.Images[name] = new BitmapCacheService.ImageTracker()
                {
                  Instance = image1
                };
              }
            }
          }
          catch (Exception ex)
          {
            LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterThumbnailImage from stream.", ex);
          }
          return image1;
        }
      }
      return (Image) null;
    }

    public Image RegisterThumbnailImage(string name, string newName, int width, int height)
    {
      Image image1 = this.GetImage(newName);
      if (image1 != null)
        return image1;
      Image image2 = this.GetImage(name);
      if (image2 == null)
        return (Image) null;
      try
      {
        Image thumbnailImage = image2.GetThumbnailImage(width, height, (Image.GetThumbnailImageAbort) (() => false), IntPtr.Zero);
        this.Images[newName] = new BitmapCacheService.ImageTracker()
        {
          Instance = thumbnailImage
        };
        return thumbnailImage;
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterThumbnailImage from existing image.", ex);
      }
      return (Image) null;
    }

    public void DropCache()
    {
      this.DropImageCache();
      this.DropBitmapImageCache();
    }

    private void DropBitmapImageCache()
    {
      int count = this._bitmapImages.Count;
      LogHelper.Instance.Log("Drop bitmap cache.");
      foreach (KeyValuePair<string, BitmapImage> bitmapImage in (IEnumerable<KeyValuePair<string, BitmapImage>>) this._bitmapImages)
        LogHelper.Instance.Log("...Dropping bitmap image '{0}'.", (object) bitmapImage.Key);
      this._bitmapImages.Clear();
      this._bitmapImageUsageCount.Clear();
      LogHelper.Instance.Log("Drop bitmap cache complete. {0} bitmaps removed.", (object) count);
    }

    private void DropImageCache()
    {
      LogHelper.Instance.Log("Drop image cache.");
      List<string> stringList = new List<string>();
      foreach (string key in (IEnumerable<string>) this.Images.Keys)
      {
        LogHelper.Instance.Log("...Dispose image '{0}'.", (object) key);
        try
        {
          this.Images[key].Dispose();
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.DropImageCache.", ex);
        }
        finally
        {
          stringList.Add(key);
        }
      }
      LogHelper.Instance.Log("...Remove image keys from dictionary.");
      foreach (string key in stringList)
        this.Images.Remove(key);
    }

    public int TrimCache(int maximumItems)
    {
      return this.TrimBitmapImageCache(maximumItems) + this.TrimImageCache(maximumItems);
    }

    private int TrimBitmapImageCache(int maximumItems)
    {
      int count = Math.Max(0, this._bitmapImages.Count - maximumItems);
      LogHelper.Instance.Log("Trim the bitmap image cache:");
      List<BitmapCacheService.ImageUsage> source = new List<BitmapCacheService.ImageUsage>();
      foreach (KeyValuePair<string, int> keyValuePair in this._bitmapImageUsageCount)
        source.Add(new BitmapCacheService.ImageUsage()
        {
          Name = keyValuePair.Key,
          Uses = keyValuePair.Value
        });
      source.Sort((Comparison<BitmapCacheService.ImageUsage>) ((x, y) => x.Uses.CompareTo(y.Uses)));
      foreach (BitmapCacheService.ImageUsage imageUsage in source.Take<BitmapCacheService.ImageUsage>(count))
      {
        LogHelper.Instance.Log("...Trim bitmap image '{0}' from the cache.", (object) imageUsage.Name);
        this._bitmapImages.Remove(imageUsage.Name);
        this._bitmapImageUsageCount.Remove(imageUsage.Name);
      }
      LogHelper.Instance.Log("...{0} bitmap images trimmed.", (object) count);
      return count;
    }

    private int TrimImageCache(int maximumItems)
    {
      int num = 0;
      if (this.m_images.Count <= maximumItems)
        return num;
      LogHelper.Instance.Log("Trim the image cache:");
      List<BitmapCacheService.ImageUsage> imageUsageList = new List<BitmapCacheService.ImageUsage>();
      IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
      if (service != null)
      {
        foreach (KeyValuePair<string, BitmapCacheService.ImageTracker> image in (IEnumerable<KeyValuePair<string, BitmapCacheService.ImageTracker>>) this.m_images)
        {
          if (!service.IsBitmapReferenced(image.Value.Instance))
            imageUsageList.Add(new BitmapCacheService.ImageUsage()
            {
              Name = image.Key,
              Uses = this.m_usageCounts.ContainsKey(image.Key) ? this.m_usageCounts[image.Key] : 0
            });
        }
      }
      imageUsageList.Sort((Comparison<BitmapCacheService.ImageUsage>) ((x, y) => x.Uses.CompareTo(y.Uses)));
      for (int index = 0; index < this.m_images.Count - maximumItems; ++index)
      {
        string name = imageUsageList[index].Name;
        LogHelper.Instance.Log("...Trim image '{0}' from the cache.", (object) name);
        BitmapCacheService.ImageTracker image = this.m_images[name];
        try
        {
          image.Dispose();
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.TrimCache.", ex);
        }
        this.m_images.Remove(name);
        this.m_usageCounts.Remove(name);
        ++num;
      }
      LogHelper.Instance.Log("...{0} images trimmed.", (object) num);
      return num;
    }

    public BitmapImage GetBitmapImage(string imageName)
    {
      BitmapImage bitmapImage = (BitmapImage) null;
      this.BitmapImages.TryGetValue(imageName, out bitmapImage);
      if (bitmapImage != null)
      {
        int num1 = 0;
        this._bitmapImageUsageCount.TryGetValue(imageName, out num1);
        int num2 = num1 + 1;
        this._bitmapImageUsageCount[imageName] = num2;
      }
      return bitmapImage;
    }

    public BitmapImage ToBitmapImage(Image image)
    {
      return image == null ? (BitmapImage) null : this.ToBitmapImage(new Bitmap(image));
    }

    public BitmapImage ToBitmapImage(Bitmap bitmap)
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        bitmap.Save((Stream) memoryStream, ImageFormat.Png);
        memoryStream.Position = 0L;
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = (Stream) memoryStream;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        return bitmapImage;
      }
    }

    public void Dispose() => this.DropCache();

    internal IDictionary<string, BitmapCacheService.ImageTracker> Images
    {
      get
      {
        if (this.m_images == null)
          this.m_images = (IDictionary<string, BitmapCacheService.ImageTracker>) new Dictionary<string, BitmapCacheService.ImageTracker>();
        return this.m_images;
      }
    }

    private IDictionary<string, BitmapImage> BitmapImages => this._bitmapImages;

    private BitmapCacheService()
    {
    }

    internal sealed class ImageUsage
    {
      public string Name { get; set; }

      public int Uses { get; set; }
    }

    internal sealed class ImageTracker : IDisposable
    {
      public void Dispose()
      {
        try
        {
          if (this.Instance != null)
            this.Instance.Dispose();
          if (this.UnderlyingStream == null)
            return;
          this.UnderlyingStream.Dispose();
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log("An unhandled exception was raised in ImageTracker.Dispose.", ex);
        }
      }

      public Stream UnderlyingStream { get; set; }

      public Image Instance { get; set; }
    }
  }
}
