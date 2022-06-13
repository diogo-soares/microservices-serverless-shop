using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Shared
{
    public enum StatusOder
    {
        collected,
        paidOut,
        billed,
        reserved
    }


    [DynamoDBTable("orders")]
    public class Order
    {
        public string Id { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime CreationDate { get; set; }
        public List<Product> Products { get; set; }
        public Client Client { get; set; }
        public Payment Payment { get; set; }
        public string CancellationJustification { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StatusOder Status { get; set; }

        public bool Canceled { get; set; }
    }
}
