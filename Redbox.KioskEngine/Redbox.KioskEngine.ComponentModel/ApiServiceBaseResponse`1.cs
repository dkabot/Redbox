using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Redbox.KioskEngine.ComponentModel
{
  public class ApiServiceBaseResponse<T>
  {
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    [Required]
    public bool Success { get; set; }

    public IList<ResponseError> Errors { get; set; } = (IList<ResponseError>) new List<ResponseError>();

    [JsonProperty(Required = Required.DisallowNull)]
    public string HostName => Environment.MachineName;

    [Required]
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public void SetResponseTime(int? responseTime) => this.ResponseTime = responseTime;

    public int? ResponseTime { get; set; }

    public T Content { get; set; }

    public ApiServiceBaseResponse(HttpStatusCode statusCode) => this.StatusCode = statusCode;

    public ApiServiceBaseResponse(int statusCode) => this.StatusCode = (HttpStatusCode) statusCode;

    public ApiServiceBaseResponse()
    {
    }
  }
}
