using Confluent.Kafka;

namespace ChatAPP.API.Services
{
    public class ProducerHostedService : IHostedService
    {
        private readonly ILogger<ProducerHostedService> _logger;
        private readonly IProducer<Null, string> _producer;
        private readonly IConfiguration _configuration;

        public ProducerHostedService(ILogger<ProducerHostedService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            var config = new ProducerConfig
            {
                BootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers"),
                SaslUsername = configuration.GetValue<string>("Kafka:SaslUsername"),
                SaslPassword = configuration.GetValue<string>("Kafka:SaslPassword"),
            };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var cancelToken = new CancellationTokenSource();
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = new Message<Null, string>
                    {
                        Value = "Hello World!"
                    };
                    var result = _producer.ProduceAsync("demo-topic", message, cancelToken.Token).GetAwaiter().GetResult();
                    _logger.LogInformation($"Message {result.Value} sent to partition {result.Partition} with offset {result.Offset}");
                }
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}");
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _producer?.Dispose();
            return Task.CompletedTask;
        }
    }
}
