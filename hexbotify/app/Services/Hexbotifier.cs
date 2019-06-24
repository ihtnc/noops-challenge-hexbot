using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Shapes;
using Hexbotify.Services.Models;

namespace Hexbotify.Services
{
    public interface IHexbotifier
    {
        Task<HexbotifiedImage> Go(int? count, int? width, int? height, string seed, string canvas, bool? animate);
    }

    public class Hexbotifier : IHexbotifier
    {
        private readonly IWebImageClient _imageClient;
        private readonly IHexbotTransformer _transformer;
        private readonly ILogger _logger;

        public Hexbotifier(IWebImageClient imageClient, IHexbotTransformer transformer, ILogger<Hexbotifier> logger)
        {
            _imageClient = imageClient;
            _transformer = transformer;
            _logger = logger;
        }

        public async Task<HexbotifiedImage> Go(int? count, int? width, int? height, string seed, string canvas, bool? animate)
        {
            width = width != null && width.Value > 0 ? width.Value : (int?)null;
            height = height != null && height.Value > 0 ? height.Value : (int?)null;

            var image = !string.IsNullOrWhiteSpace(canvas) ? GetImageCanvas(canvas) : null;
            image = image ?? GetDefaultCanvas(width, height);

            _logger.LogDebug($"Adjusting width ({width?.ToString() ?? "null"}) and height ({height?.ToString() ?? "null"}) parameters to image dimension ({image.Width}x{image.Height}) as necessary...");
            var imageWidth = Math.Min(image.Width, width ?? image.Width);
            var imageHeight = Math.Min(image.Height, height ?? image.Height);

            var hasValidCount = count != null && count > 0;
            if(!hasValidCount) { _logger.LogDebug($"Defaulting count parameter to {(imageWidth * imageHeight) / 3} ({image.Width}x{image.Height}/3)..."); }
            var hexCount = hasValidCount ? count.Value : (imageWidth * imageHeight) / 3;

            var needsAnimation = animate ?? false;
            if(animate == null) { _logger.LogDebug($"Defaulting animate parameter to false..."); }

            var response = await _transformer.Transform(image, hexCount, imageWidth, imageHeight, seed, needsAnimation);
            image.Dispose();
            return await Task.FromResult(response);
        }

        private Image<Rgb24> GetImageCanvas(string url)
        {
            _logger.LogDebug($"Getting canvas from {url}...");

            return _imageClient.GetImage<Rgb24>(url);
        }

        private Image<Rgb24> GetDefaultCanvas(int? width, int? height)
        {
            var canvasWidth = width ?? Constants.DEFAULT_CANVAS_WIDTH;
            var canvasHeight = height ?? Constants.DEFAULT_CANVAS_HEIGHT;

            _logger.LogDebug($"Getting default canvas ({canvasWidth}x{canvasHeight})...");
            var image = new Image<Rgb24>(canvasWidth, canvasHeight);
            image.Mutate(c => c.Fill(new Rgb24(0, 0, 0), new RectangularPolygon(0, 0, canvasWidth, canvasHeight)));
            return image;
        }
    }
}