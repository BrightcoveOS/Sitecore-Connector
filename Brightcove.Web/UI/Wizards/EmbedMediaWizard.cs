using System;
using System.Globalization;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using System.Linq;
using Brightcove.Web.Models;
using Brightcove.MediaFramework.Brightcove;
using Sitecore;
using Sitecore.MediaFramework;
using Brightcove.Core;
using Brightcove.Web.Utilities;
using Brightcove.Web.EmbedGenerator;
using Newtonsoft.Json;
using Brightcove.Constants;

namespace Brightcove.Web.UI.Wizards
{
    public class EmbedMediaWizard : WizardForm
    {
        private const string QueryMode = "mo";
        private const string IsPageEdit = "pe";

        private const string SourceFolder = "fo";
        private const string SearchItem = "SearchItem";

        private const string ParameterSetter = "ParameterSetter";

        private ID sourceItemID;
        protected DataContext DataContext;
        protected Edit Filename;
        protected Edit WidthInput;
        protected Edit HeightInput;
        protected Combobox PlayersList;
        protected Edit AssetLanguage;

        protected Literal SourceLiteral;
        protected Literal VideoIdLiteral;

        protected Edit ShowPlaylistHead;
        protected Literal PlaylistIdLiteral;
        protected Literal CreatedLiteral;
        protected Literal UpdatedLiteral;
        protected Literal PlaylistTypeLiteral;
        protected Literal AccountNameLiteral;

        protected Checkbox AutoplayCheckbox;
        protected Checkbox MutedCheckbox;

        protected Radiobutton JavascriptRadiobutton;
        protected Radiobutton IframeRadiobutton;
        protected Edit EmbedInput;

        protected Radiobutton ResponsiveRadiobutton;
        protected Radiobutton FixedRadiobutton;
        protected Edit SizingInput;

        protected Combobox AspectRatioList;

        protected ID SourceItemID
        {
            get
            {
                return this.sourceItemID ?? (this.sourceItemID = this.ServerProperties["itemID"] as ID);
            }
            set
            {
                this.ServerProperties["itemID"] = this.sourceItemID = value;
            }
        }

        protected Language ContentLanguage
        {
            get
            {
                Language contentLanguage;
                if (!Language.TryParse(WebUtil.GetQueryString("la"), out contentLanguage))
                {
                    contentLanguage = Context.ContentLanguage;
                }
                return contentLanguage;
            }
        }

        protected string Mode
        {
            get
            {
                return Assert.ResultNotNull(StringUtil.GetString(this.ServerProperties[QueryMode], "shell"));
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ServerProperties[QueryMode] = value;
            }
        }

        protected ShortID PlayerId
        {
            get
            {
                string str = StringUtil.GetString(this.ServerProperties[PlayerParameters.PlayerId], string.Empty);
                ShortID id;
                return ShortID.TryParse(str, out id)? id : ID.Null.ToShortID();
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ServerProperties[PlayerParameters.PlayerId] = !ReferenceEquals(value, null) ? value.ToString() : null;
            }
        }

        protected virtual void InsertMedia()
        {
            this.InsertMedia(this.GetEmbedParameters());
        }

        protected virtual void InsertMedia(EmbedRenderingParameters parameters)
        {
            if (parameters == null)
            {
                return;
            }

            var generator = new SitecoreEmbedGenerator();
            var result = generator.Generate(parameters);

            switch (this.Mode)
            {
                case "webedit":
                    SheerResponse.SetDialogValue(JsonConvert.SerializeObject(result));
                    this.EndWizard();
                    break;

                default:
                    SheerResponse.Eval("scClose(" + StringUtil.EscapeJavascriptString(result.Markup) + ")");
                    break;
            }
        }

