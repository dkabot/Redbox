using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.Environment
{
  internal class ProductLookupDataStoreProvider : DataStoreProvider
  {
    protected override long OnCount(string dataStore, string tableName)
    {
      long num = 0;
      switch (tableName)
      {
        case "Version":
          num = 2L;
          break;
        case "BarcodeToProduct":
          IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
          num = service != null ? service.GetInventoryDataCount() : -1L;
          break;
      }
      return num;
    }

    protected override object OnGet(string dataStore, string tableName, string keyName)
    {
      object obj = (object) null;
      IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
      if (service != null)
      {
        switch (tableName)
        {
          case "BarcodeToProduct":
            Inventory inventory = service.GetInventory(keyName);
            if (inventory != null)
            {
              string str = inventory.Code.ToString();
              if (inventory.Code == InventoryStatusCode.WrongTitle)
                str = "Wrong Title";
              obj = (object) ("{ " + string.Format("product_id = {0}, status = '{1}', total_rental_count = {2}", (object) inventory.TitleId, (object) str, (object) inventory.TotalRentalCount) + " }");
              break;
            }
            break;
          case "Version":
            switch (keyName)
            {
              case "origin_machine":
                obj = (object) ("{ name = '" + service.GetInventoryDataMachineOrigin()?.Trim() + "' }");
                break;
              case "generated_on":
                obj = (object) ("{ date = '" + service.GetInventoryDataCreatedOn()?.Trim() + "' }");
                break;
            }
            break;
        }
      }
      return obj;
    }

    protected override void OnBulkSet(
      string dataStore,
      string tableName,
      IDictionary<string, object> values)
    {
      int num = tableName == "Version" ? 1 : 0;
    }

    protected override ReadOnlyCollection<string> OnGetKeys(string dataStore, string tableName)
    {
      switch (tableName)
      {
        case "Version":
          return new List<string>()
          {
            "origin_machine",
            "generated_on"
          }.AsReadOnly();
        case "BarcodeToProduct":
          return new List<string>().AsReadOnly();
        default:
          return new List<string>().AsReadOnly();
      }
    }

    protected override ReadOnlyCollection<string> OnGetTables(string dataStore)
    {
      return new List<string>()
      {
        "Version",
        "BarcodeToProduct"
      }.AsReadOnly();
    }

    private class Constants
    {
      public const string Version = "Version";
      public const string BarcodeToProduct = "BarcodeToProduct";
      public const string OriginalMachine = "origin_machine";
      public const string GeneratedOn = "generated_on";
    }
  }
}
