using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Redbox.Core;
using System;

namespace Redbox.Rental.Model.KioskClientService.Campaign
{
    public class ControlListConverter : JsonConverter<ControlList>
    {
        public override ControlList ReadJson(
            JsonReader reader,
            Type objectType,
            ControlList existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            try
            {
                var controlList1 = existingValue;
                if (reader.TokenType == JsonToken.StartArray)
                {
                    var jarray = JArray.Load(reader);
                    if (jarray != null)
                    {
                        controlList1 = new ControlList();
                        foreach (var source in jarray)
                            switch (source.GetCaseInsensitveKeyValue("ControlType").ToObject<ControlType>())
                            {
                                case ControlType.StartAsset:
                                    var controlList2 = controlList1;
                                    var startAssetControl = new StartAssetControl();
                                    startAssetControl.ControlId =
                                        source.GetCaseInsensitveKeyValue("ControlId").ToObject<long>();
                                    startAssetControl.DisplayDuration =
                                        source.GetCaseInsensitveKeyValue("DisplayDuration").ToObject<int>();
                                    startAssetControl.Asset =
                                        source.GetCaseInsensitveKeyValue("Asset").ToObject<Asset>();
                                    startAssetControl.Target = source.GetCaseInsensitveKeyValue("Target")
                                        .ToObject<AssetTarget>();
                                    startAssetControl.TargetValue = source.GetCaseInsensitveKeyValue("TargetValue")
                                        ?.ToObject<string>();
                                    startAssetControl.Order =
                                        source.GetCaseInsensitveKeyValue("Order").ToObject<short>();
                                    startAssetControl.IncludeIfNoInventory = (bool?)source
                                        .GetCaseInsensitveKeyValue("IncludeIfNoInventory")?.ToObject<bool?>();
                                    startAssetControl.Validation = source.GetCaseInsensitveKeyValue("Validation")
                                        ?.ToObject<StartScreenPromotionCodeValidationResponse>();
                                    controlList2.Add((IControl)startAssetControl);
                                    continue;
                                case ControlType.Carousel:
                                    var controlList3 = controlList1;
                                    var carouselControl = new CarouselControl();
                                    carouselControl.ControlId =
                                        source.GetCaseInsensitveKeyValue("ControlId").ToObject<long>();
                                    carouselControl.DisplayDuration =
                                        source.GetCaseInsensitveKeyValue("DisplayDuration").ToObject<int>();
                                    carouselControl.MaxTitles =
                                        source.GetCaseInsensitveKeyValue("MaxTitles").ToObject<int>();
                                    controlList3.Add((IControl)carouselControl);
                                    continue;
                                default:
                                    continue;
                            }
                    }
                }

                return controlList1;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(
                    "An unhandled exception was raised in KCS.Campaign.ControlListConverter", ex);
                return existingValue;
            }
        }

        public override void WriteJson(JsonWriter writer, ControlList value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            value.ForEach((Action<IControl>)(item => serializer.Serialize(writer, (object)item)));
            writer.WriteEndArray();
        }
    }
}