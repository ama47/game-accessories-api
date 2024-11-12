using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.Services;
using static src.DTO.OrderDTO;
using src.Utils;

namespace scr.Controller
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrdersController : ControllerBase
    {
        protected IOrderService _orderService;
        public readonly static string[] orderStatuses = { "ordered", "shipped", "on delivery", "delivered" };

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        // Gets all available orders.
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<OrderReadDTO>>> GetAllOrders([FromQuery] PaginationOptions paginationOptions)
        {
            var ordersList = await _orderService.GetAllAsync(paginationOptions);
            return Ok(ordersList.OrderByDescending(o => o.OrderDate));
        }

        // Gets a specific order by it's ID
        [Authorize]
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderReadDTO>> GetOrderById([FromRoute] Guid orderId)
        {
            var foundOrder = await _orderService.GetByIdAsync(orderId);
            if (foundOrder == null)
                return NotFound("order not found");
            return Ok(foundOrder);
        }
        // Gets a user's orders by its ID in ascending.
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<OrderReadDTO>>> GetOrdersByUserID([FromRoute] Guid userId,
            [FromQuery] PaginationOptions paginationOptions)
        {
            var userOrders = await _orderService.GetByUserIdAsync(userId, paginationOptions);
            return Ok(userOrders.OrderBy(o => o.OrderDate));
        }

        // Gets a user's old orders by its ID in descending.
        [Authorize]
        [HttpGet("user/{userId}/ordershistory")]
        public async Task<ActionResult<List<OrderReadDTO>>> GetOrdersHistoryByUserID([FromRoute] Guid userId,
            [FromQuery] PaginationOptions paginationOptions)
        {
            var userOrders = await _orderService.GetHistoryByUserIdAsync(userId, paginationOptions);
            return Ok(userOrders.OrderByDescending(o => o.OrderDate));
        }


        // Post new order to the order database
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderReadDTO>> CreateOrder([FromBody] OrderCreateDTO newOrder)
        {
            var createdOrder = await _orderService.CreateOneAsync(newOrder);
            return createdOrder != null ?
                Created($"api/v1/orders/{createdOrder.Id}", createdOrder) :
                BadRequest("One of products is out of stock");
        }

        // Update current order status into ("shipped", "on delivery", "delivered") or the ship date into new one.
        [Authorize(Roles = "Admin")]
        [HttpPut("{orderId}")]
        public async Task<ActionResult> UpdateOrder(Guid orderId, OrderUpdateDTO updatedOrder)
        {
            bool foundOrderStatus = false;
            foreach (string status in orderStatuses)
            {
                if (updatedOrder.OrderStatus.Equals(status, StringComparison.OrdinalIgnoreCase))
                {
                    foundOrderStatus = true;
                    break;
                }
            }

            if (!foundOrderStatus)
                return NotFound("Invalid order status");

            if (updatedOrder.ShipDate < DateTime.Now)
                return BadRequest("Invalid ship date");

            bool isUpdated = await _orderService.UpdateOneAsync(orderId, updatedOrder);

            return isUpdated ? NoContent() : NotFound("Order ID not found");

        }
        // Cancel the current order by deleting it from orders database
        [Authorize]
        [HttpDelete("{orderId}")]
        public async Task<ActionResult> CancelOrder(Guid orderId)
        {
            var isDeleted = await _orderService.DeleteOneAsync(orderId);
            return isDeleted ? NoContent() : NotFound("Order ID not found");
        }
    }
}