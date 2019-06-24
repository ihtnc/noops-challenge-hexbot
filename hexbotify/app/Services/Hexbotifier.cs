using System;
using System.IO;
using System.Linq;
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
        Task<HexbotifierResponse> Go(int? count, int? width, int? height, string seed, string canvas);
    }

    public class Hexbotifier : IHexbotifier
    {
        private readonly INoOpsApiClient _apiClient;
        private readonly IWebImageClient _imageClient;
        private readonly ILogger _logger;

        public Hexbotifier(INoOpsApiClient apiClient, IWebImageClient imageClient, ILogger<Hexbotifier> logger)
        {
            _apiClient = apiClient;
            _imageClient = imageClient;
            _logger = logger;
        }

        public async Task<HexbotifierResponse> Go(int? count, int? width, int? height, string seed, string canvas)
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
            var hexbot = await GetHexbot(hexCount, imageWidth, imageHeight, seed);
            if(hexbot == null) { _logger.LogDebug($"Received invalid hexbot response. Skipping Hexbotification..."); }

            foreach(var color in hexbot?.Colors ?? new HexbotResponseColor[0])
            {
                if(color.Coordinates?.X == null || color.Coordinates?.Y == null)
                {
                    _logger.LogTrace($"Missing/incomplete coordinates (x={color.Coordinates?.X.ToString() ?? "null"}, y={color.Coordinates?.Y.ToString() ?? "null"}) received for item ({color.Value}). Skipping...");
                    continue;
                }

                var hex = color.Value.TrimStart('#');
                var r = Convert.ToByte(hex.Substring(0, 2), 16);
                var g = Convert.ToByte(hex.Substring(2, 2), 16);
                var b = Convert.ToByte(hex.Substring(4, 2), 16);

                _logger.LogTrace($"Updating pixel (x={color.Coordinates.X}, y={color.Coordinates.Y}) color to {color.Value} (r={r}, g={g}, b={b})...");
                image[color.Coordinates.X, color.Coordinates.Y] = new Rgb24(r, g, b);
            }

            HexbotifierResponse response = null;
            using(var stream = new MemoryStream())
            {
                image.SaveAsPng(stream);

                response = new HexbotifierResponse
                {
                    ContentType = "image/png",
                    Image = stream.ToArray()
                };
            }

            return await Task.FromResult(response);
        }

        private Image<Rgb24> GetImageCanvas(string url)
        {
            _logger.LogDebug($"Getting canvas from {url}...");

            return _imageClient.GetImage<Rgb24>(url);
        }

        private Image<Rgb24> GetDefaultCanvas(int? width, int? height)
        {
            var canvasWidth = width ?? 800;
            var canvasHeight = height ?? 600;

            _logger.LogDebug($"Getting default canvas ({canvasWidth}x{canvasHeight})...");
            var image = new Image<Rgb24>(canvasWidth, canvasHeight);
            image.Mutate(c => c.Fill(new Rgb24(0, 0, 0), new RectangularPolygon(0, 0, canvasWidth, canvasHeight)));
            return image;
        }

        private async Task<HexbotResponse> GetHexbot(int count, int width, int height, string seed)
        {
            _logger.LogDebug($"Calling hexbot API (count={count}, width={width}, height={height}, seed={seed ?? "null"})...");

            var response = await _apiClient.GetHexbot(count, width, height, seed);

            if(response != null) { _logger.LogDebug($"Received {response.Colors.Count()} color(s) from hexbot API..."); }
            return response;
        }
    }
}