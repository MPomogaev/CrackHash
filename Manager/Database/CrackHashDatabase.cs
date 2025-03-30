using Common;
using Manager.Services.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Manager.Database {
    public interface ICrackHashDatabase {
        public IMongoCollection<WorkerTask> WorkerTasks { get; }

        public IMongoCollection<CrackHashManagerRequest> PendingTasks { get; }

        public WorkerTask? FindWorkerTask(Guid requestId);
    }

    public class CrackHashDatabase: ICrackHashDatabase {
        private readonly IOptions<CrackHashDatabaseOptions> _options;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<WorkerTask> _workerTasksCollection;
        private readonly IMongoCollection<CrackHashManagerRequest> _pendingTasksCollection;

        public CrackHashDatabase(IOptions<CrackHashDatabaseOptions> options) {
            _options = options;

            var mongoClient = new MongoClient(
            _options.Value.ConnectionString);

            _database = mongoClient.GetDatabase(
                _options.Value.DatabaseName);

            _workerTasksCollection = _database.GetCollection<WorkerTask>("WorkerTasks");

            _pendingTasksCollection = _database.GetCollection<CrackHashManagerRequest>("PendingTasks");
        }

        public WorkerTask FindWorkerTask(Guid requestId) {
            return WorkerTasks
                    .Find(x => x.RequestId == requestId)
                    .FirstOrDefault();
        }

        public IMongoCollection<WorkerTask> WorkerTasks { get => _workerTasksCollection; }

        public IMongoCollection<CrackHashManagerRequest> PendingTasks { get => _pendingTasksCollection; }
    }
}
