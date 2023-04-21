using Brightcove.Core.Models;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Brightcove.Core.Extensions;
using System.Text;

namespace Brightcove.Core.Services
{
    public class BrightcoveAuthenticationService
    {
        readonly string clientId;
        readonly string clientSecret;
        readonly static HttpClient client = BrightcoveHttpClient.Instance;

        public BrightcoveAuthenticationService(string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("argument must not be null or empty", nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentException("argument must not be null or empty", nameof(clientSecret));
            }

            this.clientId = clientId.Trim();
            this.clientSecret = clientSecret.Trim();
        }

        AccessToken GetAccessToken()
        {
            HttpRequestMessage request = new HttpRequestMessage();

            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri("https://oauth.brightcove.com/v4/access_token?grant_type=client_credentials");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));

            HttpResponseMessage response = client.Send(request);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<AccessToken>(response.Content.ReadAsString());
        }

        public AuthenticationHeaderValue CreateAuthenticationHeader()
        {
            AccessToken token = GetAccessToken();
            
            return new AuthenticationHeaderValue(token.TokenType, token.Token);
        }
    }
}
