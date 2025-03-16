using Redbox.Rental.Model.Ads;
using System.Threading.Tasks;

namespace Redbox.Rental.Model.KioskClientService.Ads
{
    public interface IKioskClientServiceAds
    {
        Task<IGetAdResponse> GetAdAsync(AdLocation adLocation);

        Task<bool> ExpireAd(string advertisementId);

        Task<bool> SendProofOfPlayAsync(IAdImpression adImpression);

        IGetAdResponse GetAd(AdLocation adLocation);

        bool SendProofOfPlay(IAdImpression adImpression);
    }
}