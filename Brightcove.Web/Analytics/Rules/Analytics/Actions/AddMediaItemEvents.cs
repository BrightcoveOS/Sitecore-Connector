namespace Sitecore.MediaFramework.Rules.Analytics.Actions
{
    using Brightcove.Constants;
    using Sitecore.Data.Fields;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Rules.Actions;

  public class AddMediaItemEvents<T> : RuleAction<T> where T : PlaybackRuleContext
  {
    public override void Apply(T ruleContext)
    {
      Assert.ArgumentNotNull(ruleContext, "ruleContext");
      Assert.ArgumentNotNull(ruleContext.Item, "ruleContext.Item");

      MultilistField eventsField = ruleContext.Item.Fields[Templates.MediaElement.EventsId];

      if (eventsField == null)
      {
        return;
      }

      foreach (Item item in eventsField.GetItems())
      {
        string eventName = item[Templates.PlaybackEvent.PageEventId];
        if (eventName.Length > 0)
        {
          string parameter = item[Templates.PlaybackEvent.ParameterId];
          ruleContext.AddPlaybackEvent(eventName, parameter, parameter.Length == 0);
        }
      }
    }
  }
}