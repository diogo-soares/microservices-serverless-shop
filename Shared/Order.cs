using Amazon.DynamoDBv2.DataModel;

namespace Shared
{
    [DynamoDBTable("orders")]
    public class Order
    {
        public string Id { get; set; }

        public decimal TotalValue { get; set; }

        public DateTime CreationDate { get; set; }

        public List<Product> Products { get; set; }

        public Client Client { get; set; }

        public Payment Payment { get; set; }

        public string Justificativa { get; set; }

        public string Justification { get; set; }

        public bool PaidOut { get; set; }

        public bool Billed { get; set; }
    }
}
