using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;
using Common;

namespace Manager.RabbitMQ {
    public interface IRabbitMQService {

        public Task<bool> TrySendTaskAsync(CrackHashManagerRequest request, string contentType = "application/xml");
    }

    public class RabbitMQService: IRabbitMQService {
        private RabbitMQServiceOptions _options;
        private ConnectionFactory _connectionFactory;
        private ILogger<RabbitMQService> _logger;

        public RabbitMQService(IOptions<RabbitMQServiceOptions> options, ILogger<RabbitMQService> logger) {
            _options = options.Value;
            _connectionFactory = new ConnectionFactory() {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Password = _options.Password,
            };
            _logger = logger;
        }

        public async Task<bool> TrySendTaskAsync(CrackHashManagerRequest request, string contentType = "application/xml") {
            try {
                using var connection = await _connectionFactory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();
                await channel.QueueDeclareAsync(queue: _options.TasksQueueName,
                               durable: true,
                               exclusive: false,
                               autoDelete: false,
                               arguments: null);

                var content = RequestsSerializer.Serialize(request);
                var body = Encoding.UTF8.GetBytes(content);

                var props = new BasicProperties();
                props.ContentType = contentType;
                props.DeliveryMode = DeliveryModes.Persistent;
                props.Persistent = true;

                await channel.BasicPublishAsync(String.Empty, _options.TasksQueueName, true, props, body);
                _logger.LogInformation("published message to queue " + _options.TasksQueueName);
                return true;
            } catch (Exception ex) {
                _logger.LogError("exception ocured while trying to publish task " + request.RequestId + " to queue\n" + ex.Message);
                return false;
            }
        }
    }
}
