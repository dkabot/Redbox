using System.Collections.Generic;
using System.Net;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services
{
    public class FileVersionDataResponse : ApiBaseResponse
    {
        public FileVersionDataResponse()
        {
            StatusCode = HttpStatusCode.OK;
        }

        public IEnumerable<FileVersionData> Data { get; set; }
    }
}