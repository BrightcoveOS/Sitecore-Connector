using System;
using System.Collections.Generic;
using System.Linq;
using Brightcove.Core.Models;
using Brightcove.DataExchangeFramework.Helpers;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Providers.Sc.Plugins;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;

namespace Brightcove.DataExchangeFramework.Processors
{
    [RequiredEndpointPlugins(new Type[] { typeof(ItemModelRepositorySettings) })]
    // Gets the video items of videos that have been moved from Brightcove folders, but haven't had their models updated in Sitecore
    public class GetMovedFolderVideoItemsPipelineStepProcessor : ReadAssetItemsPipelineStepProcessor
    {
        BrightcoveFolderVideosSettings folderSettings;
        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                BrightcoveFolderVideosSettings folderSettings = pipelineContext.GetPlugin<BrightcoveFolderVideosSettings>();
                this.folderSettings = folderSettings;

                SelectedLanguagesSettings languagesSettings = pipelineContext.GetPlugin<SelectedLanguagesSettings>();

                // Add languages to batch context so all children have access to it
                pipelineContext.GetCurrentPipelineBatch().AddPlugin(languagesSettings);

                base.ProcessPipelineStep(pipelineStep, pipelineContext, logger);
            }
            catch(Exception ex)
            {
                logger.Error("Failed to read sitecore items because an unexpected error occured", ex);
                BrightcoveSyncSettingsHelper.SetErrorFlag(pipelineContext);
                pipelineContext.Finished = true;
                pipelineContext.CriticalError = false;
            }
        }
        
        public override IEnumerable<ItemModel> Search(string bucketPath, string indexName, List<Guid> templateGuids, string language, IItemModelRepository modelRepository)
        {
            var index = ContentSearchManager.GetIndex(indexName);

            using (var context = index.CreateSearchContext())
            {
                List<ItemModel> itemModels = new List<ItemModel>();

                try
                {
                    // Create a map of the current videos in the folder in VC
                    Dictionary<string, Video> currentFolderVideos = new Dictionary<string, Video>();

                    foreach (Video video in folderSettings.videos)
                        currentFolderVideos[video.Id] = video;

                    // Filter the videos in the folder in Sitecore for ones not still in the folder in VC (which means they've moved)
                    ID templateId = new ID(templateGuids[0]);
                    ItemModel folderItem = folderSettings.folderItem;

                    var query = context.GetQueryable<SearchResultItem>()
                        .Where(x => x.Path.Contains(bucketPath) && x.Path != bucketPath && x.Language == language && x.TemplateId == templateId);

                    var searchResults = query.ToList();

                    itemModels = searchResults.Select(r => modelRepository.Get(r.ItemId.ToGuid(), language))
                        .Where(model =>
                        {
                            if (model == null || 
                                model.GetFieldValueAsGuid("BrightcoveFolder") != folderItem.GetItemId() ||
                                currentFolderVideos.ContainsKey(model.GetFieldValueAsString("ID")))
                                return false;
                            return true;
                        })
                        .ToList();

                    Logger.Debug($"{itemModels.Count} videos in Brightcove folder {folderSettings.folderModel.Id} have moved");
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex.Message);
                }

                return itemModels;
            }
        }
    }
}
