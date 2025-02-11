using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class GetUnitHealthResponseEvent : BaseResponseEvent
    {
        public GetUnitHealthResponseEvent(BaseCommandRequest request)
            : base(request)
        {
            EventName = nameof(GetUnitHealthResponseEvent);
        }

        public UnitHealthModel UnitHealthModel { get; set; }
    }
}