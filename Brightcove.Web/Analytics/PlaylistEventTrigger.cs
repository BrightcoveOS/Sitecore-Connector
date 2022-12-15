using Brightcove.Constants;
using Sitecore.MediaFramework;
using Sitecore.MediaFramework.Analytics;

namespace Brightcove.MediaFramework.Brightcove.Analytics
{
  public class PlaylistEventTrigger : EventTrigger
  {
    public override void InitEvents()
    {
      this.AddEvent(Templates.Playlist.Id, PlaybackEvents.PlaybackStarted.ToString(), "Brightcove video is started.");
      this.AddEvent(Templates.Playlist.Id, PlaybackEvents.PlaybackCompleted.ToString(), "Brightcove video is completed.");
      this.AddEvent(Templates.Playlist.Id, PlaybackEvents.PlaybackChanged.ToString(), "Brightcove video progress is changed.");
      this.AddEvent(Templates.Playlist.Id, PlaybackEvents.PlaybackError.ToString(), "Brightcove video playback error.");
    }
  }
}
