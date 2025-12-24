using Inventory.API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto request)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Username == request.Username &&
                u.PasswordHash == request.Password &&
                u.IsActive
            );

            if (user == null)
                return Unauthorized("Invalid username or password");

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                role = user.Role,
                isActive = user.IsActive,
                Message = "Login Sucessfull",
                StatusCode = 200
            });
        }
    }
}
