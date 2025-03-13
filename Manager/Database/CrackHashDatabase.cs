using Manager.Services.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Manager.Database {
    public interface ICrackHashDatabase {
        public IMongoCollection<WorkerTask> WorkerTasks { get; }

        public WorkerTask? FindWorkerTask(Guid requestId);
    }

    public class CrackHashDatabase: ICrackHashDatabase {
        private readonly IOptions<CrackHashDatabaseOptions> _options;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<WorkerTask> _workerTasksCollection;

        public CrackHashDatabase(IOptions<CrackHashDatabaseOptions> options) {
            _options = options;

            var mongoClient = new MongoClient(
            _options.Value.ConnectionString);

            mongoClient.ListDatabases();

            _database = mongoClient.GetDatabase(
                _options.Value.DatabaseName);

            _workerTasksCollection = _database.GetCollection<WorkerTask>("WorkerTasks");
            var x = _database.GetCollection<WorkerTask>("WorkerTasks")
                .Find(x => x.RequestId == Guid.NewGuid())
                    .FirstOrDefault();
        }

        public WorkerTask FindWorkerTask(Guid requestId) {
            return WorkerTasks
                    .Find(x => x.RequestId == requestId)
                    .FirstOrDefault();
        }

        public IMongoCollection<WorkerTask> WorkerTasks { get => _workerTasksCollection; }
    }
}
