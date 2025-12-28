using Inventory.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MasterDataController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/MasterData/add-latex-rate
        [HttpPost("add-latex-rate")]
        public async Task<IActionResult> AddLatexRate([FromBody] LatexRate rateDto)
        {
            var today = rateDto.Rate_Date.Date;

            var existingRate = await _context.latex_rates
                .FirstOrDefaultAsync(x => x.Rate_Date == today);

            if (existingRate != null)
            {
                return BadRequest("Rate for this date already exists!");
            }

            _context.latex_rates.Add(rateDto);
            await _context.SaveChangesAsync();

            return Ok(rateDto);
        }

        // GET: api/MasterData/latex-rates-last10
        [HttpGet("latex-rates-last10")]
        public async Task<IActionResult> GetLast10Rates()
        {
            var rates = await _context.latex_rates
                .OrderByDescending(x => x.Rate_Date)
                .Take(10)
                .ToListAsync();

            return Ok(rates);
        }
    }
}
