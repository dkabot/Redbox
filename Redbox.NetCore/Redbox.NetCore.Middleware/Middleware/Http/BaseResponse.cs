using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Redbox.NetCore.Middleware.Http
{
    public class BaseResponse
    {
        public BaseResponse(HttpStatusCode statusCode)
        {
            StatusCode = (int)statusCode;
        }

        public BaseResponse(int statusCode)
        {
            StatusCode = statusCode;
        }

        public BaseResponse()
        {
        }

        [JsonProperty] public bool Success { get; set; }

        public IList<Error> Errors { get; set; } = new List<Error>();

        [JsonProperty] public string HostName => Environment.MachineName;

        public int StatusCode { get; set; } = 200;

        public int? ResponseTime { get; set; }

        public string Content { get; set; }

        public ObjectResult ToObjectResult()
        {
            return new ObjectResult(this)
            {
                StatusCode = StatusCode
            };
        }
    }
}