using Common;
using Manager.Services.Models;
using System.Collections.Concurrent;

namespace Manager.Services
{
    public interface IWorkerTaskService
    {
        public WorkerTask CreateTask();

        public WorkerTask AddPart(CrackHashWorkerResponse part);

        public WorkerTask? GetTask(Guid requestId);
    }

    public class WorkerTaskService : IWorkerTaskService
    {
        private static readonly ConcurrentDictionary<Guid, WorkerTask> _requests = new();

        private readonly ILogger<WorkerTaskService> _logger;

        public WorkerTaskService(ILogger<WorkerTaskService> logger) {
            _logger = logger;
        }

        public WorkerTask CreateTask()
        {
            var request = new WorkerTask()
            {
                RequestId = Guid.NewGuid(),
            };

            while (!_requests.TryAdd(request.RequestId, request))
            {
                request.RequestId = Guid.NewGuid();
            }

            return request;
        }

        public WorkerTask AddPart(CrackHashWorkerResponse part)
        {
            if (!_requests.TryGetValue(part.RequestId, out var request))
            {
                throw new ArgumentException("there is no stored request with id = " + part.RequestId);
            }

            if (request.ReceivedParts.FirstOrDefault(reqv => reqv.PartNumber == part.PartNumber) != null)
            {
                throw new ArgumentException("part with number = " + part.PartNumber + " is already added");
            }

            request.ReceivedParts.Add(part);
            _logger.LogInformation("added part " + part.PartNumber + " for request " + part.RequestId);
            if (request.ExpectedPartsCount == request.ReceivedParts.Count)
            {
                lock (request) {
                    if (request.State != RequestState.Error) {
                        request.State = RequestState.Ready;
                        _logger.LogInformation("request " + part.RequestId + " is ready");
                    }
                }
            }

            return request;
        }

        public WorkerTask? GetTask(Guid requestId) {
            _requests.TryGetValue(requestId, out var task);
            return task;
        }

        public bool RemoveTask(WorkerTask workerTask) {
            return _requests.Remove(workerTask.RequestId, out _);
        }

    }
}
