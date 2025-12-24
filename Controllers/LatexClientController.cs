using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/latexclient")]
    public class LatexClientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LatexClientController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("getallclients")]
        public IActionResult GetAllClients()
        {
            var clients = _context.latex_clients
                .Select(c => new
                {
                    c.Id,
                    c.Client_no,
                    c.Name,
                    c.Mobile_num,
                    c.Plot_location,
                    c.Created_at
                })
                .ToList();

            if (!clients.Any())
                return NotFound(new
                {
                    Message = "No clients found",
                    StatusCode = 404
                });

            return Ok(new
            {
                Data = clients,
                Message = "Clients fetched successfully",
                StatusCode = 200
            });
        }
    }
}
