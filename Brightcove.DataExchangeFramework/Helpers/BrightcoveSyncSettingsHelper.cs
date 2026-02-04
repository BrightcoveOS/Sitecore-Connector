using Brightcove.Core.Exceptions;
using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.DataExchangeFramework.Settings;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Attributes;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Extensions;
using System;

namespace Brightcove.DataExchangeFramework.Helpers
{
    public static class BrightcoveSyncSettingsHelper
    {
        public static void SetErrorFlag(PipelineContext pipelineContext)
        {
            if (pipelineContext == null)
            {
                throw new ArgumentNullException(nameof(pipelineContext));
            }

            var settings = pipelineContext.GetCurrentPipelineBatch().GetPlugin<BrightcoveSyncSettings>();

            if (settings == null || settings.ErrorFlag)
            {
                return;
            }

            settings.ErrorFlag = true;
        }
    }
}
