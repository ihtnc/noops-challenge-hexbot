using System;
using System.IO;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Hexbotify.Services
{
    public interface IWebImageClient
    {
        Image<TPixel> GetImage<TPixel>(string url) where TPixel : struct, IPixel<TPixel>;
    }

    public class WebImageClient : IWebImageClient
    {
        private readonly IApiRequestProvider _requestProvider;
        private readonly IApiClient _client;
        private readonly ILogger _logger;

        public WebImageClient(IApiRequestProvider requestProvider, IApiClient client, ILogger<WebImageClient> logger)
        {
            _requestProvider = requestProvider;
            _client = client;
            _logger = logger;
        }

        public Image<TPixel> GetImage<TPixel>(string url) where TPixel : struct, IPixel<TPixel>
        {
            try
            {
                var request = _requestProvider.CreateGetRequest(url);

                var bytes = _client.Send(request, r =>
                {
                    r.EnsureSuccessStatusCode();

                    var readTask = r.Content.ReadAsStreamAsync();
                    readTask.Wait();
                    using(var content = readTask.Result)
                    {
                        using(var memoryStream = new MemoryStream())
                        {
                            content.CopyTo(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                });

                var image = Image.Load<TPixel>(bytes);
                return image;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"An error has occurred while trying to retrieve image {url}.");
                return null;
            }
        }
    }
}