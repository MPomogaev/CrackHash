using Microsoft.AspNetCore.Mvc;
using Common;
using Worker.Services;

namespace Worker.Controllers {
    public class WorkerController: Controller {
        private IWorkerService _workerService;
        private ILogger<WorkerController> _logger;

        public WorkerController(IWorkerService workerService,
            ILogger<WorkerController> logger) {
            _workerService = workerService;
            _logger = logger;
        }

        [HttpPost]
        [Route("/internal/api/worker/hash/crack/task")]
        [Consumes("application/xml")]
        public void CrackTask([FromBody] CrackHashManagerRequest request) {
            _logger.LogInformation("part " + request.PartNumber + " got crack request for hash " + request.Hash);
            _workerService.CrackAsync(request);
        }

    }
}
