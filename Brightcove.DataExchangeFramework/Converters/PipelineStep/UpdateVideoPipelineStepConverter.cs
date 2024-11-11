using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Providers.Sc.Converters.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;

namespace Brightcove.DataExchangeFramework.Converters
{
    [SupportedIds("{D79A5F5C-9A5E-4B1C-B884-8E3B97CACA2D}", "{F598D123-2FE9-45D3-99E2-3E4B5063190A}")]
    public class UpdateVideoPipelineStepConverter : BasePipelineStepConverter
    {
        public UpdateVideoPipelineStepConverter(IItemModelRepository repository) : base(repository) { }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            MappingSettings mappingSettings = new MappingSettings()
            {
                ModelMappingSets = this.ConvertReferencesToModels<IMappingSet>(source, "ModelMappingSets"),
                VariantMappingSets = this.ConvertReferencesToModels<IMappingSet>(source, "VariantMappingSets"),
                SourceObjectLocation = this.GetGuidValue(source, "SourceObjectLocation"),
                TargetObjectLocation = this.GetGuidValue(source, "TargetObjectLocation")
            };

            pipelineStep.AddPlugin<MappingSettings>(mappingSettings);

            BrightcoveEndpointSettings endpointSettings = new BrightcoveEndpointSettings()
            {
                BrightcoveEndpoint = this.ConvertReferenceToModel<Endpoint>(source, "BrightcoveEndpoint"),
                SitecoreEndpoint = this.ConvertReferenceToModel<Endpoint>(source, "SitecoreEndpoint")
            };

            pipelineStep.AddPlugin<BrightcoveEndpointSettings>(endpointSettings);

            Guid endpointId = this.GetGuidValue(source, "BrightcoveEndpoint");

            if(endpointId == null)
            {
                return;
            }

            ItemModel endpointModel = ItemModelRepository.Get(endpointId);

            if(endpointModel == null)
            {
                return;
            }

            Guid accountItemId = this.GetGuidValue(endpointModel, "Account");

            if(accountItemId == null)
            {
                return;
            }

            ItemModel accountItem = ItemModelRepository.Get(accountItemId);

            if(accountItem == null)
            {
                return;
            }

            WebApiSettings webApiSettings = new WebApiSettings();

            if (accountItem != null)
            {
                webApiSettings.AccountId = this.GetStringValue(accountItem, "AccountId") ?? "";
                webApiSettings.ClientId = this.GetStringValue(accountItem, "ClientId") ?? "";
                webApiSettings.ClientSecret = this.GetStringValue(accountItem, "ClientSecret") ?? "";
            }

            endpointSettings.BrightcoveEndpoint.AddPlugin(webApiSettings);
        }
    }
}