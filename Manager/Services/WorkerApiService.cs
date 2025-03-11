using Common;
using System.Text;
using System.Xml.Serialization;

namespace Manager.Services {
    public interface IWorkerApiService {
        public void SendTaskAsync(CrackHashManagerRequest request);

    }

    public class WorkerApiService: IWorkerApiService {
        private readonly ILogger<IWorkerApiService> _logger;
        private readonly IWorkerClient _workerClient;

        public WorkerApiService(IWorkerClient workerClient, 
            ILogger<WorkerApiService> logger) {
            _logger = logger;
            _workerClient = workerClient;
        }

        public async void SendTaskAsync(CrackHashManagerRequest request) {
            var content = SerializeRequest(request);

            var response = await _workerClient.PostCrackAsync(content);

            if (response.IsSuccessStatusCode) {
                _logger.LogInformation("succesfuly send request " + request.RequestId + " to worker");
            } else {
                _logger.LogError("couldn't send request " + request.RequestId + " to worker");
            }
        }

        private StringContent SerializeRequest(CrackHashManagerRequest request) {
            var xmlSerializer = new XmlSerializer(typeof(CrackHashManagerRequest));
            using var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, request);
            var xmlContent = stringWriter.ToString();
            return new StringContent(xmlContent, Encoding.Unicode, "application/xml");
        }
    }
}
