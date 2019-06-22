using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Hexbotify.Services;
using Hexbotify.Services.Models;
using Xunit;
using FluentAssertions;
using NSubstitute;

namespace Hexbotify.Tests.Services
{
    public class NoOpsApiClientTests
    {
        private readonly string _noopsUrl;
        private readonly IApiRequestProvider _requestProvider;
        private readonly IApiClient _apiClient;

        private readonly INoOpsApiClient _client;

        public NoOpsApiClientTests()
        {
            _noopsUrl = "noopsUrl";
            var options = Substitute.For<IOptionsSnapshot<NoOpsChallengeOptions>>();
            options.Value.Returns(new NoOpsChallengeOptions { NoOpsChallengeApiUrl = _noopsUrl });

            _requestProvider = Substitute.For<IApiRequestProvider>();

            _apiClient = Substitute.For<IApiClient>();

            _client = new NoOpsApiClient(options, _requestProvider, _apiClient, Substitute.For<ILogger<NoOpsApiClient>>());
        }

        [Fact]
        public async void GetHexbot_Should_Call_IApiRequestProvider_CreateGetRequest()
        {
            Dictionary<string, string> queries = null;
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Do<Dictionary<string, string>>(a => queries = a));

            await _client.GetHexbot(null, null, null, null);

            _requestProvider.Received(1).CreateGetRequest($"{_noopsUrl}/hexbot", Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());
            queries.Should().BeEmpty();
        }

        [Fact]
        public async void GetHexbot_Should_Add_Count_Query_If_Supplied()
        {
            Dictionary<string, string> queries = null;
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Do<Dictionary<string, string>>(a => queries = a));

            var count = 123;
            await _client.GetHexbot(count, null, null, null);

            queries.Should().HaveCount(1);
            queries.Keys.Should().Contain("count");
            queries["count"].Should().Be(count.ToString());
        }

        [Fact]
        public async void GetHexbot_Should_Add_Width_Query_If_Supplied()
        {
            Dictionary<string, string> queries = null;
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Do<Dictionary<string, string>>(a => queries = a));

            var width = 456;
            await _client.GetHexbot(null, width, null, null);

            queries.Should().HaveCount(1);
            queries.Keys.Should().Contain("width");
            queries["width"].Should().Be(width.ToString());
        }

        [Fact]
        public async void GetHexbot_Should_Add_Height_Query_If_Supplied()
        {
            Dictionary<string, string> queries = null;
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Do<Dictionary<string, string>>(a => queries = a));

            var height = 789;
            await _client.GetHexbot(null, null, height, null);

            queries.Should().HaveCount(1);
            queries.Keys.Should().Contain("height");
            queries["height"].Should().Be(height.ToString());
        }

        [Fact]
        public async void GetHexbot_Should_Add_Seed_Query_If_Supplied()
        {
            Dictionary<string, string> queries = null;
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Do<Dictionary<string, string>>(a => queries = a));

            var seed = "seed";
            await _client.GetHexbot(null, null, null, "seed");

            queries.Should().HaveCount(1);
            queries.Keys.Should().Contain("seed");
            queries["seed"].Should().Be(seed.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData("     ")]
        public async void GetHexbot_Should_Not_Add_Seed_Query_If_Whitespace(string seed)
        {
            Dictionary<string, string> queries = null;
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Do<Dictionary<string, string>>(a => queries = a));

            await _client.GetHexbot(null, null, null, seed);

            queries.Should().BeEmpty();
        }

        [Fact]
        public async void GetHexbot_Should_Call_IApiClient_SendAsync()
        {
            var request = new HttpRequestMessage();
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(request);

            Func<HttpResponseMessage, Task<HexbotResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<HexbotResponse>>>(a => responseMapper = a));

            await _client.GetHexbot(null, null, null, null);

            await _apiClient.Received(1).SendAsync(request, Arg.Any<Func<HttpResponseMessage, Task<HexbotResponse>>>());
            responseMapper.Should().NotBeNull();
        }

        [Fact]
        public async void GetHexbot_Should_Check_For_Successful_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<HexbotResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<HexbotResponse>>>(a => responseMapper = a));
            await _client.GetHexbot(null, null, null, null);
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            Func<Task> action = async () => await responseMapper(response);

            action.Should().Throw<HttpRequestException>();
        }

        [Fact]
        public async void GetHexbot_Should_Deserialize_SendAsync_Response()
        {
            Func<HttpResponseMessage, Task<HexbotResponse>> responseMapper = null;
            await _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, Task<HexbotResponse>>>(a => responseMapper = a));
            await _client.GetHexbot(null, null, null, null);

            var content = new HexbotResponse
            {
                Colors = new []
                {
                    new HexbotResponseColor
                    {
                        Value = "value1",
                        Coordinates = new HexbotResponseCoordinates
                        {
                            X = 123,
                            Y = 456
                        }
                    }
                }
            };

            var stringContent = JsonConvert.SerializeObject(content);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(stringContent)
            };

            var actual = await responseMapper(response);

            actual.Should().BeEquivalentTo(content);
        }

        [Fact]
        public async void GetHexbot_Should_Return_Correctly()
        {
            var response = new HexbotResponse();
            _apiClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<Func<HttpResponseMessage, Task<HexbotResponse>>>()).Returns(response);

            var actual = await _client.GetHexbot(null, null, null, null);

            actual.Should().Be(response);
        }

        [Fact]
        public async void GetHexbot_Should_Handle_Exceptions()
        {
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(x => throw new Exception());

            var actual = await _client.GetHexbot(null, null, null, null);

            actual.Should().BeNull();
        }
    }
}