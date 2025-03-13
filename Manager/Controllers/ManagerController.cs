using Common;
using Manager.Models;
using Manager.Services;
using Manager.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Manager.Controllers {
    public class ManagerController: Controller {
        private ILogger<ManagerController> _logger;
        private IWorkerApiService _workerApiService;
        private IWorkerTaskService _workerTaskService;
        private ITimeoutService _timeoutService;

        private readonly int _workersCount;
        private readonly List<string> _alphabet;
        private readonly int _timeoutInSeconds;

        public ManagerController(ILogger<ManagerController> logger,
            IWorkerTaskService workerTaskService,
            IWorkerApiService workerApiService,
            ITimeoutService timeoutService,
            IOptions<WorkersOptions> options) {
            _logger = logger;
            _workerApiService = workerApiService;
            _workerTaskService = workerTaskService;
            _timeoutService = timeoutService;

            _workersCount = options.Value.WorkerCount;
            _alphabet = options.Value.Alphabet;
            _timeoutInSeconds = options.Value.TimeoutInSeconds;
        }

        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        [Route("/api/hash/crack")]
        public CrackResponse Crack([FromBody] CrackRequest crackRequest) {
            _logger.LogInformation("started crack for hash " + crackRequest.Hash);

            var workerTask = _workerTaskService.CreateTask(_workersCount);

            for (int i = 0; i < _workersCount; i++) {
                var request = new CrackHashManagerRequest {
                    Hash = crackRequest.Hash,
                    Alphabet = _alphabet,
                    RequestId = workerTask.RequestId,
                    MaxLength = crackRequest.MaxLength,
                    PartCount = _workersCount,
                    PartNumber = i
                };

                _workerApiService.SendTaskAsync(request);
            }

            _timeoutService.SetTimeoutAsync(workerTask, _timeoutInSeconds);

            return new CrackResponse {
                RequestId = workerTask.RequestId
            };
        }

        [HttpPatch]
        [Route("/internal/api/manager/hash/crack/request")]
        [Consumes("application/xml")]
        public void ReceiveWorkerAnswer([FromBody] CrackHashWorkerResponse response) {
            _logger.LogInformation("got answer for request " + response.RequestId + " from worker part " + response.PartNumber);
            _workerTaskService.AddPart(response);
        }

        [HttpGet]
        [Route("/api/hash/status")]
        public StatusResponse GetStatus([FromQuery] Guid requestId) {
            var task = _workerTaskService.GetTask(requestId);
            if (task == null) {
                return new StatusResponse {
                    Status = RequestState.Error.ToString()
                };
            }

            List<string> data = new List<string>();
            if (task.State == RequestState.Ready) {
                foreach(var part in task.ReceivedParts) {
                    data.AddRange(part.Answers);
                }
            }

            return new StatusResponse {
                Status = task.State.ToString(),
                Data = data.Count == 0 ? null : data
            };
        }
    }
}
