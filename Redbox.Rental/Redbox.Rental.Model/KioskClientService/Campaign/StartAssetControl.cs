namespace Redbox.Rental.Model.KioskClientService.Campaign
{
    internal class StartAssetControl : Control, IStartAssetControl, IControl
    {
        public StartAssetControl()
        {
            ControlType = ControlType.StartAsset;
        }

        public Asset Asset { get; internal set; }

        public AssetTarget Target { get; internal set; }

        public string TargetValue { get; internal set; }

        public short Order { get; internal set; }

        public bool? IncludeIfNoInventory { get; internal set; }

        public StartScreenPromotionCodeValidationResponse Validation { get; internal set; }
    }
}