        protected virtual EmbedRenderingParameters GetEmbedParameters()
        {
            var parameters = new EmbedRenderingParameters();
            Item accountItem = MediaItemUtil.GetAccountForMedia(this.SourceItemID);

            parameters.MediaId = MediaItemUtil.GetMediaId(this.SourceItemID);
            parameters.PlayerId = MediaItemUtil.GetMediaId(new ID(this.PlayersList.Value));
            parameters.AccountId = accountItem["AccountId"];

            parameters.Width = int.Parse(this.WidthInput.Value);
            parameters.Height = int.Parse(this.HeightInput.Value);

            parameters.Embed = this.EmbedInput.Value;
            parameters.Sizing = this.SizingInput.Value;

            parameters.Autoplay = this.AutoplayCheckbox.Checked;
            parameters.Muted = this.MutedCheckbox.Checked;
            parameters.Language = this.AssetLanguage.Value;

            parameters.IsPlaylist = false;
            parameters.Title = "";

            if (IsPlaylist(SourceItemID))
            {
                parameters.IsPlaylist = true;
            }
            else
            {
                string customTitleField = accountItem["IframeTitleCustomField"];

                if (!string.IsNullOrWhiteSpace(customTitleField))
                {
                    parameters.Title = ((NameValueListField)GetItem()?.Fields["CustomFields"])?.NameValues[customTitleField] ?? "";
                }

                //If Title still not set then try to fallback. Note the fallback is not a custom field
                if(string.IsNullOrWhiteSpace(parameters.Title))
                {
                    string titleFallbackField = accountItem["IframeTitleFallbackField"];

                    if(!string.IsNullOrWhiteSpace(titleFallbackField))
                    {
                        parameters.Title = GetItem()?.Fields[titleFallbackField]?.Value ?? "";
                    }
                }
            }

            return parameters;
        }

        protected virtual ID InitMediaItem()
        {
            var item = this.GetItem();
            if (item == null)
            {
                SheerResponse.Alert(Translations.MediaItemCouldNotBeFound);
            }
            /*else if (!MediaItemUtil.IsMediaElement(item.Template))
            {
            SheerResponse.Alert(Translations.SelectedItemIsNotMediaElement);
            }*/
            else
            {
                this.InitPlayersList(item);
                return item.ID;
            }

            this.Back();
            return ID.Null;
        }

        protected virtual void InitProperties()
        {
            this.WidthInput.Value = "960";
            this.HeightInput.Value = "540";
            this.InitAspectRatiosList();
            this.JavascriptRadiobutton.Checked = true;
            this.EmbedInput.Value = PlayerParameters.EmbedJavascript;
            this.ResponsiveRadiobutton.Checked = true;
            this.SizingInput.Value = PlayerParameters.SizingResponsive;
            string player = WebUtil.GetQueryString(PlayerParameters.PlayerId, string.Empty);

            this.PlayerId = ShortID.IsShortID(player) ? new ShortID(player) : ID.Null.ToShortID();

            var mediaItemId = WebUtil.GetQueryString(PlayerParameters.ItemId);

            if (ID.IsID(mediaItemId))
            {
                Item item;
                Database db = Sitecore.Context.ContentDatabase ?? Sitecore.Context.Database;

                if (db != null && (item = db.GetItem(new ID(mediaItemId))) != null)
                {
                    this.Filename.Value = item.Paths.MediaPath;
                    this.DataContext.SetFolder(item.Uri);

                    this.SourceItemID = item.ID;
                    this.InitPlayersList(item);

                    string activePage = WebUtil.GetQueryString(PlayerParameters.ActivePage);
                    if (!string.IsNullOrEmpty(activePage))
                    {
                        this.Active = activePage;
                    }
                }
            }
        }
        protected virtual void InitAspectRatiosList()
        {
            this.AspectRatioList.Controls.Clear();
            this.HeightInput.Disabled = true;
            this.AspectRatioList.Controls.Add(new ListItem
            {
                ID = Control.GetUniqueID("ListItem"),
                Selected = true,
                Header = PlayerParameters.Ratio16X9,
                Value = PlayerParameters.Ratio16X9
            });
            this.AspectRatioList.Controls.Add(new ListItem
            {
                ID = Control.GetUniqueID("ListItem"),
                Selected = false,
                Header = PlayerParameters.Ratio4X3,
                Value = PlayerParameters.Ratio4X3
            });
            this.AspectRatioList.Controls.Add(new ListItem
            {
                ID = Control.GetUniqueID("ListItem"),
                Selected = false,
                Header = PlayerParameters.RatioCustom,
                Value = PlayerParameters.RatioCustom
            });
        }

