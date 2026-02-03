using Brightcove.DataExchangeFramework.Helpers;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Model;
using System;

namespace Brightcove.DataExchangeFramework.Converters
{
    [SupportedIds("{D79A5F5C-9A5E-4B1C-B884-8E3B97CACA2D}", "{F598D123-2FE9-45D3-99E2-3E4B5063190A}")]
    public class UpdatePipelineStepConverter : BasePipelineStepConverter
    {
        public UpdatePipelineStepConverter(IItemModelRepository repository) : base(repository) { }

        protected override void AddPlugins(ItemModel source, PipelineStep pipelineStep)
        {
            MappingSettings mappingSettings = new MappingSettings()
            {
                ModelMappingSets = ConvertReferencesToModels<IMappingSet>(source, FieldName.Mapping.ModelMappingSets),
                VariantMappingSets = ConvertReferencesToModels<IMappingSet>(source, FieldName.Mapping.VariantMappingSets),
                SourceObjectLocation = GetGuidValue(source, FieldName.Mapping.SourceObjectLocation),
                TargetObjectLocation = GetGuidValue(source, FieldName.Mapping.TargetObjectLocation)
            };

            pipelineStep.AddPlugin(mappingSettings);

            BrightcoveEndpointSettings endpointSettings = new BrightcoveEndpointSettings()
            {
                BrightcoveEndpoint = ConvertReferenceToModel<Endpoint>(source, FieldName.Endpoint.BrightcoveEndpoint),
                SitecoreEndpoint = ConvertReferenceToModel<Endpoint>(source, FieldName.Endpoint.SitecoreEndpoint)
            };

            pipelineStep.AddPlugin(endpointSettings);

            Guid endpointId = GetGuidValue(source, FieldName.Endpoint.BrightcoveEndpoint);

            if(endpointId == null)
            {
                return;
            }

            ItemModel endpointModel = ItemModelRepository.Get(endpointId);

            if(endpointModel == null)
            {
                return;
            }

            Guid accountItemId = GetGuidValue(endpointModel, FieldName.Account);

            if(accountItemId == null)
            {
                return;
            }

            Item accountItem = Sitecore.Context.ContentDatabase.GetItem(new ID(accountItemId));

            if(accountItem == null)
            {
                return;
            }

            WebApiSettings webApiSettings = new WebApiSettings();

            if (accountItem != null)
            {
                webApiSettings.AccountId = accountItem[FieldName.BrightcoveAccount.AccountId];
                webApiSettings.ClientId = accountItem[FieldName.BrightcoveAccount.ClientId];
                webApiSettings.ClientSecret = accountItem[FieldName.BrightcoveAccount.ClientSecret];
            }

            endpointSettings.BrightcoveEndpoint.AddPlugin(webApiSettings);
        }
    }
}