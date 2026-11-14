using KivaBelion.API.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KivaBelion.API.Controllers;

/// <summary>Public endpoint the website calls to place a Cash on Delivery order. No online payment exists.</summary>
[ApiController]
[Route("api/v1/orders")]
public sealed class OrdersController(OrderService orders) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("checkout")]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var result = await orders.PlaceAsync(request, idempotencyKey, cancellationToken);

        switch (result.Outcome)
        {
            case PlaceOrderOutcome.Created:
                return StatusCode(StatusCodes.Status201Created, result.Order);

            case PlaceOrderOutcome.AlreadyExists:
                return Ok(result.Order);

            case PlaceOrderOutcome.Invalid:
                return BadRequest(new ValidationProblemDetails(result.Errors!)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Please check the highlighted fields.",
                });

            case PlaceOrderOutcome.ProductUnavailable:
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = result.Message,
                    Extensions = { ["code"] = "product_unavailable" },
                });

            case PlaceOrderOutcome.PriceChanged:
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = result.Message,
                    Extensions = { ["code"] = "price_changed", ["serverTotal"] = result.ServerTotal },
                });

            case PlaceOrderOutcome.OrdersClosed:
            default:
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
                {
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Title = result.Message ?? "Online ordering is not open yet.",
                    Extensions = { ["code"] = "orders_closed" },
                });
        }
    }
}
