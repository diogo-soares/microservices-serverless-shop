using Amazon.DynamoDBv2.DataModel;

namespace Shared
{
    public enum StatusOder
    {
        Collected,
        PaidOut,
        Billed
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
        public StatusOder Status { get; set; }
        public bool Canceled { get; set; }
    }
}
