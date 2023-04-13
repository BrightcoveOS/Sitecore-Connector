using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class UpdateFolderModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            base.ProcessPipelineStep(pipelineStep, pipelineContext, logger);

            if(pipelineContext.CriticalError)
            {
                return;
            }

            var resolveAssetModelSettings = pipelineStep.GetPlugin<ResolveAssetModelSettings>();
            if (resolveAssetModelSettings == null)
            {
                logger.Error(
                    "No resolve asset model settings are specified for the pipeline step. " +
                    "(pipeline step: {0})",
                    pipelineStep.Name);

                pipelineContext.CriticalError = true;
                return;
            }

            try
            {
                BrightcoveService service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);
                Folder folder = (Folder)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetModelLocation);
                ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                Item item = Sitecore.Context.ContentDatabase.GetItem(itemModel.GetItemId().ToString());

                //The item has been marked for deletion in Sitecore
                if ((string)itemModel["Delete"] == "1")
                {
                    logger.Info($"Deleting the brightcove model '{folder.Id}' because it has been marked for deletion in Sitecore (pipeline step: {pipelineStep.Name})");
                    service.DeleteFolder(folder.Id);

                    logger.Info($"Deleting the brightcove item '{item.ID}' because it has been marked for deletion in Sitecore (pipeline step: {pipelineStep.Name})");
                    item.Delete();

                    return;
                }

                string itemName = (string)itemModel["Name"];
                DateTime lastSyncTime = DateTime.Parse(item["LastSyncTime"]);

                if (folder.Name != itemName)
                {
                    //If the folder names are different and the folder has not been updated outside of Sitecore since last sync then the name has been modified in Sitecore
                    if (folder.UpdatedDate < lastSyncTime)
                    {
                        //We can only update one field for folders (the name) so it is easier to manually map it
                        folder.Name = itemName;
                        service.UpdateFolder(folder);
                        logger.Info($"Updated the brightcove asset '{folder.Id}'");

                        item.Editing.BeginEdit();
                        item.Name = folder.Name;
                        item.Editing.EndEdit();
                    }
                    else
                    {
                        logger.Warn($"Ignored changes made to brightcove item '{item.ID}' because the brightcove asset '{folder.Id}' has been modified since last sync. Please run the pull pipeline to get the latest changes (pipeline step: {pipelineStep.Name})");
                    }
                }
                else
                {
                    logger.Debug($"Ignored the brightcove item '{item.ID}' because it has not been updated since last sync (pipeline step: {pipelineStep.Name})");
                }
            }
            catch(Exception ex)
            {
                logger.Error($"Failed to update the brightcove model because an unexpected error occured (pipeline step: {pipelineStep.Name}, exception: {ex.Message})");
            }
        }
    }
}
