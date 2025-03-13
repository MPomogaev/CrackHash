using Manager.Services.Models;

namespace Manager.Services {
    public interface ITimeoutService {
        void SetTimeoutAsync(WorkerTask task, int timeoutInSeconds);
    }

    public class TimeoutService: ITimeoutService {
        private ILogger<TimeoutService> _logger;
        private IWorkerTaskService _workerTaskService;

        public TimeoutService(ILogger<TimeoutService> logger, IWorkerTaskService workerTaskService) {
            _logger = logger;
            _workerTaskService = workerTaskService;
        }

        public async void SetTimeoutAsync(WorkerTask request, int timeoutInSeconds) {
            await Task.Delay(timeoutInSeconds * 1000);

            var task = _workerTaskService.GetTask(request.RequestId);

            if (task == null) {
                throw new NullReferenceException("timeout set for task not found in database");
            }

            if (task.State != RequestState.Ready) {
                task.State = RequestState.Error;
                _workerTaskService.UpdateTask(task);
                _logger.LogInformation("request " + task.RequestId + " is timed out");
            }
        }
    }
}
