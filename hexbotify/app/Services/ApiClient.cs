using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace Hexbotify.Services
{
    public interface IApiClient
    {
        Task<T> SendAsync<T>(HttpRequestMessage request, Func<HttpResponseMessage, Task<T>> responseMapper);
    }

    public class ApiClient : IApiClient
    {
        private readonly IHttpClientFactory _clientFactory;

        public ApiClient(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage request, Func<HttpResponseMessage, Task<T>> responseMapper)
        {
            using(var client = _clientFactory.CreateClient())
            {
                using(var responseMessage = await client.SendAsync(request))
                {
                    return await responseMapper(responseMessage);
                }
            }
        }
    }
}