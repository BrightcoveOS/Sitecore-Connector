using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Brightcove.Core.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpRequestMessage Clone(this HttpRequestMessage request)
        {
            HttpRequestMessage clone = new HttpRequestMessage(request.Method, request.RequestUri);

            if (request.Content != null)
            {
                var ms = new MemoryStream();

                request.Content.CopyToAsync(ms).Wait();

                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                foreach (var header in request.Content.Headers)
                    clone.Content.Headers.Add(header.Key, header.Value);
            }

            foreach (var property in request.Properties)
                clone.Properties[property.Key] = property.Value;

            foreach (var header in request.Headers)
                clone.Headers.Add(header.Key, header.Value);

            return clone;
        }
    }
}
