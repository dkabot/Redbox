using System.Collections.Generic;

namespace Redbox.UpdateService.Model
{
    public class PollRequestDTO
    {
        public PollRequestType PRT { get; set; }

        public int SId { get; set; }

        public string D { get; set; }

        public static List<PollRequestDTO> GetPollReplyDTOList(List<PollRequest> list)
        {
            var pollReplyDtoList = new List<PollRequestDTO>();
            foreach (var pollRequest in list)
            {
                var pollRequestDto = new PollRequestDTO();
                pollReplyDtoList.Add(pollRequestDto);
                pollRequestDto.PRT = pollRequest.PollRequestType;
                pollRequestDto.SId = pollRequest.SyncId;
                pollRequestDto.D = pollRequest.Data;
            }

            return pollReplyDtoList;
        }

        public static List<PollRequest> GetPollRequestList(List<PollRequestDTO> list)
        {
            var pollRequestList = new List<PollRequest>();
            foreach (var pollRequestDto in list)
            {
                var pollRequest = new PollRequest();
                pollRequestList.Add(pollRequest);
                pollRequest.PollRequestType = pollRequestDto.PRT;
                pollRequest.SyncId = pollRequestDto.SId;
                pollRequest.Data = pollRequestDto.D;
            }

            return pollRequestList;
        }
    }
}