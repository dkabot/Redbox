using System.Collections.Generic;

namespace Redbox.UpdateService.Model
{
  public class PollRequestDTO
  {
    public static List<PollRequestDTO> GetPollReplyDTOList(List<PollRequest> list)
    {
      List<PollRequestDTO> pollReplyDtoList = new List<PollRequestDTO>();
      foreach (PollRequest pollRequest in list)
      {
        PollRequestDTO pollRequestDto = new PollRequestDTO();
        pollReplyDtoList.Add(pollRequestDto);
        pollRequestDto.PRT = pollRequest.PollRequestType;
        pollRequestDto.SId = pollRequest.SyncId;
        pollRequestDto.D = pollRequest.Data;
      }
      return pollReplyDtoList;
    }

    public static List<PollRequest> GetPollRequestList(List<PollRequestDTO> list)
    {
      List<PollRequest> pollRequestList = new List<PollRequest>();
      foreach (PollRequestDTO pollRequestDto in list)
      {
        PollRequest pollRequest = new PollRequest();
        pollRequestList.Add(pollRequest);
        pollRequest.PollRequestType = pollRequestDto.PRT;
        pollRequest.SyncId = pollRequestDto.SId;
        pollRequest.Data = pollRequestDto.D;
      }
      return pollRequestList;
    }

    public PollRequestType PRT { get; set; }

    public int SId { get; set; }

    public string D { get; set; }
  }
}
