namespace Sitecore.MediaFramework.Analytics
{
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Globalization;
    using Brightcove.Constants;
    using Brightcove.MediaFramework.Brightcove;
    using Sitecore.Data;

  public class EventProperties
  {
        public NameValueCollection Parameters { get; protected set; }
        public EventProperties(Dictionary<string, string> dictionary)
    {
            foreach (var pair in dictionary)
            {
                this.Parameters.Add(pair.Key, pair.Value);
            }
        }

    public EventProperties(NameValueCollection collection)
    {
            this.Parameters = new NameValueCollection(collection);
        }

    public ID ContextItemId
    {
      get
      {
        return GetId(this.Parameters[PlayerEventParameters.ContextItemId]);
      }
      set
      {
        this.Parameters[PlayerEventParameters.ContextItemId] = value.ToShortID().ToString();
      }
    }

    public string MediaName
    {
      get
      {
        return this.Parameters[PlayerEventParameters.MediaName];
      }
      set
      {
        this.Parameters[PlayerEventParameters.MediaName] = value;
      }
    }

    public int MediaLength
    {
      get
      {
        return MainUtil.GetInt(this.Parameters[PlayerEventParameters.MediaLength], 0);
      }
      set
      {
        this.Parameters[PlayerEventParameters.MediaLength] = value.ToString(CultureInfo.InvariantCulture);
      }
    }

    public string EventParameter
    {
      get
      {
        return this.Parameters[PlayerEventParameters.EventParameter];
      }
      set
      {
        this.Parameters[PlayerEventParameters.EventParameter] = value;
      }
    }

        protected static ID GetId(string value)
        {
            return !string.IsNullOrEmpty(value) && ShortID.IsShortID(value) ? new ID(value) : ID.Null;
        }

        public ID Template
        {
            get
            {
                return GetId(this.Parameters[PlayerParameters.Template]);
            }
            set
            {
                this.Parameters[PlayerParameters.Template] = !ReferenceEquals(value, null) ? value.ToShortID().ToString() : null;
            }
        }

        public string MediaId
        {
            get
            {
                return this.Parameters["mediaId"];
            }
        }
    }
}