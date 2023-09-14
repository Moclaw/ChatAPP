using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using ChatServer.Hubs;
using ChatServer.Models.DTO;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace ChatServer.Services
{

    public class AwsSNSServices : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AwsSNSServices> _logger;
        private readonly ChatHub _chatHub;
        public AwsSNSServices(IConfiguration configuration, ILogger<AwsSNSServices> logger, ChatHub chatHub)
        {
            _configuration = configuration;
            _logger = logger;
            _chatHub = chatHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AwsSQN start");
            var credentials = new BasicAWSCredentials(_configuration["AWS:AccessKey"], _configuration["AWS:SecretKey"]);
            var client = new AmazonSQSClient(credentials, Amazon.RegionEndpoint.USEast1);
            var request = new ReceiveMessageRequest
            {
                QueueUrl = _configuration["AWS:SQSUrl"],
                WaitTimeSeconds = 20,
                MaxNumberOfMessages = 10
            };
            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await client.ReceiveMessageAsync(request);
                if (response.Messages.Any())
                {
                    foreach (var message in response.Messages)
                    {
                        try
                        {
                            var messageBody = JsonConvert.DeserializeObject<NotificationDTO>(message.Body);
                            string result = JsonConvert.SerializeObject(messageBody);

                            //_logger.LogInformation($"SNS: {result}");
                            await _chatHub.SendNoitification(messageBody!);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.Message);
                        }
                    }
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AwsSQN stop");
            await base.StopAsync(stoppingToken);
        }

        public async Task SendNotification(NotificationDTO message)
        {
            var credentials = new BasicAWSCredentials(_configuration["AWS:AccessKey"], _configuration["AWS:SecretKey"]);
            var client = new AmazonSQSClient(credentials, Amazon.RegionEndpoint.USEast1);
            string notification = JsonConvert.SerializeObject(message);
            var clientSNS = new AmazonSimpleNotificationServiceClient();

            await clientSNS.CreateTopicAsync(message.Topic);
            var request = new SendMessageRequest
            {
                QueueUrl = _configuration["AWS:SQSUrl"],
                MessageBody = notification
            };

            await _chatHub.SendNoitification(message);
            var response = client.SendMessageAsync(request).Result;
        }

        public async Task<SubscribeResponse> SubcribeToSNSTopic()
        {
            var credentials = new BasicAWSCredentials(_configuration["AWS:AccessKey"], _configuration["AWS:SecretKey"]);
            var client = new AmazonSimpleNotificationServiceClient(credentials, Amazon.RegionEndpoint.USEast1);
            var request = new SubscribeRequest
            {
                Protocol = "sqs",
                TopicArn = _configuration["AWS:SNSTopicArn"],
                Endpoint = _configuration["AWS:SQSUrl"]
            };
            var response = await client.SubscribeAsync(request);

            return response;
        }
        public async Task<UnsubscribeResponse> UnsubcribeToSNSTopic()
        {
            var credentials = new BasicAWSCredentials(_configuration["AWS:AccessKey"], _configuration["AWS:SecretKey"]);
            var client = new AmazonSimpleNotificationServiceClient(credentials, Amazon.RegionEndpoint.USEast1);
            var request = new UnsubscribeRequest
            {
                SubscriptionArn = _configuration["AWS:SNSSubscriptionArn"]
            };
            var response = await client.UnsubscribeAsync(request);

            return response;
        }

        public async Task<CreateTopicResponse> CreateSNSTopic(string topic)
        {
            var credentials = new BasicAWSCredentials(_configuration["AWS:AccessKey"], _configuration["AWS:SecretKey"]);
            var client = new AmazonSimpleNotificationServiceClient(credentials, Amazon.RegionEndpoint.USEast1);
            var request = new CreateTopicRequest
            {
                Name = topic
            };
            var response = await client.CreateTopicAsync(request);

            return response;
        }

        public async Task<DeleteTopicResponse> Delete(string topic)
        {
            var credentials = new BasicAWSCredentials(_configuration["AWS:AccessKey"], _configuration["AWS:SecretKey"]);
            var client = new AmazonSimpleNotificationServiceClient(credentials, Amazon.RegionEndpoint.USEast1);

            var respone = await client.DeleteTopicAsync(topic);
            return respone;
        }
    }
}
