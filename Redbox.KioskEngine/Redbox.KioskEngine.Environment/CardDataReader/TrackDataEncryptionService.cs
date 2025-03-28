using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.REDS.Framework;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace Redbox.KioskEngine.Environment.CardDataReader
{
  public class TrackDataEncryptionService : ITrackDataEncryptionService
  {
    private int _keyId = 99999;
    private bool _useStoreCertificate;

    public int KeyId => this._keyId;

    public void RefreshCertificate()
    {
      int num1 = 0;
      IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
      int num2;
      if (service != null && service.TryGetValue<int>("system", "General", "CertificateName", out num2))
        num1 = num2;
      if (num1 == 0)
      {
        this._keyId = this.GetBundleKeyId();
        this._useStoreCertificate = false;
      }
      else
      {
        this._keyId = num1;
        this._useStoreCertificate = true;
      }
      if (!this._useStoreCertificate || this.GetStoreCertificate() != null)
        return;
      this._keyId = this.GetBundleKeyId();
      this._useStoreCertificate = false;
    }

    public string GetEncryptedAccountNumber(IUnencryptedTrackData unencryptedTrackData)
    {
      return unencryptedTrackData == null || unencryptedTrackData.AccountNumber == null ? (string) null : this.Encrypt(unencryptedTrackData.AccountNumber);
    }

    public SecureString GetEncryptedTrack2(IUnencryptedTrackData unencryptedTrackData)
    {
      return unencryptedTrackData == null ? (SecureString) null : CardHelper.CopyToSecure(this.Encrypt(unencryptedTrackData.Track2));
    }

    private string Encrypt(SecureString value)
    {
      if (this._useStoreCertificate)
      {
        X509Certificate2 storeCertificate = this.GetStoreCertificate();
        return value == null ? (string) null : CardHelper.EncryptCreditCardFromCertificate(value, storeCertificate);
      }
      string bundleCertificateXml = this.GetBundleCertificateXML();
      if (bundleCertificateXml == null)
        return (string) null;
      return value == null ? (string) null : CardHelper.EncryptCreditCardFromXmlKey(value, bundleCertificateXml);
    }

    private string GetBundleCertificateXML()
    {
      IResource manifest = ResourceBundleService.Instance.GetManifest(out IManifestInfo _);
      return manifest == null || manifest["public_key"] == null ? (string) null : ResourceBundleService.Instance.GetXml((string) manifest["public_key"]).InnerXml;
    }

    private X509Certificate2 GetStoreCertificate()
    {
      return Redbox.KioskEngine.Environment.CertificateHelper.GetCertificateBySubjectName(StoreName.My, StoreLocation.LocalMachine, this._keyId.ToString());
    }

    private int GetBundleKeyId()
    {
      IResource manifest = ResourceBundleService.Instance.GetManifest(out IManifestInfo _);
      if (manifest == null || manifest["public_key"] == null)
        return 99999;
      string resourceName = (string) manifest["public_key"];
      if (!ResourceBundleService.Instance.HasActiveBundle())
        return 99999;
      IResource resource = ResourceBundleService.Instance.GetResource(resourceName);
      return resource == null ? 99999 : (int) resource["key_id"];
    }
  }
}
