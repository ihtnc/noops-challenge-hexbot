using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Hexbotify.Services;
using Hexbotify.Services.Models;
using Xunit;
using FluentAssertions;
using NSubstitute;

namespace Hexbotify.Tests.Services
{
    public class HexbotifierTests
    {
        private readonly IWebImageClient _imageClient;
        private readonly IHexbotTransformer _transformer;

        private readonly IHexbotifier _hexbotifier;

        public HexbotifierTests()
        {
            _imageClient = Substitute.For<IWebImageClient>();
            _transformer = Substitute.For<IHexbotTransformer>();

            _hexbotifier = new Hexbotifier(_imageClient, _transformer, Substitute.For<ILogger<Hexbotifier>>());
        }

        [Theory]
        [InlineData("canvas", 1)]
        [InlineData(null, 0)]
        public async void Go_Should_Call_IWebImageClient_GetImage_If_Canvas_Parameter_Is_Supplied(string canvas, int expectedCalls)
        {
            await _hexbotifier.Go(null, null, null, null, canvas, null);

            _imageClient.Received(expectedCalls).GetImage<Rgb24>(canvas);
        }

        [Fact]
        public async void Go_Should_Call_IHexbotTransformer_Transform()
        {
            var image = new Image<Rgb24>(100, 100);
            _imageClient.GetImage<Rgb24>(Arg.Any<string>()).Returns(image);

            var count = 123;
            var width = 10;
            var height = 11;
            var seed = "seed";
            var animate = true;
            await _hexbotifier.Go(count, width, height, seed, "canvas", animate);

            await _transformer.Received(1).Transform(image, count, width, height, seed, animate);

            image.Dispose();
        }

        [Theory]
        [InlineData(null, -1, Constants.DEFAULT_CANVAS_WIDTH, Constants.DEFAULT_CANVAS_HEIGHT)]
        [InlineData(0, null, Constants.DEFAULT_CANVAS_WIDTH, Constants.DEFAULT_CANVAS_HEIGHT)]
        [InlineData(-2, 0, Constants.DEFAULT_CANVAS_WIDTH, Constants.DEFAULT_CANVAS_HEIGHT)]
        [InlineData(1, 2, 1, 2)]
        public async void Go_Should_Call_Transform_With_Default_Image_If_GetImage_Returns_Null(int? width, int? height, int expectedWidth, int expectedHeight)
        {
            _imageClient.GetImage<Rgb24>(Arg.Any<string>()).Returns((Image<Rgb24>)null);

            Image<Rgb24> image = null;
            await _transformer.Transform(Arg.Do<Image<Rgb24>>(a => image = a.Clone()), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());

            var canvas = "canvas";
            await _hexbotifier.Go(null, width, height, null, canvas, null);

            _imageClient.Received(1).GetImage<Rgb24>(canvas);
            await _transformer.Received(1).Transform(Arg.Any<Image<Rgb24>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());
            image.Should().NotBeNull();
            image.Width.Should().Be(expectedWidth);
            image.Height.Should().Be(expectedHeight);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async void Go_Should_Call_Transform_With_Default_Image_If_Canvas_Is_Not_Supplied(string canvas)
        {
            Image<Rgb24> image = null;
            await _transformer.Transform(Arg.Do<Image<Rgb24>>(a => image = a.Clone()), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());

            await _hexbotifier.Go(null, null, null, null, canvas, null);

            await _transformer.Received(1).Transform(Arg.Any<Image<Rgb24>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());
            image.Should().NotBeNull();
            image.Width.Should().Be(Constants.DEFAULT_CANVAS_WIDTH);
            image.Height.Should().Be(Constants.DEFAULT_CANVAS_HEIGHT);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-1)]
        public async void Go_Should_Call_Transform_With_Image_Width_If_Width_Is_Invalid(int? width)
        {
            var imageWidth = 111;
            var image = new Image<Rgb24>(imageWidth, 222);
            _imageClient.GetImage<Rgb24>(Arg.Any<string>()).Returns(image);

            await _hexbotifier.Go(null, width, null, null, "canvas", null);

            await _transformer.Received(1).Transform(Arg.Any<Image<Rgb24>>(), Arg.Any<int>(), imageWidth, Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());

            image.Dispose();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-1)]
        public async void Go_Should_Call_Transform_With_Image_Height_If_Height_Is_Invalid(int? height)
        {
            var imageHeight = 222;
            var image = new Image<Rgb24>(111, imageHeight);
            _imageClient.GetImage<Rgb24>(Arg.Any<string>()).Returns(image);

            await _hexbotifier.Go(null, null, height, null, "canvas", null);

            await _transformer.Received(1).Transform(Arg.Any<Image<Rgb24>>(), Arg.Any<int>(), Arg.Any<int>(), imageHeight, Arg.Any<string>(), Arg.Any<bool>());

            image.Dispose();
        }

