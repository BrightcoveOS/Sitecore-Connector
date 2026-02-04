using Sitecore.DataExchange.Repositories;
using System;

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
