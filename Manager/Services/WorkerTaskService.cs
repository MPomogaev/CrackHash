using Common;
using Manager.Database;
using Manager.Services.Models;
using MongoDB.Driver;

namespace Manager.Services
{
    public interface IWorkerTaskService
    {
        public WorkerTask CreateTask(int expectedPartsCount);

        public WorkerTask AddPart(CrackHashWorkerResponse part);

        public WorkerTask? GetTask(Guid requestId);

        public void UpdateTask(WorkerTask task);
    }

    public class WorkerTaskService : IWorkerTaskService
    {
        private readonly ILogger<WorkerTaskService> _logger;
        private readonly ICrackHashDatabase _db;

        public WorkerTaskService(ILogger<WorkerTaskService> logger, ICrackHashDatabase db) {
            _logger = logger;
            _db = db;
        }

        public WorkerTask CreateTask(int expectedPartsCount)
        {
            var request = new WorkerTask()
            {
                RequestId = Guid.NewGuid(),
                ExpectedPartsCount = expectedPartsCount
            };

            lock (_db) {
                while (_db.FindWorkerTask(request.RequestId) != null) {
                    request.RequestId = Guid.NewGuid();
                }
            }

            _db.WorkerTasks.InsertOne(request);

            return request;
        }

        public WorkerTask AddPart(CrackHashWorkerResponse part)
        {
            lock (_db) {
                var request = _db.FindWorkerTask(part.RequestId);
                if (request == null) {
                    throw new ArgumentException("there is no stored request with id = " + part.RequestId);
                }

                if (request.ReceivedParts.FirstOrDefault(reqv => reqv.PartNumber == part.PartNumber) != null) {
                    throw new ArgumentException("part with number = " + part.PartNumber + " is already added");
                }

                request.ReceivedParts.Add(part);
                _logger.LogInformation("added part " + part.PartNumber + " for request " + part.RequestId);
                if (request.ExpectedPartsCount == request.ReceivedParts.Count
                    && request.State != RequestState.Error) {

                    request.State = RequestState.Ready;
                    _logger.LogInformation("request " + part.RequestId + " is ready");
                }

                _db.WorkerTasks
                    .ReplaceOne(x => x.RequestId == request.RequestId, request);

                return request;
            }
        }

        public WorkerTask? GetTask(Guid requestId) {
            return _db.FindWorkerTask(requestId);
        }

        public void UpdateTask(WorkerTask task) {
            lock (_db) {
                _db.WorkerTasks
                    .ReplaceOne(x => x.RequestId == task.RequestId, task);
            }
        }

    }
}
