using System.Threading.Tasks;

namespace Hexbotify.Services
{
    public interface IHexbotifier
    {
        Task<dynamic> Go(int? count, int? width, int? height, string seed, string imageUrl);
    }

    public class Hexbotifier : IHexbotifier
    {
        private readonly INoOpsApiClient _apiClient;

        public Hexbotifier(INoOpsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<dynamic> Go(int? count, int? width, int? height, string seed, string imageUrl)
        {
            return await _apiClient.GetHexbot(count, width, height, seed);
        }
    }
}