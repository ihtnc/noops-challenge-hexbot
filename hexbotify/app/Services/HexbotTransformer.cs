using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Hexbotify.Services.Models;

namespace Hexbotify.Services
{
    public interface IHexbotTransformer
    {
        Task<HexbotifiedImage> Transform(Image<Rgb24> image, int count, int width, int height, string seed, bool animate);
    }

    public class HexbotTransformer : IHexbotTransformer
    {
        private readonly INoOpsApiClient _apiClient;
        private readonly ILogger<HexbotTransformer> _logger;

        public HexbotTransformer(INoOpsApiClient apiClient, ILogger<HexbotTransformer> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<HexbotifiedImage> Transform(Image<Rgb24> image, int count, int width, int height, string seed, bool animate)
        {
            if(animate)
            {
                if(image.Frames.Count == 1)
                {
                    _logger.LogDebug($"Animate parameter is true but image only has 1 frame. Expanding frames...");
                    image = ExpandFrames(image);
                }

                using(image = await AddHexbotAnimation(image, count, width, height, seed))
                {
                    var response = GetHexbotifiedImage(image);
                    return response;
                }
            }
            else
            {
                using(image = await AddHexbotOverlay(image, count, width, height, seed))
                {
                    var response = GetHexbotifiedImage(image);
                    return response;
                }
            }
        }

        private Image<Rgb24> ExpandFrames(Image<Rgb24> image, int frameCount = 16)
        {
            for(var i = 1; i < frameCount; i++)
            {
                var copy = image.Frames.CloneFrame(0);
                var frame = copy.Frames.First();
                image.Frames.AddFrame(frame);
            }

            return image;
        }

        private async Task<Image<Rgb24>> AddHexbotAnimation(Image<Rgb24> image, int count, int width, int height, string seed)
        {
            _logger.LogDebug($"Adding the responses from hexbot API as an animation in the image...");

            var frameCount = 0;
            HexbotResponse hexbot = null;
            foreach(var frame in image.Frames)
            {
                var needsAnimation = (frameCount++ % Constants.FRAME_PER_HEXBOT == 0);
                hexbot = needsAnimation ? null : hexbot;
                if(needsAnimation) { _logger.LogDebug($"Calling hexbot API once every {Constants.FRAME_PER_HEXBOT} frame(s)..."); }

                hexbot = hexbot ?? await GetHexbot(count, width, height, seed);
                if(hexbot == null) { _logger.LogDebug($"Received invalid hexbot response. Skipping Hexbotification of this frame..."); }
                
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
                    frame[color.Coordinates.X, color.Coordinates.Y] = new Rgb24(r, g, b);
                }
            }

            return image;
        }

        private async Task<Image<Rgb24>> AddHexbotOverlay(Image<Rgb24> image, int count, int width, int height, string seed)
        {
            _logger.LogDebug($"Populating each frame in the image with the response from hexbot API...");

            var hexbot = await GetHexbot(count, width, height, seed);
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
                var pixelColor = new Rgb24(r, g, b);

                _logger.LogTrace($"Updating pixel (x={color.Coordinates.X}, y={color.Coordinates.Y}) color to {color.Value} (r={r}, g={g}, b={b})...");

                foreach(var frame in image.Frames) { frame[color.Coordinates.X, color.Coordinates.Y] = pixelColor; }
            }

            return image;
        }

        private async Task<HexbotResponse> GetHexbot(int count, int width, int height, string seed)
        {
            _logger.LogDebug($"Calling hexbot API (count={count}, width={width}, height={height}, seed={seed ?? "null"})...");

            var response = await _apiClient.GetHexbot(count, width, height, seed);

            if(response != null) { _logger.LogDebug($"Received {response.Colors.Count()} color(s) from hexbot API..."); }
            return response;
        }

        private HexbotifiedImage GetHexbotifiedImage(Image<Rgb24> image)
        {
            var response = new HexbotifiedImage();
            using(var stream = new MemoryStream())
            {
                if(image.Frames.Count > 1)
                {
                    image.SaveAsGif(stream);
                    response.ContentType = "image/gif";
                }
                else
                {
                    image.SaveAsPng(stream);
                    response.ContentType = "image/png";
                }

                response.Image = stream.ToArray();
            }

            return response;
        }
    }
}