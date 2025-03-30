using Common;
using Manager.Database;
using Manager.Models;
using Manager.RabbitMQ;
using Manager.Services;
using Manager.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Manager.Controllers {
    public class ManagerController: Controller {
        private ILogger<ManagerController> _logger;
        private IWorkerTaskService _workerTaskService;
        private IRabbitMQService _rabbitMQService;
        private ITimeoutService _timeoutService;
        private ICrackHashDatabase _crackHashDatabase;

        private readonly int _workersCount;
        private readonly List<string> _alphabet;
        private readonly int _timeoutInSeconds;

        public ManagerController(ILogger<ManagerController> logger,
            IWorkerTaskService workerTaskService,
            IRabbitMQService rabbitMQService,
            ITimeoutService timeoutService,
            ICrackHashDatabase crackHashDatabase,
            IOptions<WorkersOptions> options) {
            _logger = logger;
            _rabbitMQService = rabbitMQService;
            _crackHashDatabase = crackHashDatabase;
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
        public async Task<CrackResponse> Crack([FromBody] CrackRequest crackRequest) {
            _logger.LogInformation("started crack for hash " + crackRequest.Hash);

            var workerTask = _workerTaskService.CreateTask(_workersCount);

            Task.Run(async () => await SendWorkerTasks(crackRequest, workerTask));

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

        private async Task SendWorkerTasks(CrackRequest crackRequest, WorkerTask workerTask) {
            for (int i = 0; i < _workersCount; i++) {
                var request = new CrackHashManagerRequest {
                    Hash = crackRequest.Hash,
                    Alphabet = _alphabet,
                    RequestId = workerTask.RequestId,
                    MaxLength = crackRequest.MaxLength,
                    PartCount = _workersCount,
                    PartNumber = i
                };

                if (!await _rabbitMQService.TrySendTaskAsync(request)) {
                    _crackHashDatabase.PendingTasks.InsertOne(request);
                } else {
                    _logger.LogInformation("send request " + request.RequestId + " to worker " + request.PartCount + " in queue");
                }
            }

            _timeoutService.SetTimeoutAsync(workerTask, _timeoutInSeconds);
        }
    }
}
