using Microsoft.AspNetCore.Mvc;
using Common;
using Worker.Services;
using Worker.RabbitMQ;

namespace Worker.Controllers {
    public class WorkerController: Controller {
        private IWorkerService _workerService;
        private IRabbitMQService _rabbitMQService;
        private ILogger<WorkerController> _logger;

        public WorkerController(IWorkerService workerService,
            IRabbitMQService rabbitMQService,
            ILogger<WorkerController> logger) {
            _workerService = workerService;
            _rabbitMQService =rabbitMQService;
            _logger = logger;
        }

    }
}
