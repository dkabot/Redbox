using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.ProductLookupCatalog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Redbox.KioskEngine.Environment
{
  public class ProductLookupCatalogService : IProductLookupCatalogService
  {
    public ProductLookupCatalogModel GetProductLookupCatalog(string path)
    {
      Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> dictionary = new Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory>();
      try
      {
        ProductLookupCatalogModel productLookupCatalog = new ProductLookupCatalogModel()
        {
          Barcodes = new Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory>()
        };
        if (!File.Exists(path))
        {
          LogHelper.Instance.Log("ProductLookupCatalogModel.GetProductLookupCatalog - ProductLookup file ({0}) does not exist", (object) path);
          return (ProductLookupCatalogModel) null;
        }
        if (!MemoryArchive.IsValid(path))
        {
          LogHelper.Instance.LogError("ProductLookupCatalogModel.GetProductLookupCatalog", string.Format("ProductLookup file ({0}) is not valid", (object) path));
          return (ProductLookupCatalogModel) null;
        }
        using (MemoryArchive memoryArchive = MemoryArchive.Open(path))
        {
          productLookupCatalog.OriginMachine = memoryArchive.GetOriginMachine().Trim();
          productLookupCatalog.GeneratedOn = memoryArchive.GetCreatedOn().Trim();
          for (int i = 0; (long) i < memoryArchive.Count; ++i)
          {
            Redbox.ProductLookupCatalog.Inventory inventory = memoryArchive[(long) i];
            if (inventory != null && inventory.Barcode != null && !productLookupCatalog.Barcodes.ContainsKey(inventory.Barcode))
              productLookupCatalog.Barcodes.Add(inventory.Barcode, new Redbox.KioskEngine.ComponentModel.Inventory()
              {
                Barcode = inventory.Barcode,
                TitleId = inventory.TitleId,
                Code = (Redbox.KioskEngine.ComponentModel.InventoryStatusCode) inventory.Code,
                TotalRentalCount = inventory.TotalRentalCount
              });
          }
        }
        LogHelper.Instance.Log("ProductLookupCatalogModel.GetProductLookupCatalog - ProductLookup file {0} contains {1} barcodes.", (object) path, (object) productLookupCatalog.Barcodes.Count);
        return productLookupCatalog;
      }
      catch (Exception ex)
      {
        LogHelper.Instance.LogException(string.Format("ProductLookupCatalogModel.GetProductLookupCatalog - An unhandled exception occurred"), ex);
      }
      return (ProductLookupCatalogModel) null;
    }
  }
}
