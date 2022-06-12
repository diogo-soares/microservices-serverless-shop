using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Registration.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        [HttpPost]
        public async Task PostAsync([FromBody] Order order)
        {
            order.Id = Guid.NewGuid().ToString();
            order.CreationDate = DateTime.Now;

            await order.SaveAsync();

            Console.WriteLine($"Order saved successfully: id {order.Id}");
        }
    }
}
