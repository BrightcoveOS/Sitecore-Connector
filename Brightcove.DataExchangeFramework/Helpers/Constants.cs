using System.Linq.Expressions;

namespace Brightcove.DataExchangeFramework.Helpers
{
    public static class FieldName
    {
        public static class Endpoint
        {
            public const string BrightcoveEndpoint = "BrightcoveEndpoint";
            public const string SitecoreEndpoint = "SitecoreEndpoint";
        }

        public static class BrightcoveAccount
        {
            public const string AccountId = "AccountId";
            public const string ClientId = "ClientId";
            public const string ClientSecret = "ClientSecret";
        }

        public static class Mapping
        {
            public const string ModelMappingSets = "ModelMappingSets";
            public const string VariantMappingSets = "VariantMappingSets";
            public const string SourceObjectLocation = "SourceObjectLocation";
            public const string TargetObjectLocation = "TargetObjectLocation";
            public const string SyncDeletionsFromBrightcove = "SyncDeletionsFromBrightcove";
        }

        public const string Account = "Account";
        public const string PropertyName = "PropertyName";
        public const string RelativePath = "RelativePath";
        public const string EndpointFrom = "EndpointFrom";
        public const string TemplateAssetItemLocation = "AssetItemLocation";
        public const string TemplateAssetModelLocation = "AssetModelLocation";
        public const string EnumType = "EnumType";
    }
}
