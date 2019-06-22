using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Hexbotify.Services
{
    public interface IApiRequestProvider
    {
        HttpRequestMessage CreateGetRequest(string url, Dictionary<string, string> headers = null, Dictionary<string, string> queries = null);
        HttpRequestMessage CreateRequest(HttpMethod method, string url, Dictionary<string, string> headers = null, JToken content = null, Dictionary<string, string> queries = null);
    }

    public class ApiRequestProvider : IApiRequestProvider
    {
        public HttpRequestMessage CreateGetRequest(string url, Dictionary<string, string> headers = null, Dictionary<string, string> queries = null)
        {
            return CreateRequest(HttpMethod.Get, url, headers: headers, queries: queries);
        }

        public HttpRequestMessage CreateRequest(HttpMethod method, string url, Dictionary<string, string> headers = null, JToken content = null, Dictionary<string, string> queries = null)
        {
            var requestUrl = url;

            if (queries?.Count > 0)
            {
                var checkUri = new Uri(url);
                var urlHasQueryString = checkUri.Query?.StartsWith('?') == true;
                var list = queries.Select(q => $"{q.Key}={q.Value}");
                var concatenated = string.Join("&", list);
                var queryString = (urlHasQueryString ? "&" : "?") + concatenated;
                requestUrl = url + queryString;
            }

            var message = new HttpRequestMessage(method, requestUrl);

            if (headers?.Count > 0)
            {
                foreach(var item in headers)
                {
                    message.Headers.Add(item.Key, item.Value);
                }
            }

            if (content != null)
            {
                message.Content = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            }

            return message;
        }
    }
}