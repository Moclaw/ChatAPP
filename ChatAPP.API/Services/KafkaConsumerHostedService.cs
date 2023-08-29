using Confluent.Kafka;

namespace ChatAPP.API.Services
{
    public class KafkaConsumerHostedService : IHostedService
    {
        private readonly ILogger<KafkaConsumerHostedService> _logger;
        private readonly IConfiguration _configuration;
        public KafkaConsumerHostedService(ILogger<KafkaConsumerHostedService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var conf = new ConsumerConfig
            {
                GroupId = "demo",
                BootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers"),
                AutoOffsetReset = AutoOffsetReset.Latest,
                SaslPassword = _configuration.GetValue<string>("Kafka:SaslPassword"),
                SaslUsername = _configuration.GetValue<string>("Kafka:SaslUsername"),
            };
        
            using (var builder = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                try
                {
                    builder.Subscribe("demo-topic");
                    var cancelToken = new CancellationTokenSource();
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var consumer = builder.Consume(cancelToken.Token);
                            Console.WriteLine($"Message Recived: {consumer.Message.Value} ");
                        }
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occured: {e.Error.Reason}");
                        builder.Close();
                    }
                    return Task.CompletedTask;
                }
                catch (OperationCanceledException)
                {
                    builder.Close();
                    return Task.CompletedTask;
                }
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
