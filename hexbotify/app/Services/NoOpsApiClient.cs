using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Hexbotify.Services.Models;

namespace Hexbotify.Services
{
    public interface INoOpsApiClient
    {
        Task<HexbotResponse> GetHexbot(int? count, int? width, int? height, string seed);
    }

    public class NoOpsApiClient : INoOpsApiClient
    {
        private readonly string _apiUrl;
        private readonly IApiRequestProvider _requestProvider;
        private readonly IApiClient _client;
        private readonly ILogger _logger;

        public NoOpsApiClient(IOptionsSnapshot<NoOpsChallengeOptions> options, IApiRequestProvider requestProvider, IApiClient client, ILogger<NoOpsApiClient> logger)
        {
            _apiUrl = options?.Value?.NoOpsChallengeApiUrl ?? throw new ArgumentNullException(nameof(options));
            _requestProvider = requestProvider;
            _client = client;
            _logger = logger;
        }

        public async Task<HexbotResponse> GetHexbot(int? count, int? width, int? height, string seed)
        {
            var url = $"{_apiUrl}/hexbot";

            try
            {
                var queries = new Dictionary<string, string>();
                if (count != null) { queries.Add("count", count.ToString()); }
                if (width != null) { queries.Add("width", width.ToString()); }
                if (height != null) { queries.Add("height", height.ToString()); }
                if (!string.IsNullOrWhiteSpace(seed)) { queries.Add("seed", seed); }

                var request = _requestProvider.CreateGetRequest(url, queries: queries);

                var response = await _client.SendAsync(request, async r =>
                {
                    r.EnsureSuccessStatusCode();
                    var content = await r.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeAnonymousType(content, new HexbotResponse());
                });

                return response;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"An error has occurred while trying to call the hexbot API {url}.");
                return null;
            }
            
        }
    }
}