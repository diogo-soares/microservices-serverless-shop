using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace Shared
{
    public static class Amazon
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
    }
}
