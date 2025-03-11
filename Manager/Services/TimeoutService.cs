using Manager.Services.Models;

namespace Manager.Services {
    public interface ITimeoutService {
        void SetTimeoutAsync(WorkerTask task, int timeoutInSeconds);
    }

    public class TimeoutService: ITimeoutService {
        private ILogger<TimeoutService> _logger;

        public TimeoutService(ILogger<TimeoutService> logger) {
            _logger = logger;
        }

        public async void SetTimeoutAsync(WorkerTask task, int timeoutInSeconds) {
            await Task.Delay(timeoutInSeconds * 1000);

            lock (task) {
                if (task.State != RequestState.Ready) {
                    task.State = RequestState.Error;
                    _logger.LogInformation("request " + task.RequestId + " is timed out");
                }
            }
        }
    }
}