        [Fact]
        public async void Go_Should_Call_Transform_With_Default_Image_Dimensions_If_Canvas_Width_Height_Are_Not_Supplied()
        {
            await _hexbotifier.Go(null, null, null, null, null, null);

            await _transformer.Received(1).Transform(Arg.Any<Image<Rgb24>>(), Arg.Any<int>(), Constants.DEFAULT_CANVAS_WIDTH, Constants.DEFAULT_CANVAS_HEIGHT, Arg.Any<string>(), Arg.Any<bool>());
        }

        [Theory]
        [InlineData(10, 11, 10)]
        [InlineData(11, 10, 10)]
        public async void Go_Should_Call_Transform_With_The_Lower_Width_Between_Image_And_Parameter(int imageWidth, int width, int expectedWidth)
        {
            var image = new Image<Rgb24>(imageWidth, 2);
            _imageClient.GetImage<Rgb24>(Arg.Any<string>()).Returns(image);

            await _hexbotifier.Go(null, width, null, null, "canvas", null);

            await _transformer.Received(1).Transform(Arg.Any<Image<Rgb24>>(), Arg.Any<int>(), expectedWidth, Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());

            image.Dispose();
        }

        [Theory]
        [InlineData(10, 11, 10)]
        [InlineData(11, 10, 10)]
        public async void Go_Should_Call_Transform_With_The_Lower_Height_Between_Image_And_Parameter(int imageHeight, int height, int expectedHeight)
        {
            var image = new Image<Rgb24>(2, imageHeight);
            _imageClient.GetImage<Rgb24>(Arg.Any<string>()).Returns(image);

            await _hexbotifier.Go(null, null, height, null, "canvas", null);

            await _transformer.Received(1).Transform(Arg.Any<Image<Rgb24>>(), Arg.Any<int>(), Arg.Any<int>(), expectedHeight, Arg.Any<string>(), Arg.Any<bool>());

            image.Dispose();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(-1)]
        [InlineData(0)]
        public async void Go_Should_Call_Transform_With_The_Third_Of_The_Image_Resolution_If_Count_Is_Invalid(int? count)
        {
            var width = 5;
            var height = 6;
            var image = new Image<Rgb24>(width, height);
            _imageClient.GetImage<Rgb24>(Arg.Any<string>()).Returns(image);

            await _hexbotifier.Go(count, null, null, null, "canvas", null);

            var calculatedCount = width * height / 3;
            await _transformer.Received(1).Transform(Arg.Any<Image<Rgb24>>(), calculatedCount, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());

            image.Dispose();
        }

        [Fact]
        public async void Go_Should_Call_Transform_With_Default_Animate_If_Parameter_Is_Not_Supplied()
        {
            await _hexbotifier.Go(null, null, null, null, null, null);

            await _transformer.Received(1).Transform(Arg.Any<Image<Rgb24>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), false);
        }

        [Fact]
        public async void Go_Should_Return_Correctly()
        {
            var response = new HexbotifiedImage();
            _transformer.Transform(Arg.Any<Image<Rgb24>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(response);

            var actual = await _hexbotifier.Go(null, null, null, null, null, null);

            actual.Should().Be(response);
        }
    }
}