namespace Worker.Services {
    public interface IManagerClient {
        public Task<HttpResponseMessage> PatchAnswerAsync(StringContent content);
    }

    public class ManagerClient: IManagerClient {
        const string ManagerUrl = "http://manager:8080/internal/api/manager/hash/crack/request";

        private readonly HttpClient _httpClient;

        public ManagerClient(IHttpClientFactory httpClientFactory) {
            _httpClient = httpClientFactory?.CreateClient();
        }


        public async Task<HttpResponseMessage> PatchAnswerAsync(StringContent content) {
            return await _httpClient.PatchAsync(ManagerUrl, content);
        }
    }
}
