using ChatServer.Data;
using ChatServer.Hubs;
using ChatServer.Models.Entities;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace ChatServer.Services
{
    public class ConsumerService : IHostedService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ConsumerConfig _consumerConfig;
        private readonly ILogger<ConsumerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AdminClientBuilder _adminClientBuilder;

        public ConsumerService(
            IConfiguration configuration,
            ConsumerConfig consumerConfig,
            ILogger<ConsumerService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _consumerConfig = consumerConfig;
            _logger = logger;
            _scopeFactory = scopeFactory;
            var config = new AdminClientConfig
            {
                BootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers"),
                SocketTimeoutMs = 6000,
            };
            _adminClientBuilder = new AdminClientBuilder(config);

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ConsumerService is starting.");

            Task.Run(() => Consume(cancellationToken));

            return Task.CompletedTask;
        }

        private void Consume(CancellationToken cancellationToken)
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig)
                .SetPartitionsAssignedHandler((c, partitions) =>
				{
					_logger.LogInformation($"Assigned partitions: [{string.Join(", ", partitions)}]");
					return partitions.Select(tp => new TopicPartitionOffset(tp, Offset.Beginning)).ToList();
				})
				.SetPartitionsRevokedHandler((c, partitions) =>
				{
					_logger.LogInformation($"Revoking assignment: [{string.Join(", ", partitions)}]");
				})
				.SetErrorHandler((_, e) => _logger.LogError($"Error: {e.Reason}"))
				.SetValueDeserializer(Deserializers.Utf8)
                .Build();
            var topic = _adminClientBuilder.Build().GetMetadata(TimeSpan.FromSeconds(10)).Topics.Select(x => x.Topic).ToList();
            consumer.Subscribe(topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    if (consumeResult != null && !consumeResult.IsPartitionEOF)
                    {
                        //time delay để đảm bảo tin nhắn được gửi đến client trước khi update trạng thái tin nhắn
						Thread.Sleep(5000);
                        if (consumeResult?.Message != null)
                        {
                            // Tiến hành deserialize và xử lý tin nhắn
							var message = JsonConvert.DeserializeObject<Message>(consumeResult.Message.Value);

							//update lại trạng thái tin nhắn
							using var scope = _scopeFactory.CreateScope();
							var context = scope.ServiceProvider.GetRequiredService<ChatAPPContext>();
							var msg = context.Messages.Where(x => x.Id == message.Id).FirstOrDefault();
							if (msg != null && msg.SenderStatus != SenderStatusEnum.Delivery.ToString())
							{
								msg.SenderStatus = SenderStatusEnum.Delivery.ToString();
								context.SaveChanges();
							}
							//gửi tin nhắn đến client
							var hub = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();
							hub.Clients.All.SendAsync("ReceiveMessage", message);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                catch (Exception)
                {
                    _logger.LogError("Error consuming message");
					continue;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ConsumerService is stopping.");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogInformation("ConsumerService is disposing.");
        }
    }
}
