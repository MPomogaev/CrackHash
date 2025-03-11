namespace Manager.Services {
    public interface IWorkerClient {
        public Task<HttpResponseMessage> PostCrackAsync(StringContent content);
    }

    public class WorkerClient: IWorkerClient {
        const string WokerUrl = "http://worker:8080/internal/api/worker/hash/crack/task";

        private readonly HttpClient _httpClient;

        public WorkerClient(IHttpClientFactory httpClientFactory) {
            _httpClient = httpClientFactory?.CreateClient();
        }

        public async Task<HttpResponseMessage> PostCrackAsync(StringContent content) {
            return await _httpClient.PostAsync(WokerUrl, content);
        }

    }
}
