using Sitecore.DataExchange;
using Sitecore.Services.Core.Model;
using System;

namespace Brightcove.DataExchangeFramework.Settings
{
    public class BrightcoveSyncSettings : IPlugin
    {
        public ItemModel AccountItem { get; set; }

        public DateTime LastSyncStartTime;

        public DateTime LastSyncFinishTime;

        public DateTime StartTime;
        
        public bool ErrorFlag { get; set; }
    }
}