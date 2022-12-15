using Brightcove.Constants;
using Sitecore.Analytics;
using Sitecore.Analytics.Data;
using Sitecore.MediaFramework;
using Sitecore.MediaFramework.Analytics;

// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The player event trigger.
// </summary>                                                                                
// --------------------------------------------------------------------------------------------------------------------

namespace Brightcove.MediaFramework.Brightcove.Analytics
{
    public class VideoEventTrigger : EventTrigger
    {
        public override void InitEvents()
        {
            this.AddEvent(Templates.Video.Id, PlaybackEvents.PlaybackStarted.ToString(), "Brightcove video is started.");
            this.AddEvent(Templates.Video.Id, PlaybackEvents.PlaybackCompleted.ToString(), "Brightcove video is completed.");
            this.AddEvent(Templates.Video.Id, PlaybackEvents.PlaybackChanged.ToString(), "Brightcove video progress is changed.");
            this.AddEvent(Templates.Video.Id, PlaybackEvents.PlaybackError.ToString(), "Brightcove video playback error.");
        }
        protected override void TriggerEvent(PageEventData eventData)
        {
            if (Sitecore.Configuration.Settings.GetBoolSetting("Xdb.Enabled", false))
            {
                if (!Tracker.IsActive)
                {
                    Tracker.StartTracking();
                }

                if (Tracker.Current.CurrentPage != null)
                {
                    switch (eventData.Name.ToLowerInvariant()){
                        case "playbackstarted":
                            eventData.PageEventDefinitionId = Items.PageEvents.PlaybackStarted.ToGuid();
                            break;
                        case "playbackcompleted":
                            eventData.PageEventDefinitionId = Items.PageEvents.PlaybackCompleted.ToGuid();
                            break;
                        case "playbackchanged":
                            eventData.PageEventDefinitionId = Items.PageEvents.PlaybackChanged.ToGuid();
                            break;
                        case "playbackerror":
                            eventData.PageEventDefinitionId = Items.PageEvents.PlaybackError.ToGuid();
                            break;
                        default:
                            break;
                    }
                    Tracker.Current.CurrentPage.Register(eventData);
                }
            }
        }

    }
}
