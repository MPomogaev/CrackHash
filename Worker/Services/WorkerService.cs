using Common;
using Worker.RabbitMQ;
using Worker.Services.Crack;
using Worker.Services.Crack.Models;

namespace Worker.Services
{
    public interface IWorkerService {
        public Task CrackAsync(CrackHashManagerRequest request);
    }

    public class WorkerService: IWorkerService
    {
        private readonly ICrackService _crackService;
        private readonly ILogger<WorkerService> _logger;
        private readonly IRabbitMQService _rabbitMQService;

        public WorkerService(ICrackService crackService,
            IRabbitMQService rabbitMQService,
            ILogger<WorkerService> logger) {
            _crackService = crackService;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        public async Task CrackAsync(CrackHashManagerRequest request) {
            _logger.LogInformation("started crack of " + request.RequestId + " request");
            var answers = await _crackService.CrackAsync(new CrackRequest {
                Alphabet = request.Alphabet,
                PartCount = request.PartCount,
                PartNumber = request.PartNumber,
                MaxLength = request.MaxLength,
                Hash = request.Hash
            });

            _logger.LogInformation("sending answer of " + request.RequestId + " request");
            await _rabbitMQService.SendPartAsync(new CrackHashWorkerResponse {
                RequestId = request.RequestId,
                PartNumber = request.PartNumber,
                Answers = answers
            });
        }

    }
}
