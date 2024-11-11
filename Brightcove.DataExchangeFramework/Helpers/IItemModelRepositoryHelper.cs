using Brightcove.Core.Exceptions;
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
using Brightcove.Core.Extensions;
using Sitecore.DataExchange.ApplyMapping;
using Sitecore.Data;

namespace Brightcove.DataExchangeFramework.Helpers
{
    public static class IItemModelRepositoryHelper
    {
        public static string GetIndexName(IItemModelRepository itemModelRepository)
        {
            if (itemModelRepository == null)
            {
                throw new ArgumentNullException(nameof(itemModelRepository));
            }

            return $"sitecore_{itemModelRepository.DatabaseName}_index";
        }
    }
}
