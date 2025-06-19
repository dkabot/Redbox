using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.Rental.UI.ControllersLogic
{
    public class BaseLogic : DynamicRouting
    {
        protected static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

        public static SecureString CreateSecureString(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return null;
            var secureString = new SecureString();
            foreach (var c in source) secureString.AppendChar(c);
            return secureString;
        }

        protected static byte[] CreateHashString(string s)
        {
            byte[] array = null;
            using (var sha = SHA256.Create())
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(s)))
                {
                    array = sha.ComputeHash(memoryStream);
                }
            }

            return array;
        }

        public static void HandleWPFHit()
        {
            var service = ServiceLocator.Instance.GetService<IRenderingService>();
            if (service != null) service.ActiveScene.PrcoessWPFHit();
            PostHealthActivity();
        }

        protected static void ProcessWPFHit()
        {
            var service = ServiceLocator.Instance.GetService<IRenderingService>();
            if (service != null) service.ActiveScene.PrcoessWPFHit();
            PostHealthActivity();
        }

        protected static void PostHealthActivity()
        {
            var service = ServiceLocator.Instance.GetService<ITouchScreenHealth>();
            if (service != null) service.PostActivity();
            var service2 = ServiceLocator.Instance.GetService<IViewHealth>();
            if (service2 == null) return;
            service2.PostActivity("Button Press");
        }

        protected static void SetCurrentUICulture()
        {
            var service = ServiceLocator.Instance.GetService<IResourceBundleService>();
            var text = (service != null ? service.Filter["culture"] : null) ?? null;
            if (text != null)
            {
                var name = Thread.CurrentThread.CurrentUICulture.Name;
                if (!text.Equals(name)) Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(text);
            }
        }
    }
}