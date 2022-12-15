using Brightcove.Core.Models;
using Brightcove.Core.Services;
using Brightcove.MediaFramework.Brightcove;
using Brightcove.Web.Models;
using Newtonsoft.Json;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Names;
using Sitecore.Sites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sitecore.Diagnostics;
using Brightcove.Constants;

namespace Brightcove.Web.Controllers
{
    public class UploadController : SitecoreController
    {
        [HttpPost]
        public ActionResult Index(string accountItemId, string videoName)
        {
            try
            {
                if (!Sitecore.Context.IsLoggedIn)
                    return Json(new EmptyApiResponse("You are not authorized to upload files."));

                if (string.IsNullOrWhiteSpace(accountItemId) || string.IsNullOrWhiteSpace(videoName))
                    return Json(new EmptyApiResponse("Missing at least one required field, please check the form and try again."));

                BrightcoveService service = GetService(accountItemId);
                Video video = service.CreateVideo(videoName);

                TemporaryIngestUrls ingestUrls = service.GetTemporaryIngestUrls(video.Id);
                ingestUrls.VideoId = video.Id;

                WebApiResponse<TemporaryIngestUrls> response = new WebApiResponse<TemporaryIngestUrls>(ingestUrls);

                return Json(response);
            }
            catch(Exception ex)
            {
                Log.Error("Failed to start file upload", ex, this);
                return Json(new EmptyApiResponse("An error has occured, please try again later."));
            }
        }

        [HttpPost]
        public ActionResult Ingest(string accountItemId, string videoName, string videoId, string videoUrl)
        {
            try
            {
                if (!Sitecore.Context.IsLoggedIn)
                    return Json(new EmptyApiResponse("You are not authorized to upload files."));

                if (string.IsNullOrWhiteSpace(accountItemId) || string.IsNullOrWhiteSpace(videoName) || string.IsNullOrWhiteSpace(videoId) || string.IsNullOrWhiteSpace(videoUrl))
                    return Json(new EmptyApiResponse("Missing at least one required field, please check the form and try again."));

                BrightcoveService service = GetService(accountItemId);
                string jobId = service.IngestVideo(videoId, videoUrl);

                Item accountItem = Sitecore.Context.ContentDatabase.GetItem(new ID(accountItemId));
                string videosPath = accountItem.Paths.Path + "/Videos";

                Item videoItem = Sitecore.Context.ContentDatabase.GetItem(videosPath).Add(ItemUtil.ProposeValidItemName(videoName), new TemplateID(Templates.Video.Id));
                videoItem.Editing.BeginEdit();
                videoItem["ID"] = videoId;
                videoItem["Name"] = videoName;
                videoItem["Ingest Job Id"] = jobId;
                videoItem["Ingest Status"] = "STARTED";
                videoItem.Editing.EndEdit();

                return Json(new WebApiResponse<string>(videoItem.ID.ToString()));
            }
            catch (Exception ex)
            {
                Log.Error("Failed to start file ingest", ex, this);
                return Json(new EmptyApiResponse("An error has occured, please try again later."));
            }
        }

        private BrightcoveService GetService(string accountItemId)
        {
            Item accountItem = Sitecore.Context.ContentDatabase.GetItem(accountItemId);

            string accountId = accountItem?["AccountId"];
            string clientId = accountItem?["ClientId"];
            string clientSecret = accountItem?["ClientSecret"];

            return new BrightcoveService(accountId, clientId, clientSecret);
        }
    }
}