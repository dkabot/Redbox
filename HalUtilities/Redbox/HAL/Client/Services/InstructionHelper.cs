using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services
{
    public struct InstructionHelper : IDisposable
    {
        private const int DefaultTimeout = 120000;
        private readonly HardwareService Service;

        public void Dispose()
        {
        }

        public InstructionHelper(HardwareService s)
            : this()
        {
            Service = s;
        }

        public HardwareJob Execute(string instruction)
        {
            return Execute(instruction, 120000);
        }

        public HardwareJob Execute(string instruction, int timeout)
        {
            var job = (HardwareJob)null;
            var result = Service.ExecuteImmediate(instruction, timeout, out job);
            result.Dump();
            return !result.Success ? null : job;
        }

        public ErrorCodes ExecuteErrorCode(string instruction)
        {
            return ExecuteErrorCode(instruction, 120000);
        }

        public ErrorCodes ExecuteErrorCode(string instruction, int timeout)
        {
            var str = ExecuteGeneric(instruction, timeout);
            return string.IsNullOrEmpty(str)
                ? ErrorCodes.ServiceChannelError
                : Enum<ErrorCodes>.ParseIgnoringCase(str, ErrorCodes.ServiceChannelError);
        }

        public IControlResponse ExecuteWithResponse(string instruction)
        {
            return ExecuteWithResponse(instruction, 120000);
        }

        public IControlResponse ExecuteWithResponse(string instruction, int timeout)
        {
            return From(ExecuteGeneric(instruction, timeout));
        }

        public string ExecuteGeneric(string instruction)
        {
            return ExecuteGeneric(instruction, 120000);
        }

        public string ExecuteGeneric(string instruction, int timeout)
        {
            var job = (HardwareJob)null;
            var result = Service.ExecuteImmediate(instruction, timeout, out job);
            result.Dump();
            return !result.Success ? string.Empty : job.GetTopOfStack();
        }

        private IControlResponse From(string serviceResponse)
        {
            var clientControlResponse = new ClientControlResponse();
            if (string.IsNullOrEmpty(serviceResponse))
            {
                clientControlResponse.Diagnostic = ErrorCodes.ServiceChannelError.ToString().ToUpper();
                clientControlResponse.CommError = true;
            }
            else if (serviceResponse.Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase))
            {
                clientControlResponse.Success = true;
            }
            else if (serviceResponse.Equals("TIMEOUT", StringComparison.CurrentCultureIgnoreCase))
            {
                clientControlResponse.TimedOut = true;
            }
            else
            {
                clientControlResponse.CommError = clientControlResponse.TimedOut = true;
                clientControlResponse.Diagnostic = serviceResponse;
            }

            return clientControlResponse;
        }

        private class ClientControlResponse : IControlResponse
        {
            internal ClientControlResponse()
            {
                Diagnostic = string.Empty;
            }

            public bool Success { get; internal set; }

            public bool TimedOut { get; internal set; }

            public bool CommError { get; internal set; }

            public string Diagnostic { get; internal set; }
        }
    }
}