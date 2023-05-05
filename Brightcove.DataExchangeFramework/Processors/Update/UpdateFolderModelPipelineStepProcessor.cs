using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Extensions;
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
using Sitecore.Globalization;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class UpdateFolderModelPipelineStepProcessor : BasePipelineStepWithWebApiEndpointProcessor
    {
        protected override void ProcessPipelineStepInternal(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                var resolveAssetModelSettings = GetPluginOrFail<ResolveAssetModelSettings>();
                BrightcoveService service = new BrightcoveService(WebApiSettings.AccountId, WebApiSettings.ClientId, WebApiSettings.ClientSecret);
                Folder folder = (Folder)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetModelLocation);
                ItemModel itemModel = (ItemModel)pipelineContext.GetObjectFromPipelineContext(resolveAssetModelSettings.AssetItemLocation);
                Item item = Sitecore.Context.ContentDatabase.GetItem(itemModel.GetItemId().ToString(), Language.Parse(itemModel.GetLanguage()));

                //The item has been marked for deletion in Sitecore
                if ((string)itemModel["Delete"] == "1")
                {
                    LogInfo($"Deleting the brightcove model '{folder.Id}' because it has been marked for deletion in Sitecore");
                    service.DeleteFolder(folder.Id);

                    LogInfo($"Deleting the brightcove item '{item.ID}' because it has been marked for deletion in Sitecore");
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
                        LogInfo($"Updated the brightcove asset '{folder.Id}'");
                    }
                    else
                    {
                        LogWarn($"Ignored changes made to brightcove item '{item.ID}' because the brightcove asset '{folder.Id}' has been modified since last sync. Please run the pull pipeline to get the latest changes");
                    }
                }
                else
                {
                    LogDebug($"Ignored the brightcove item '{item.ID}' because it has not been updated since last sync");
                }
            }
            catch(Exception ex)
            {
                LogError($"Failed to update the brightcove model because an unexpected error occured", ex);
            }
        }
    }
}
