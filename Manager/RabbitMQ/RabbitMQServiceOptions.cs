namespace Manager.RabbitMQ {
    public class RabbitMQServiceOptions {
        public string HostName { get; set; } = null!;

        public string TasksQueueName { get; set; } = null!;

        public string AnswersQueueName { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;

    }
}
