using System;
using System.Text.RegularExpressions;
using Brightcove.Constants;
using Brightcove.Core.EmbedGenerator.Models;
using Brightcove.MediaFramework.Brightcove;
using Brightcove.Web.Models;
using Newtonsoft.Json;
using Sitecore;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;

namespace Brightcove.Web.Commands
{
    [Serializable]
    public class EmbedMedia : Command
    {
        private const string GuidRegex = @"[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?";
        private const string ItemIdGroupName = "itemid";
        private static readonly Regex ItemIdRegex = new Regex(@"item(\-)?[I|i]d=(\')?(?'" + ItemIdGroupName + "'" + GuidRegex + ")");

        public override void Execute(CommandContext context)
        {
            Context.ClientPage.Start(this, "Run", context.Parameters);
        }

        /// <summary>
        /// Run Form
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        protected void Run(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                var rendering = GetRendering(ShortID.Decode(args.Parameters["uniqueid"]));
                if (rendering == null)
                {
                    return;
                }
                var urlString = new UrlString(UIUtil.GetUri("control:MediaFramework.EmbedMedia"));
                urlString["mo"] = "webedit";

                if (!string.IsNullOrEmpty(rendering.Parameters))
                {
                    var collection = StringUtil.GetNameValues(rendering.Parameters, '=', '&');
                    foreach (string key in collection)
                    {
                        urlString[key] = collection[key];
                    }
                }

                if (ID.IsID(rendering.Datasource))
                {
                    urlString[PlayerParameters.ItemId] = rendering.Datasource;
                }

                string activePage = args.Parameters[PlayerParameters.ActivePage];
                if (!string.IsNullOrEmpty(activePage))
                {
                    urlString[PlayerParameters.ActivePage] = activePage;
                }

                Context.ClientPage.ClientResponse.ShowModalDialog(urlString.ToString(), "1100", "600", string.Empty, true);
                args.WaitForPostBack();
            }
            else
            {
                Assert.ArgumentNotNull(args, "args");

                if (args.HasResult)
                {
                    var formValue = WebUtil.GetFormValue("scLayout");
                    var id = ShortID.Decode(WebUtil.GetFormValue("scDeviceID"));
                    var uniqueId = ShortID.Decode(args.Parameters["uniqueid"]);
                    var layoutDefinition = WebEditUtil.ConvertJSONLayoutToXML(formValue);
                    var parsedLayout = LayoutDefinition.Parse(layoutDefinition);
                    var device = parsedLayout.GetDevice(id);
                    var deviceIndex = parsedLayout.Devices.IndexOf(device);
                    var index = device.GetIndex(uniqueId);
                    var rendering = (RenderingDefinition)device.Renderings[index];

                    EmbedMarkup embed = JsonConvert.DeserializeObject<EmbedMarkup>(args.Result);
                    rendering.Parameters = new EmbedRenderingParameters(embed.Model).ToString();

                    parsedLayout.Devices[deviceIndex] = device;
                    var updatedLayout = parsedLayout.ToXml();
                    var layout = GetLayout(updatedLayout);
                    SheerResponse.SetAttribute("scLayoutDefinition", "value", layout);
                    SheerResponse.Eval("window.parent.Sitecore.PageModes.ChromeManager.handleMessage('chrome:rendering:propertiescompleted');");
                }
            }
        }

        /// <summary>
        /// Get Rendering
        /// </summary>
        /// <param name="renderingId">
        /// The rendering Id.
        /// </param>
        /// <returns>
        /// returns Rendering as RenderingDefinition
        /// </returns>
        private static RenderingDefinition GetRendering(string renderingId)
        {
            var formValue = WebUtil.GetFormValue("scLayout");
            var id = ShortID.Decode(WebUtil.GetFormValue("scDeviceID"));
            var layoutDefinition = WebEditUtil.ConvertJSONLayoutToXML(formValue);
            var parsedLayout = LayoutDefinition.Parse(layoutDefinition);
            var device = parsedLayout.GetDevice(id);
            var index = device.GetIndex(renderingId);
            return (RenderingDefinition)device.Renderings[index];
        }

        /// <summary>
        /// Get Layout
        /// </summary>
        /// <param name="layout">
        /// The layout.
        /// </param>
        /// <returns>
        /// returns layout as string
        /// </returns>
        private static string GetLayout(string layout)
        {
            Assert.ArgumentNotNull(layout, "layout");
            return WebEditUtil.ConvertXMLLayoutToJSON(layout);
        }
    }
}