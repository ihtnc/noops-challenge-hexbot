using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Shapes;
using Hexbotify.Services;
using Xunit;
using FluentAssertions;
using NSubstitute;

namespace Hexbotify.Tests.Services
{
    public class WebImageClientTests
    {
        private readonly IApiRequestProvider _requestProvider;
        private readonly IApiClient _apiClient;

        private readonly IWebImageClient _client;

        public WebImageClientTests()
        {
            _requestProvider = Substitute.For<IApiRequestProvider>();

            _apiClient = Substitute.For<IApiClient>();

            _client = new WebImageClient(_requestProvider, _apiClient, Substitute.For<ILogger<WebImageClient>>());
        }

        [Fact]
        public void GetImage_Should_Call_IApiRequestProvider_CreateGetRequest()
        {
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());

            var imageUrl = "imageUrl";
            _client.GetImage<Rgb24>(imageUrl);

            _requestProvider.Received(1).CreateGetRequest(imageUrl, Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public void GetImage_Should_Call_IApiClient_Send()
        {
            var request = new HttpRequestMessage();
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(request);

            Func<HttpResponseMessage, byte[]> responseMapper = null;
            _apiClient.Send(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, byte[]>>(a => responseMapper = a));

            _client.GetImage<Rgb24>("any");

            _apiClient.Received(1).Send(request, Arg.Any<Func<HttpResponseMessage, byte[]>>());
            responseMapper.Should().NotBeNull();

            request.Dispose();
        }

        [Fact]
        public void GetImage_Should_Check_For_Successful_Send_Response()
        {
            Func<HttpResponseMessage, byte[]> responseMapper = null;
            _apiClient.Send(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, byte[]>>(a => responseMapper = a));
            _client.GetImage<Rgb24>("any");
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            Action action = () => responseMapper(response);

            action.Should().Throw<HttpRequestException>();

            response.Dispose();
        }

        [Fact]
        public void GetImage_Should_Copy_Content_Send_Response()
        {
            Func<HttpResponseMessage, byte[]> responseMapper = null;
            _apiClient.Send(Arg.Any<HttpRequestMessage>(), Arg.Do<Func<HttpResponseMessage, byte[]>>(a => responseMapper = a));
            _client.GetImage<Rgb24>("any");

            var content = new byte[] { 123, 234, 0 };

            var response = new HttpResponseMessage
            {
                Content = new ByteArrayContent(content)
            };

            var actual = responseMapper(response);

            actual.Should().BeEquivalentTo(content);
        }

        [Fact]
        public void GetImage_Should_Return_Correctly()
        {
            var memoryStream = new MemoryStream();
            var image = new Image<Rgb24>(2, 2);
            image.Mutate(c => c.Fill(new Rgb24(0, 0, 0), new RectangularPolygon(0, 0, 2, 2)));
            image.SaveAsPng(memoryStream);
            var bytes = memoryStream.ToArray();

            _apiClient.Send(Arg.Any<HttpRequestMessage>(), Arg.Any<Func<HttpResponseMessage, byte[]>>()).Returns(bytes);

            var actual = _client.GetImage<Rgb24>("any");

            var actualStream = new MemoryStream();
            actual.SaveAsPng(actualStream);
            var actualBytes = actualStream.ToArray();
            actualBytes.Should().BeEquivalentTo(bytes);

            memoryStream.Dispose();
            image.Dispose();
            actualStream.Dispose();
        }

        [Fact]
        public void GetImage_Should_Handle_Exceptions()
        {
            _requestProvider.CreateGetRequest(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<Dictionary<string, string>>()).Returns(x => throw new Exception());

            var actual = _client.GetImage<Rgb24>("any");

            actual.Should().BeNull();
        }
    }
}