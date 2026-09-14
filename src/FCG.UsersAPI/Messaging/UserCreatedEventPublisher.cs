using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using FCG.Contracts.Events;
using FCG.UsersAPI.Application.Abstractions.Messaging;
using Microsoft.Extensions.Configuration;

namespace FCG.UsersAPI.Messaging
{
    public class UserCreatedEventPublisher
        : IUserCreatedEventPublisher
    {
        private readonly IAmazonSQS _sqs;
        private readonly IConfiguration _configuration;

        public UserCreatedEventPublisher(
            IAmazonSQS sqs,
            IConfiguration configuration)
        {
            _sqs = sqs;
            _configuration = configuration;
        }

        public async Task Publish(
            string userId,
            string name,
            string email)
        {
            var userCreatedEvent = new UserCreatedEvent(
                userId,
                name,
                email);

            var message = new
            {
                eventType = "UserCreated",
                data = userCreatedEvent
            };

            var queueUrl =
                _configuration["AWS:SQS:NotificationsQueueUrl"];

            if (string.IsNullOrWhiteSpace(queueUrl))
            {
                throw new InvalidOperationException(
                    "AWS:SQS:NotificationsQueueUrl não configurada.");
            }

            await _sqs.SendMessageAsync(
                new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = JsonSerializer.Serialize(message)
                });
        }
    }
}