using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Payments;
using WebApp.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers;

/// <summary>
/// Controller xử lý thanh toán qua các cổng thanh toán điện tử
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Khởi tạo thanh toán qua ví điện tử
    /// </summary>
    /// <param name="request">Thông tin yêu cầu thanh toán</param>
    /// <returns>Kết quả khởi tạo thanh toán và URL thanh toán</returns>
    /// <response code="200">Khởi tạo thanh toán thành công</response>
    /// <response code="400">Thông tin thanh toán không hợp lệ</response>
    /// <response code="500">Lỗi hệ thống</response>
    [HttpPost("initiate")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentResultDto), 200)]
    [ProducesResponseType(typeof(PaymentResultDto), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PaymentResultDto>> InitiatePayment([FromBody] PaymentRequestDto request)
    {
        try
        {
            _logger.LogInformation("Initiating payment for order {OrderId} with method {PaymentMethod}", 
                request.OrderId, request.PaymentMethod);

            var result = await _paymentService.InitiatePayment(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for order {OrderId}", request.OrderId);
            return StatusCode(500, new PaymentResultDto
            {
                Success = false,
                ErrorMessage = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Lấy trạng thái thanh toán của đơn hàng
    /// </summary>
    /// <param name="orderId">ID của đơn hàng</param>
    /// <returns>Thông tin trạng thái thanh toán</returns>
    /// <response code="200">Lấy trạng thái thanh toán thành công</response>
    /// <response code="404">Không tìm thấy giao dịch thanh toán</response>
    /// <response code="500">Lỗi hệ thống</response>
    [HttpGet("status/{orderId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentTransactionDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PaymentTransactionDto>> GetPaymentStatus(Guid orderId)
    {
        try
        {
            var paymentTransaction = await _paymentService.GetPaymentTransaction(orderId);
            
            if (paymentTransaction == null)
            {
                return NotFound(new { message = "Payment transaction not found" });
            }

            var dto = PaymentTransactionMapping.ToDto(paymentTransaction);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for order {OrderId}", orderId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Lấy thông tin giao dịch thanh toán theo Transaction ID
    /// </summary>
    /// <param name="transactionId">ID của giao dịch thanh toán</param>
    /// <returns>Thông tin chi tiết giao dịch thanh toán</returns>
    /// <response code="200">Lấy thông tin giao dịch thành công</response>
    /// <response code="404">Không tìm thấy giao dịch thanh toán</response>
    /// <response code="500">Lỗi hệ thống</response>
    [HttpGet("transaction/{transactionId}")]
    [ProducesResponseType(typeof(PaymentTransactionDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PaymentTransactionDto>> GetPaymentByTransactionId(string transactionId)
    {
        try
        {
            var paymentTransaction = await _paymentService.GetPaymentTransactionByTransactionId(transactionId);
            
            if (paymentTransaction == null)
            {
                return NotFound(new { message = "Payment transaction not found" });
            }

            var dto = PaymentTransactionMapping.ToDto(paymentTransaction);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment by transaction ID {TransactionId}", transactionId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("momo-callback")]
    public async Task<IActionResult> MoMoCallback([FromForm] IFormCollection form)
    {
        try
        {
            _logger.LogInformation("Received MoMo callback: {@Callback}", form.ToDictionary(x => x.Key, x => x.Value.ToString()));

            var callback = new PaymentCallbackDto
            {
                TransactionId = form["requestId"].ToString(),
                OrderId = form["orderId"].ToString(),
                Amount = decimal.TryParse(form["amount"], out var amount) ? amount : 0,
                Status = form["resultCode"].ToString(),
                Signature = form["signature"].ToString(),
                Parameters = form.ToDictionary(x => x.Key, x => x.Value.ToString())
            };

            var paymentTransaction = await _paymentService.ProcessPaymentCallback(callback);

            // Return success response to MoMo
            return Ok(new { message = "Callback processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo callback");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("momo-return")]
    public async Task<IActionResult> MoMoReturn()
    {
        try
        {
            _logger.LogInformation("MoMo return with query parameters: {@QueryParams}", Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));

            // Process MoMo return parameters
            var callback = new PaymentCallbackDto
            {
                TransactionId = Request.Query["requestId"].ToString(),
                OrderId = Request.Query["orderId"].ToString(),
                Amount = decimal.TryParse(Request.Query["amount"], out var amount) ? amount : 0,
                Status = Request.Query["resultCode"].ToString(),
                Signature = Request.Query["signature"].ToString(),
                Parameters = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString())
            };

            // Process payment callback to update transaction status
            var paymentTransaction = await _paymentService.ProcessPaymentCallback(callback);

            // Redirect to frontend with payment result
            var frontendUrl = GetFrontendReturnUrl();
            var redirectUrl = $"{frontendUrl}/payment-result?transactionId={callback.TransactionId}&status={callback.Status}&orderId={callback.OrderId}";
            
            _logger.LogInformation("Redirecting to frontend: {RedirectUrl}", redirectUrl);
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo return");
            var frontendUrl = GetFrontendReturnUrl();
            return Redirect($"{frontendUrl}/payment-result?status=error");
        }
    }

    [HttpPost("vnpay-callback")]
    public async Task<IActionResult> VnPayCallback([FromForm] IFormCollection form)
    {
        try
        {
            _logger.LogInformation("Received VnPay callback: {@Callback}", form.ToDictionary(x => x.Key, x => x.Value.ToString()));

            var callback = new PaymentCallbackDto
            {
                TransactionId = form["vnp_TxnRef"].ToString(),
                OrderId = ExtractOrderIdFromTxnRef(form["vnp_TxnRef"].ToString()),
                Amount = decimal.TryParse(form["vnp_Amount"], out var amount) ? amount / 100 : 0, // VnPay amount * 100
                Status = form["vnp_ResponseCode"].ToString(),
                Signature = form["vnp_SecureHash"].ToString(),
                Parameters = form.ToDictionary(x => x.Key, x => x.Value.ToString())
            };

            var paymentTransaction = await _paymentService.ProcessPaymentCallback(callback);

            // Return success response to VnPay
            return Ok(new { RspCode = "00", Message = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VnPay callback");
            return Ok(new { RspCode = "99", Message = "error" });
        }
    }

    [HttpGet("vnpay-return")]
    public async Task<IActionResult> VnPayReturn()
    {
        try
        {
            _logger.LogInformation("VnPay return with query parameters: {@QueryParams}", Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));

            // Process VnPay return parameters
            var callback = new PaymentCallbackDto
            {
                TransactionId = Request.Query["vnp_TxnRef"].ToString(),
                OrderId = "", // Will be resolved in ProcessPaymentCallback
                Amount = decimal.TryParse(Request.Query["vnp_Amount"], out var amount) ? amount / 100 : 0,
                Status = Request.Query["vnp_ResponseCode"].ToString(),
                Signature = Request.Query["vnp_SecureHash"].ToString(),
                Parameters = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString())
            };

            // Process payment callback to update transaction status
            var paymentTransaction = await _paymentService.ProcessPaymentCallback(callback);

            // Redirect to frontend with payment result
            var frontendUrl = GetFrontendReturnUrl();
            var orderId = paymentTransaction?.OrderId.ToString() ?? "";
            var redirectUrl = $"{frontendUrl}/payment-result?transactionId={callback.TransactionId}&status={callback.Status}&orderId={orderId}";
            
            _logger.LogInformation("Redirecting to frontend: {RedirectUrl}", redirectUrl);
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VnPay return");
            var frontendUrl = GetFrontendReturnUrl();
            return Redirect($"{frontendUrl}/payment-result?status=error");
        }
    }

    [HttpPost("expire/{paymentTransactionId:guid}")]
    [Authorize]
    public async Task<IActionResult> ExpirePaymentTransaction(Guid paymentTransactionId)
    {
        try
        {
            var result = await _paymentService.ExpirePaymentTransaction(paymentTransactionId);
            
            if (result)
            {
                return Ok(new { message = "Payment transaction expired successfully" });
            }

            return BadRequest(new { message = "Failed to expire payment transaction" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring payment transaction {PaymentTransactionId}", paymentTransactionId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    private string GetFrontendReturnUrl()
          {
          // Có thể config trong appsettings.json
          return "http://localhost:3000"; // React frontend URL
      }

    private static string ExtractOrderIdFromTxnRef(string txnRef)
    {
        // VnPay sử dụng timestamp làm TxnRef, cần lookup từ PaymentTransaction
        // For now, return empty - sẽ được resolve trong ProcessPaymentCallback
        return "";
    }
} 