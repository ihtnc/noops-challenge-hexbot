using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json.Linq;
using Hexbotify.Services;
using Xunit;
using FluentAssertions;
using FluentAssertions.Json;

namespace Hexbotify.Tests.Services
{
    public class ApiRequestProviderTests
    {
        private readonly IApiRequestProvider _provider;

        public ApiRequestProviderTests()
        {
            _provider = new ApiRequestProvider();
        }

        [Fact]
        public void CreateGetRequest_Should_Set_HttpMethod_Correctly()
        {
            _provider.CreateGetRequest("https://api.noopschallenge.com").Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public void CreateRequest_Should_Set_Uri_Correctly()
        {
            var url = "https://api.noopschallenge.com";
            _provider.CreateRequest(HttpMethod.Get, url).RequestUri.OriginalString.Should().Be(url);
        }

        [Fact]
        public void CreateRequest_Should_Set_HttpMethod_Correctly()
        {
            var url = "https://api.noopschallenge.com";
            _provider.CreateRequest(HttpMethod.Get, url).Method.Should().Be(HttpMethod.Get);
            _provider.CreateRequest(HttpMethod.Put, url).Method.Should().Be(HttpMethod.Put);
            _provider.CreateRequest(HttpMethod.Post, url).Method.Should().Be(HttpMethod.Post);
        }

        [Fact]
        public void CreateRequest_Should_Set_Headers_Correctly()
        {
            var headers = new Dictionary<string, string>
            {
                {"header1", "value1"},
                {"header2", "value2"},
            };

            var request = _provider.CreateRequest(HttpMethod.Get, "https://api.noopschallenge.com", headers: headers);

            foreach(var item in headers)
            {
                request.Headers.GetValues(item.Key).Should().OnlyContain(s => string.Equals(s, item.Value));
            }
        }

        [Fact]
        public void CreateRequest_Should_Handle_Empty_Headers_Parameter()
        {
            var request = _provider.CreateRequest(HttpMethod.Get, "https://api.noopschallenge.com", headers: new Dictionary<string, string>());

            request.Headers.Should().BeEmpty();
        }

        [Fact]
        public void CreateRequest_Should_Handle_Null_Headers_Parameter()
        {
            var request = _provider.CreateRequest(HttpMethod.Get, "https://api.noopschallenge.com");

            request.Headers.Should().BeEmpty();
        }

        [Fact]
        public async void CreateRequest_Should_Set_Content_Correctly()
        {
            var requestObj = new
            {
                StringProp = "string",
                IntProp = 123
            };
            var request = JToken.FromObject(requestObj);

            var actual = _provider.CreateRequest(HttpMethod.Get, "https://api.noopschallenge.com", content: request);

            actual.Content.Headers.GetValues("content-type").Should().OnlyContain(s => string.Equals(s, "application/json; charset=utf-8"));

            var content = await actual.Content.ReadAsStringAsync();
            var token = JToken.Parse(content);
            token.Should().BeEquivalentTo(request);
        }

        [Fact]
        public void CreateRequest_Should_Handle_Null_Content_Parameter()
        {
            var actual = _provider.CreateRequest(HttpMethod.Get, "https://api.noopschallenge.com");

            actual.Content.Should().BeNull();
        }

        [Fact]
        public void CreateRequest_Should_Set_QueryString_Correctly()
        {
            var queries = new Dictionary<string, string>
            {
                {"query1", "value1"},
                {"query2", "value2"},
            };

            var request = _provider.CreateRequest(HttpMethod.Get, "https://api.noopschallenge.com", queries: queries);

            var list = queries.Select(q => $"{q.Key}={q.Value}");
            var queryString = $"?{string.Join('&', list)}";
            request.RequestUri.Query.Should().Be(queryString);
        }

        [Fact]
        public void CreateRequest_Should_Append_QueryString_If_Existing()
        {
            var queries = new Dictionary<string, string>
            {
                {"query1", "value1"},
                {"query2", "value2"},
            };

            var request = _provider.CreateRequest(HttpMethod.Get, "https://api.noopschallenge.com?originalQuery=originalValue", queries: queries);

            var list = queries.Select(q => $"{q.Key}={q.Value}");
            var queryString = $"?originalQuery=originalValue&{string.Join('&', list)}";
            request.RequestUri.Query.Should().Be(queryString);
        }

        [Fact]
        public void CreateRequest_Should_Handle_Empty_Queries_Parameter()
        {
            var request = _provider.CreateRequest(HttpMethod.Get, "https://api.noopschallenge.com", queries: new Dictionary<string, string>());

            request.RequestUri.Query.Should().BeEmpty();
        }

        [Fact]
        public void CreateRequest_Should_Handle_Null_Queries_Parameter()
        {
            var request = _provider.CreateRequest(HttpMethod.Get, "https://api.noopschallenge.com");

            request.RequestUri.Query.Should().BeEmpty();
        }
    }
}