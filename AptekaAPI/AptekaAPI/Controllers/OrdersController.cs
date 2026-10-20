using AptekaAPI.Data;
using AptekaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AptekaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AptekaContext _context;
        public OrdersController(AptekaContext context) => _context = context;

        public record OrderItemDto(int MedicineId, int Amount);
        public record CreateOrderRequest(List<OrderItemDto> Items);

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return BadRequest(new { error = "Заказ не может быть пустым" });

            int nextId = await _context.KursOrders.AnyAsync()
                ? await _context.KursOrders.MaxAsync(o => o.IdOrder) + 1
                : 1;
            string orderNumber = $"ORD-{nextId:D4}";

            var status = await _context.KursStatuses.FirstOrDefaultAsync(s => s.Name == "В обработке");
            if (status == null)
                return StatusCode(500, new { error = "Статус 'В обработке' не найден в БД" });

            var newOrder = new KursOrder
            {
                IdClient = 1,
                OrderDate = DateTime.Now,
                OrderNumber = orderNumber,
                IdStatus = status.IdStatus
            };

            decimal totalSum = 0;

            foreach (var item in request.Items)
            {
                var med = await _context.KursMedicines.FindAsync(item.MedicineId);
                if (med == null)
                    return BadRequest(new { error = $"Лекарство ID={item.MedicineId} не найдено" });

                if (med.Amount < item.Amount)
                    return BadRequest(new { error = $"Недостаточно товара '{med.Name}' на складе" });

                totalSum += med.Price * item.Amount;
                med.Amount -= item.Amount; 

                newOrder.OrderItems.Add(new KursOrderSostav
                {
                    IdMedicine = item.MedicineId,
                    Amount = item.Amount
                });
            }

            newOrder.Sum = totalSum;
            _context.KursOrders.Add(newOrder);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                orderNumber = newOrder.OrderNumber,
                totalSum = newOrder.Sum,
                message = "Заказ успешно создан"
            });
        }

        [HttpGet("{orderNumber}")]
        public async Task<IActionResult> GetStatus(string orderNumber)
        {
            var order = await _context.KursOrders
                .Include(o => o.Status)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null) return NotFound(new { error = "Заказ не найден" });

            return Ok(new
            {
                order.OrderNumber,
                order.Sum,
                statusName = order.Status.Name,
                order.OrderDate,
                items = order.OrderItems.Select(i => new
                {
                    i.Medicine.Name,
                    i.Amount,
                    i.Medicine.Price
                })
            });
        }
    }
}