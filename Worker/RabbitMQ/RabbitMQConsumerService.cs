using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Worker.Services;
using Common;

namespace Worker.RabbitMQ {
    public class RabbitMQConsumerService: BackgroundService {
        private RabbitMQServiceOptions _options;
        private ConnectionFactory _connectionFactory;
        private IConnection _connection = null!;
        private IChannel _channel = null!;
        private IWorkerService _workerService;
        private ILogger<RabbitMQService> _logger;

        private const int SecondsToWait = 3;

        public RabbitMQConsumerService(IOptions<RabbitMQServiceOptions> options,
            IWorkerService workerService,
            ILogger<RabbitMQService> logger) {
            _workerService = workerService;
            _options = options.Value;
            _connectionFactory = new ConnectionFactory() {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Password = _options.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                try {
                    await Task.Delay(TimeSpan.FromSeconds(SecondsToWait), cancellationToken);
                    if (await Reconnect()) {
                        _connection = await _connectionFactory.CreateConnectionAsync();
                        _channel = await _connection.CreateChannelAsync();

                        await _channel.QueueDeclareAsync(queue: _options.TasksQueueName,
                           durable: true,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);

                        await SetConsumerAsync();
                    }
                } catch (Exception ex) {
                    _logger.LogError("exception ocured while trying to set consumer to queue\n" + ex.Message);
                }
                
            }
        }

        public override void Dispose() {
            try {
                _logger.LogInformation("consumer has stoped to listen queue " + _options.TasksQueueName);
                _channel.Dispose();
                _connection.Dispose();
                base.Dispose();
            } catch (Exception ex) {
                _logger.LogError("exception happend while trying to dispose consumer connection\n" + ex.Message);
            }
        }

        private async Task<bool> Reconnect() {
            if (_connection == null || _channel == null) {
                _connection = await _connectionFactory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                return true;
            }
            if (!_channel.IsOpen || !_connection.IsOpen) {
                _channel.Dispose();
                _connection.Dispose();
                _connection = await _connectionFactory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                return true;
            }
            return false;
        }

        private async Task SetConsumerAsync() {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (ch, ea) => {
                try {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var request = RequestsSerializer.Deserialize<CrackHashManagerRequest>(message);

                    _logger.LogInformation("consumer got message from request " + request.RequestId);

                    await _workerService.CrackAsync(request);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                } catch (Exception ex) {
                    _logger.LogError("exceprion in consumer: " + ex.Message);
                }
            };

            string consumerTag = await _channel.BasicConsumeAsync(_options.TasksQueueName, false, consumer);
            _logger.LogInformation("consumer has started to listen queue " + _options.TasksQueueName);
        }
    }
}
