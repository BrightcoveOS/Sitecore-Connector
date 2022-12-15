// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerEventsRecorder.asmx.cs" company="Sitecore A/S">
//   Copyright (C) 2013 by Sitecore A/S
// </copyright>
// <summary>
//   Player events recorder.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.MediaFramework.Analytics
{
    using System.Collections.Generic;
    using System.Web.Script.Services;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using Brightcove.Constants;
    using Brightcove.MediaFramework.Brightcove.Analytics;
    using Brightcove.Web.Utilities;
    using Newtonsoft.Json;
    using Sitecore.Data;
    using Sitecore.Diagnostics;
    using Sitecore.MediaFramework.Pipelines.Analytics;

    /// <summary>
    /// Player events recorder.
    /// </summary>
    [WebService]
    [ScriptService]
    public class PlayerEventsRecorder : WebService
    {
        private Dictionary<ID, IEventTrigger> EventTriggers = new Dictionary<ID, IEventTrigger>();

        /// <summary>
        /// Records an event.
        /// </summary>
        /// <param name="eventName">
        /// The event name.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        [WebMethod(EnableSession = true)]
        [ScriptMethod]
        public void RecordEvent(string eventName, string parameters)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(parameters);

            if (dict != null)
            {
                var args = new PlayerEventArgs { EventName = eventName, Properties = new EventProperties(dict) };
                Assert.ArgumentNotNull(args, "args");
                Assert.ArgumentNotNull(args.Properties, "args.Parameters");

                InitPlayerEventsTriggers();

                IEventTrigger trigger = EventTriggers[args.Properties.Template];
                if (trigger != null)
                {
                    args.Trigger = trigger;
                    args.Trigger.Register(args);
                }
                else
                {
                    LogHelper.Warn("Player event trigger couldn't be determine for the player.", this);
                }
            }
            else
            {
                throw new SoapException("Parameters could not be parsed", SoapException.ServerFaultCode);
            }
        }

        protected virtual void InitPlayerEventsTriggers()
        {
            EventTriggers.Add(Templates.Video.Id, new VideoEventTrigger());
            EventTriggers.Add(Templates.Playlist.Id, new PlaylistEventTrigger());

            foreach (IEventTrigger eventTrigger in EventTriggers.Values)
            {
                eventTrigger.InitEvents();
            }
        }
    }
}