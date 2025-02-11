using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Serilog;
using UpdateClientService.API.Services.IoT.Commands;

namespace UpdateClientService.API
{
    public static class ResetEventExtensions
    {
        public static async Task<bool> WaitAsyncAndDisposeOnFinish(
            this AsyncManualResetEvent mre,
            TimeSpan cancelAfter,
            IoTCommandModel request = null)
        {
            var tokenSource = new CancellationTokenSource(cancelAfter);
            var flag = false;
            try
            {
                await mre.WaitAsync(tokenSource.Token);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex,
                    "ResetEventExtensions.WaitAsyncAndDisposeOnFinish -> Error occured while waiting" +
                    (request != null ? " for request id " + request?.RequestId : null));
            }
            finally
            {
                flag = tokenSource.IsCancellationRequested;
                tokenSource.Dispose();
            }

            return flag;
        }
    }
}