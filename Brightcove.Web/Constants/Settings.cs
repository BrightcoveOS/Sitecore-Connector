using Sitecore.SecurityModel.License;

namespace Brightcove.Constants
{
    public static class Settings
    {
        public static bool AnalyticsEnabled
        {
            get
            {
                var analyticsEnabled = false;
                var xdbEnabled = Sitecore.Configuration.Settings.GetBoolSetting("Xdb.Enabled", false);
                var hasLicense = License.HasModule("Sitecore.xDB.Base") || License.HasModule("Sitecore.xDB.Plus") || (License.HasModule("Sitecore.xDB.Premium") || License.HasModule("Sitecore.xDB.Base.Cloud")) || (License.HasModule("Sitecore.xDB.Plus.Cloud") || License.HasModule("Sitecore.xDB.Premium.Cloud")) || License.HasModule("Sitecore.xDB.Enterprise.Cloud") || License.HasModule("Sitecore.OMS");

                analyticsEnabled = xdbEnabled && hasLicense;
                return analyticsEnabled;
            }
        }
    }
}