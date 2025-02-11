using System.Collections.Generic;

namespace Redbox.UpdateService.Model
{
    public class PollReplyDTO
    {
        public PollReplyType PRT { get; set; }

        public int SId { get; set; }

        public string D { get; set; }

        public static List<PollReplyDTO> GetPollReplyDTOList(List<PollReply> list)
        {
            var pollReplyDtoList = new List<PollReplyDTO>();
            foreach (var pollReply in list)
                pollReplyDtoList.Add(GetPollReplyDTO(pollReply));
            return pollReplyDtoList;
        }

        public static PollReplyDTO GetPollReplyDTO(PollReply pollReply)
        {
            return new PollReplyDTO
            {
                PRT = pollReply.PollReplyType,
                SId = pollReply.SyncId,
                D = pollReply.Data
            };
        }

        public static List<PollReply> GetPollReplyList(List<PollReplyDTO> list)
        {
            var pollReplyList = new List<PollReply>();
            foreach (var pollReplyDto in list)
            {
                var pollReply = new PollReply();
                pollReplyList.Add(pollReply);
                pollReply.PollReplyType = pollReplyDto.PRT;
                pollReply.SyncId = pollReplyDto.SId;
                pollReply.Data = pollReplyDto.D;
            }

            return pollReplyList;
        }
    }
}