using ChatServer.Data;
using ChatServer.Hubs;
using ChatServer.Models.DTO;
using ChatServer.Models.Entities;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Channels;
using static Confluent.Kafka.ConfigPropertyNames;

namespace ChatServer.Services
{
    public class ConsumerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly ILogger<ConsumerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ChatHub _chatHub;
        public ConsumerService(
            IConfiguration configuration,
            IConsumer<Ignore, string> consumer,
            ILogger<ConsumerService> logger,
            IServiceScopeFactory scopeFactory,
            ChatHub chatHub)
        {
            _configuration = configuration;
            _consumer = consumer;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _chatHub = chatHub;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scopce = _scopeFactory.CreateScope();
            var db = scopce.ServiceProvider.GetRequiredService<ChatAPPContext>();
            var channels = await db.Channels
                .AsNoTracking()
                .ToListAsync();
            if (channels.Count == 0)
            {
                return;
            }

            _consumer.Subscribe(channels.Select(c => c.Topic));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(10000);
                    if (consumeResult == null) 
                        break;
                    var mess = JsonConvert.DeserializeObject<MessageDTO>(consumeResult.Message.Value);
                    await _chatHub.SendMessage(mess!, mess!.Topic);
                    Console.WriteLine($"Consumed message: {consumeResult.Message.Value}");
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error while consuming: {e.Error.Reason}");
                }
            }
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }

        public async Task<List<Message>> GetMessages(string topic, int partition, long offset)
        {
            try
            {
                // Subscribe to the topic if not already subscribed
                _consumer.Subscribe(topic);

                var messages = new List<Message>();
                var timeout = TimeSpan.FromSeconds(10);

                // Keep track of consumed messages in batches
                var batchMessages = new List<string>();

                while (true)
                {
                    try
                    {
                        var topicPartition = new TopicPartition(topic, partition);
                        _consumer.Position(topicPartition);
                        var consumeResult = _consumer.Consume(timeout);

                        if (consumeResult == null)
                            break;

                        batchMessages.Add(consumeResult.Message.Value);

                        // Process messages in batches to improve performance
                        if (batchMessages.Count >= 10) // Adjust batch size as needed
                        {
                            messages.AddRange(await DeserializeMessagesAsync(batchMessages));
                            batchMessages.Clear();
                        }
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error while consuming: {e.Error.Reason}");
                    }
                }

                // Deserialize any remaining messages in the last batch
                messages.AddRange(await DeserializeMessagesAsync(batchMessages));

                return messages;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<Message>();
            }
        }

        private async Task<IEnumerable<Message>> DeserializeMessagesAsync(List<string> messages)
        {
            var deserializedMessages = new List<Message>();
            var tasks = new List<Task<Message>>();

            foreach (var messageJson in messages)
            {
                if (messageJson == null)
                    continue;
                tasks.Add(Task.Run(() =>
                {
                    return JsonConvert.DeserializeObject<Message>(messageJson)!;
                }));
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                var message = task.Result;
                if (message != null)
                {
                    deserializedMessages.Add(message);
                }
            }

            return deserializedMessages;
        }
    }
}
