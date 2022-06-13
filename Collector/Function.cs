using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Shared;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Collector;

public class Function
{
    public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        foreach (var record in dynamoEvent.Records)
        {
            if (record.EventName == "INSERT")
            {
                var order = record.Dynamodb.NewImage.ToObject<Order>();
                order.Status = StatusOder.Collected;
                try
                {
                    await ProcessOrderAmount(order);
                    // send to queue order
                    await AmazonUtil.SendToQueue(EnumQueueSQS.order, order);
                    context.Logger.LogLine($"Sucess Order: '{order.Id}'");

                }
                catch (Exception ex)
                {
                    context.Logger.LogLine($"Error: '{ex.Message}'");
                    order.CancellationJustification = ex.Message;
                    order.Canceled = true;
                    await AmazonUtil.SendToQueue(EnumQueueSNS.failure, order);
                    //add Dead letter queue
                }
                // save order
                await order.SaveAsync();
            }
        }
    }

    private async Task ProcessOrderAmount(Order order)
    {
        foreach (var product in order.Products)
        {
            var stockProduct = await GetProductFromDynamoDBAsync(product.Id);
            if (stockProduct == null) throw new InvalidOperationException($"Produto não encontrado na tabela estoque. {product.Id}");

            product.Value = stockProduct.Value;
            product.Name = stockProduct.Name;
        }

        var totalValue = order.Products.Sum(x => x.Value * x.Amount);
        if (order.TotalValue != 0 && order.TotalValue != totalValue)
            throw new InvalidOperationException($"O valor esperado do pedido é de R$ {order.TotalValue} e o valor verdadeiro é R$ {totalValue}");

        order.TotalValue = totalValue;
    }

    private async Task<Product> GetProductFromDynamoDBAsync(string id)
    {
        var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
        var request = new QueryRequest
        {
            TableName = "stock",
            KeyConditionExpression = "Id = :v_id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":v_id", new AttributeValue { S = id } } }
        };

        var response = await client.QueryAsync(request);
        var item = response.Items.FirstOrDefault();
        if (item == null) return null;
        return item.ToObject<Product>();
    }

}