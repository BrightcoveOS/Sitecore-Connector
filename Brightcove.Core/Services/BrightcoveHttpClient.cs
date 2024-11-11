using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Brightcove.Core.Services
{
    public static class BrightcoveHttpClient
    {
        public readonly static HttpClient Instance;

        static BrightcoveHttpClient()
        {
            Instance = new HttpClient();
            Instance.Timeout = TimeSpan.FromSeconds(30);
        }
    }
}
