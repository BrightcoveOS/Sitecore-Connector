using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Converters.PipelineSteps;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Processors.PipelineSteps;
using Sitecore.DataExchange.Repositories;
using Sitecore.SecurityModel;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class BasePipelineStepWithWebApiEndpointProcessor : BasePipelineStepWithEndpointFromProcessor
    {
        protected WebApiSettings WebApiSettings { get; set; }

        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            base.ProcessPipelineStep(pipelineStep, pipelineContext, logger);

            if (pipelineContext.CriticalError)
            {
                return;
            }

            WebApiSettings = EndpointFrom.GetPlugin<WebApiSettings>();

            if (WebApiSettings == null)
            {
                logger.Error(
                    "No web api settings specified on the endpoint. " +
                    "(pipeline step: {0})",
                    pipelineStep.Name);

                pipelineContext.CriticalError = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(WebApiSettings.AccountId))
            {
                logger.Error(
                    "No account ID is specified on the endpoint. " +
                    "(pipeline step: {0})",
                    pipelineStep.Name);

                pipelineContext.CriticalError = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(WebApiSettings.ClientId))
            {
                logger.Error(
                    "No client ID is specified on the endpoint. " +
                    "(pipeline step: {0})",
                    pipelineStep.Name);

                pipelineContext.CriticalError = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(WebApiSettings.ClientSecret))
            {
                logger.Error(
                    "No client secret is specified on the endpoint. " +
                    "(pipeline step: {0})",
                    pipelineStep.Name);

                pipelineContext.CriticalError = true;
                return;
            }
        }

        public void SetFolderSettings(string accountName, string folderName)
        {
            Sitecore.Data.Database masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
            Sitecore.Data.Items.Item node = masterDB.GetItem($"/sitecore/media library/BrightCove/{accountName}/{folderName}");
            if (node == null)
            {
                Sitecore.Data.Items.Item parentNode = masterDB.GetItem($"/sitecore/media library/BrightCove/{accountName}");
                Sitecore.Data.Items.Item folder = masterDB.GetItem("/sitecore/templates/Common/Folder");
                parentNode.Add(folderName, new TemplateItem(folder));

                node = masterDB.GetItem("/sitecore/media library/BrightCove/BrightCove Account/"+ folderName);
                using (new Sitecore.Data.Items.EditContext(node, SecurityCheck.Disable))
                {
                    IsBucketItemCheckBox(node).Checked = true;
                }
            }
            else
            {
                using (new Sitecore.Data.Items.EditContext(node, SecurityCheck.Disable))
                {
                    if (!IsBucketItemCheck(node))
                        IsBucketItemCheckBox(node).Checked = true;
                }
            }
        }

        public bool IsBucketItemCheck(Item item)
        {
            return (((item != null) && (item.Fields[Sitecore.Buckets.Util.Constants.IsBucket] != null)) && item.Fields[Sitecore.Buckets.Util.Constants.IsBucket].Value.Equals("1"));
        }

        public CheckboxField IsBucketItemCheckBox(Item item)
        {
            return item.Fields[Sitecore.Buckets.Util.Constants.IsBucket];
        }
    }
}
