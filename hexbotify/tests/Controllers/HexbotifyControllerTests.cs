using Microsoft.AspNetCore.Mvc;
using Hexbotify.Controllers;
using Hexbotify.Services;
using Hexbotify.Services.Models;
using Xunit;
using FluentAssertions;
using NSubstitute;

namespace Hexbotify.Tests.Controllers
{
    public class HexbotifyControllerTests
    {
        private readonly HexbotifyController _controller;

        public HexbotifyControllerTests()
        {
            _controller = new HexbotifyController();
        }

        [Fact]
        public void Class_Should_Include_ApiControllerAttribute()
        {
            var t = _controller.GetType();
            t.Should().BeDecoratedWith<ApiControllerAttribute>();
        }

        [Fact]
        public void Class_Should_Include_RouteAttribute()
        {
            var t = _controller.GetType();
            t.Should().BeDecoratedWith<RouteAttribute>(attr => attr.Template == "api/[controller]");
        }

        [Fact]
        public void Get_Should_Include_HttpGetAttribute()
        {
            var t = _controller.GetType();
            t.GetMethod("Get").Should().BeDecoratedWith<HttpGetAttribute>();
        }

        [Fact]
        public async void Get_Should_Call_IHexbotifier_Go()
        {
            var service = Substitute.For<IHexbotifier>();
            service.Go(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>()).Returns(new HexbotifiedImage { Image = new byte[] { 1 }, ContentType = "image/jpeg" });

            await _controller.Get(service, 123, 456, 789, "seed", "canvas", true);

            await service.Received(1).Go(123, 456, 789, "seed", "canvas", true);
        }

        [Fact]
        public async void Get_Should_Return_Correctly()
        {
            var service = Substitute.For<IHexbotifier>();
            var response = new HexbotifiedImage
            {
                ContentType = "image/jpeg",
                Image = new byte[] { 123, 234, 0 }
            };
            service.Go(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>()).Returns(response);

            var actual = await _controller.Get(service);

            actual.Should().BeOfType<FileContentResult>();
            actual.As<FileContentResult>().ContentType.Should().Be(response.ContentType);
            actual.As<FileContentResult>().FileContents.Should().BeEquivalentTo(response.Image);
        }
    }
}