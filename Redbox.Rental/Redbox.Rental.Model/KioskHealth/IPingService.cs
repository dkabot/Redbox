using Redbox.Rental.Model.UpdateClientService.KioskHealth;
using System.Threading.Tasks;

namespace Redbox.Rental.Model.KioskHealth
{
    public interface IPingService
    {
        Task<PingResponse> SendPing();

        bool Enabled { get; }

        int PingInterval { get; }
    }
}