using Sitecore.DataExchange;
using Sitecore.DataExchange.DataAccess;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using Brightcove.Core.Models;

namespace Brightcove.DataExchangeFramework.Settings
{
    public class BrightcoveFolderVideosSettings: IPlugin
    {
        public Folder folderModel;
        public ItemModel folderItem;
        public List<Video> videos = new List<Video>();
    }
}
