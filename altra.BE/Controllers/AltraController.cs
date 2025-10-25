using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace altra.BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AltraController : ControllerBase
    {
        private readonly ILogger<AltraController> _logger;

        private readonly OrderManager _orderManager;
        private readonly LoginManager _loginManager;

        public AltraController(ILogger<AltraController> logger,OrderManager orderManager, LoginManager loginManager)
        {
            _logger = logger;
            _orderManager = orderManager;
            _loginManager = loginManager;
        }

        [HttpGet("Refresh")]
        public IActionResult Refresh([FromQuery] string accessToken)
        {
            _logger.LogInformation($"Refresh");
            var msg =_loginManager.ProcessLoginWithAccessToken(accessToken).Result == true ? "LoggedIn" : "LoggedOut";
            return Ok(new { message = msg});
        }

        [HttpGet("Login")]
        public IActionResult Login([FromQuery] string requestToken)
        {
            _logger.LogInformation($"Refresh");
            return Ok(new { message = _loginManager.ProcessLoginWithRequestToken(requestToken).Result });
        }

        [HttpGet("GetLoginURL")]
        public IActionResult GetLoginURL()
        {
            _logger.LogInformation($"GetLoginURL");
            return Ok(new { message = _loginManager.GetLoginURL() });
        }

        [HttpGet("PlaceOrder")]
        public IActionResult PlaceOrder([FromQuery] string symbol, [FromQuery] int quantity, [FromQuery] decimal buy, decimal sell)
        {
            _logger.LogInformation($"Order received: {symbol} {quantity} @ {buy} & {sell}");
            
            return Ok(new { message = _orderManager.PlaceOpenOrder(symbol, quantity, buy, sell), time = DateTime.Now });
        }

        [HttpGet("CancelOrder")]
        public IActionResult CancelOrder()
        {
            _logger.LogInformation($"Cancel all orders");
            _orderManager.CancelAllGTTOrders();
            return Ok(new { message = "All Orders Cancelled", time = DateTime.Now });
        }


    }
}
