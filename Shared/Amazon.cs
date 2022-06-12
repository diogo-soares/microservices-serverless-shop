using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

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
    }
}
