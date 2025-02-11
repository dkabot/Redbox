using DeviceService.ComponentModel.Commands;

namespace DeviceService.ComponentModel.Responses
{
    public class ValidateVersionResponseEvent : BaseResponseEvent
    {
        public ValidateVersionResponseEvent(BaseCommandRequest request)
            : base(request)
        {
            EventName = "ValidateVersionReponseEvent";
        }

        public ValidateVersionModel ValidateVersionModel { get; set; }
    }
}