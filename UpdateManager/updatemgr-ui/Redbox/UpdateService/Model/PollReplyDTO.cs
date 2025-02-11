using System.Collections.Generic;

namespace Redbox.UpdateService.Model
{
    internal class PollReplyDTO
    {
        public static List<PollReplyDTO> GetPollReplyDTOList(List<PollReply> list)
        {
            List<PollReplyDTO> pollReplyDtoList = new List<PollReplyDTO>();
            foreach (PollReply pollReply in list)
                pollReplyDtoList.Add(PollReplyDTO.GetPollReplyDTO(pollReply));
            return pollReplyDtoList;
        }

        public static PollReplyDTO GetPollReplyDTO(PollReply pollReply)
        {
            return new PollReplyDTO()
            {
                PRT = pollReply.PollReplyType,
                SId = pollReply.SyncId,
                D = pollReply.Data
            };
        }

        public static List<PollReply> GetPollReplyList(List<PollReplyDTO> list)
        {
            List<PollReply> pollReplyList = new List<PollReply>();
            foreach (PollReplyDTO pollReplyDto in list)
            {
                PollReply pollReply = new PollReply();
                pollReplyList.Add(pollReply);
                pollReply.PollReplyType = pollReplyDto.PRT;
                pollReply.SyncId = pollReplyDto.SId;
                pollReply.Data = pollReplyDto.D;
            }
            return pollReplyList;
        }

        public PollReplyType PRT { get; set; }

        public int SId { get; set; }

        public string D { get; set; }
    }
}
