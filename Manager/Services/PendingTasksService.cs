using Common;
using Manager.Database;
using Manager.RabbitMQ;
using MongoDB.Driver;

namespace Manager.Services {
    public class PendingTasksService: BackgroundService {
        private IRabbitMQService _rabbitmqService;
        private ICrackHashDatabase _crackHashDatabase;
        private ILogger<PendingTasksService> _logger;

        private List<CrackHashManagerRequest> _requestsToDelete = new();

        private const int SecondsToWait = 3;

        public PendingTasksService(IRabbitMQService rabbitMQService,
            ICrackHashDatabase crackHashDatabase,
            ILogger<PendingTasksService> logger) { 
            _rabbitmqService = rabbitMQService;
            _crackHashDatabase = crackHashDatabase;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                try {
                    await Task.Delay(TimeSpan.FromSeconds(SecondsToWait), cancellationToken);

                    var filter = Builders<CrackHashManagerRequest>.Filter.Empty;

                    using var cursor = await _crackHashDatabase.PendingTasks.FindAsync(filter, null, cancellationToken);

                    if (cursor != null) {
                        await cursor.ForEachAsync(ProcessRequest, cancellationToken);
                    }

                    if (_requestsToDelete.Count != 0) {
                        _crackHashDatabase.PendingTasks.DeleteMany(GetDeleteFilter(), cancellationToken);
                        _requestsToDelete.Clear();
                    }
                } catch(Exception ex) {
                    _logger.LogError("exception ocured while processing pending tasks \n" + ex.Message);
                }
            }
        }

        private async Task ProcessRequest(CrackHashManagerRequest request) {
            if (await _rabbitmqService.TrySendTaskAsync(request)) { 
                _requestsToDelete.Add(request);
            }
        }

        public FilterDefinition<CrackHashManagerRequest> GetDeleteFilter() {
            var filters = _requestsToDelete.Select(req =>
                Builders<CrackHashManagerRequest>.Filter.And(
                    Builders<CrackHashManagerRequest>.Filter.Eq(r => r.RequestId, req.RequestId),
                    Builders<CrackHashManagerRequest>.Filter.Eq(r => r.PartNumber, req.PartNumber)
                )
            );

            return Builders<CrackHashManagerRequest>.Filter.Or(filters);
        }
    }
}
