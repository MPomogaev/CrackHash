using Common;
using System.Text;
using System.Xml.Serialization;

namespace Worker.Services {
    public interface IManagerApiService {
        public void SendAnswerAsync(CrackHashWorkerResponse response);
    }

    public class ManagerApiService: IManagerApiService {
        private readonly IManagerClient _managerClient;
        private readonly ILogger<ManagerApiService> _logger; 

        public ManagerApiService(IManagerClient managerClient, ILogger<ManagerApiService> logger) {
            _managerClient = managerClient;
            _logger = logger;
        }

        public async void SendAnswerAsync(CrackHashWorkerResponse response) {
            var content = SerializeRequest(response);

            var managerResponse = await _managerClient.PatchAnswerAsync(content);

            if (managerResponse.IsSuccessStatusCode) {
                _logger.LogInformation("succesfuly send answer " + response.RequestId + " to manager");
            } else {
                _logger.LogError("couldn't send answer " + response.RequestId + " to manager");
            }
        }

        private StringContent SerializeRequest(CrackHashWorkerResponse response) {
            var xmlSerializer = new XmlSerializer(typeof(CrackHashWorkerResponse));
            using var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, response);
            var xmlContent = stringWriter.ToString();
            return new StringContent(xmlContent, Encoding.Unicode, "application/xml");
        }
    }
}
