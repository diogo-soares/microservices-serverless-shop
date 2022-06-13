using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;
using Shared;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Booker;

public class Function
{
    private AmazonDynamoDBClient AmazonDynamoDBClient { get; }
    public Function()
    {
        AmazonDynamoDBClient = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
    }

    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        if (evnt.Records.Count > 1) throw new InvalidOperationException("only one message can be handled at a time");

        var message = evnt.Records.FirstOrDefault();

        if (message == null) return;

        await ProcessMessageAsync(message, context);
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        var order = JsonConvert.DeserializeObject<Order>(message.Body);
        order.Status = StatusOder.reserved;

        foreach (var product in order.Products)
        {
            try
            {
                await WithdrawFromStock(product.Id, product.Amount);
                product.Reserved = true;
                context.Logger.LogInformation($"Withdraw From Stock {product.Id} - {product.Name}");

                context.Logger.LogInformation($"Processed message {message.Body}");
            }
            catch (ConditionalCheckFailedException)
            {
                order.CancellationJustification = $"product out of stock {product.Id} - {product.Name}";
                order.Canceled = true;
                context.Logger.LogInformation($"Error {order.CancellationJustification}");
                break;
            }
        }

        if (order.Canceled)
        {
            foreach (var product in order.Products)
            {
                if (product.Reserved)
                {
                    await ReturnToStock(product.Id, product.Amount);
                    product.Reserved = false;
                    order.CancellationJustification = $"product returned to stock {product.Id} - {product.Name}";
                }

                await AmazonUtil.SendToQueue(EnumQueueSNS.failure, order);
                await order.SaveAsync();
            }
        }
        else
        {
            await AmazonUtil.SendToQueue(EnumQueueSQS.reserved, order);
            await order.SaveAsync();
        }
    }

    private async Task ReturnToStock(string id, int amount)
    {
        var request = new UpdateItemRequest
        {
             TableName = "stock",
             ReturnValues = "NONE",
             Key = new Dictionary<string, AttributeValue>
             {
                 { "Id", new AttributeValue{ S = id} }
             },
             UpdateExpression = "SET Amount = (Amount - :orderQuantity)",
             ConditionExpression = "Amount >= :orderQuantity",
             ExpressionAttributeValues = new Dictionary<string, AttributeValue>
             {
                 { ":orderQuantity", new AttributeValue{ N = amount.ToString() } }
             }
        };

        await AmazonDynamoDBClient.UpdateItemAsync(request);
    }

    private async Task WithdrawFromStock(string id, int amount)
    {
        var request = new UpdateItemRequest
        {
            TableName = "stock",
            ReturnValues = "NONE",
            Key = new Dictionary<string, AttributeValue>
             {
                 { "id", new AttributeValue{ S = id} }
             },
            UpdateExpression = "SET Amount = (Amount + :orderQuantity)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
             {
                 { ":orderQuantity", new AttributeValue{ N = amount.ToString() } }
             }
        };

        await AmazonDynamoDBClient.UpdateItemAsync(request);
    }
}