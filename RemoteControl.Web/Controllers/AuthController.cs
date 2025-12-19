using Microsoft.AspNetCore.Mvc;

namespace RemoteControl.Web.Controllers
{
    [ApiController]
    // Giữ route cũ: /Auth/verify
    [Route("[controller]")]
    // Thêm route mới đúng checklist: /api/auth/verify
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly string _expectedPasskey;

        public AuthController(IConfiguration config)
        {
            _expectedPasskey = config["Passkey:Value"] ?? "";
        }

        public record VerifyRequest(string Passkey);
        public record VerifyResponse(bool Success);

        [HttpPost("verify")]
        public ActionResult<VerifyResponse> Verify([FromBody] VerifyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Passkey))
                return Ok(new VerifyResponse(false));

            var isValid = string.Equals(request.Passkey, _expectedPasskey, StringComparison.Ordinal);
            return Ok(new VerifyResponse(isValid));
        }
    }
}
