using System.Collections.Generic;

namespace Redbox.UpdateService.Model
{
    public class WorkResultPollRequestDTO
    {
        public Dictionary<object, object> Results;
        public long ScriptID;
        public string StoreNumber;
    }
}