        protected virtual void InitPlayersList(Item item)
        {
            Item accountItem = MediaItemUtil.GetAccountForMedia(item);
            var players = ((MultilistField)accountItem?.Fields["Players"])?.GetItems();

            this.PlayersList.Controls.Clear();
            foreach (var playerItem in players)
            {
                this.PlayersList.Controls.Add(new ListItem
                {
                    ID = Control.GetUniqueID("ListItem"),
                    Selected = true,
                    Header = playerItem.DisplayName,
                    Value = playerItem.ID.ToString()
                });
            }
            Context.ClientPage.ClientResponse.Refresh(this.PlayersList);
        }

        protected virtual bool IsPlaylist(ID item)
        {
            return IsPlaylist(Sitecore.Context.ContentDatabase.GetItem(item));
        }

        protected virtual bool IsPlaylist(Item item)
        {
            return item.TemplateID == Templates.Playlist.Id;
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (!Context.ClientPage.IsEvent)
            {
                this.Mode = WebUtil.GetQueryString(QueryMode);
                this.InitProperties();

                this.DataContext.GetFromQueryString();
                string queryString = WebUtil.GetQueryString(SourceFolder);
                if (ShortID.IsShortID(queryString))
                {
                    queryString = ShortID.Parse(queryString).ToID().ToString();
                    this.DataContext.Folder = queryString;
                }

                var folder = this.DataContext.GetFolder();
                Assert.IsNotNull(folder, "Folder not found");
            }
        }

        protected override void OnCancel(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");

            if (this.Active == ParameterSetter)
            {
                if (this.IsValid())
                {
                    this.InsertMedia();
                }

               return;
            }

            if (this.Mode == "webedit")
            {
                base.OnCancel(sender, args);
            }
            else
            {
                SheerResponse.Eval("scCancel()");
            }
        }

        protected virtual bool IsValid()
        {
            string message = null;

            if (string.IsNullOrEmpty(this.PlayersList.Value))
            {
                message = Translations.PlayerIsNotSelected;
            }

            int width;
            int height;
            if (!int.TryParse(this.WidthInput.Value, out width) || width <= 0)
            {
                message = Translations.IncorrectWidthValue;
            }

            if (!int.TryParse(this.HeightInput.Value, out height) || height <= 0)
            {
                message = Translations.IncorrectHeightValue;
            }

            if (!string.IsNullOrEmpty(message))
            {
                SheerResponse.Alert(message);
                return false;
            }

            return true;
        }

        protected override void OnNext(object sender, EventArgs formEventArgs)
        {
            if (this.Active == SearchItem)
            {
                this.SourceItemID = this.InitMediaItem();
            }

            var item = this.GetItem();
            if (item != null)
            {
                this.PlaylistIdLiteral.Text = item[PlayerParameters.PlaylistId] ?? String.Empty;
                //var account = AccountManager.GetAccountItemForDescendant(item);
                //this.AccountNameLiteral.Text = account?.Name ?? String.Empty;
                this.CreatedLiteral.Text = item[PlayerParameters.PlaylistCreated] ?? String.Empty;
                this.UpdatedLiteral.Text = item[PlayerParameters.PlaylistUpdated] ?? String.Empty;
                this.PlaylistTypeLiteral.Text = item[PlayerParameters.PlaylistType] ?? String.Empty;

                this.SourceLiteral.Text = item.DisplayName ?? String.Empty;
                this.VideoIdLiteral.Text = item[PlayerParameters.MediaId] ?? String.Empty;

                this.ShowPlaylistHead.Value = this.IsPlaylist(item) ? "true" : String.Empty;
                SheerResponse.Eval("scNext()");
            }

            base.OnNext(sender, formEventArgs);
        }

        protected virtual Item GetItem()
        {
            string fullPath = "/sitecore/media library"+this.Filename.Value;

            return Sitecore.Context.ContentDatabase.GetItem(fullPath);
        }
    }
}