using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;
using Common;

namespace Worker.RabbitMQ {
    public interface IRabbitMQService {
        public Task SendPartAsync(CrackHashWorkerResponse response, string contentType = "application/xml");

    }

    public class RabbitMQService: IRabbitMQService {
        private RabbitMQServiceOptions _options;
        private ConnectionFactory _connectionFactory;
        private ILogger<RabbitMQService> _logger;

        public RabbitMQService(IOptions<RabbitMQServiceOptions> options,
            ILogger<RabbitMQService> logger) {
            _options = options.Value;
            _connectionFactory = new ConnectionFactory() {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Password = _options.Password,
            };
            _logger = logger;
        }

        public async Task SendPartAsync(CrackHashWorkerResponse response, string contentType = "application/xml") {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _options.AnswersQueueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

            var content = RequestsSerializer.Serialize(response);
            var body = Encoding.UTF8.GetBytes(content);

            var props = new BasicProperties();
            props.ContentType = contentType;
            props.DeliveryMode = DeliveryModes.Persistent;

            await channel.BasicPublishAsync(String.Empty, _options.AnswersQueueName, true, props, body);
        }

    }
}
