using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace Shared
{
    public static class AmazonUtil
    {
        public static async Task SaveAsync(this Order order)
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
            var context = new DynamoDBContext(client);

            await context.SaveAsync(order);
        }

        public static T ToObject<T>(this Dictionary<string, AttributeValue> dictionary)
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
            var context = new DynamoDBContext(client);

            var doc = Document.FromAttributeMap(dictionary);

            return context.FromDocument<T>(doc);
        }

        public static async Task SendToQueue(EnumQueueSQS queue, Order order)
        {
            var json = JsonConvert.SerializeObject(order);

            var client = new AmazonSQSClient(RegionEndpoint.SAEast1);

            var request = new SendMessageRequest
            {
                QueueUrl = $"https://sqs.sa-east-1.amazonaws.com/981378892361/{queue}",
                MessageBody = json
            };

            await client.SendMessageAsync(request);
        }

        public static async Task SendToQueue(EnumQueueSNS queue, Order order)
        {
            // await 
            await Task.CompletedTask;
        }

    }
}
