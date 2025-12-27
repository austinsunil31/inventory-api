using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("latexstock")]
        public async Task<IActionResult> GetLatexStockReport(
            [FromQuery] string fromDate,
            [FromQuery] string toDate,
            [FromQuery] string? clientNo = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "Created_on",
            [FromQuery] string? sortDir = "desc"
        )
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new
                {
                    Message = "Both FromDate and ToDate are required",
                    StatusCode = 400
                });
            }

            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            var fromIstDate = DateTime.Parse(fromDate).Date;
            var toIstDate = DateTime.Parse(toDate).Date;

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(fromIstDate, istZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(toIstDate.AddDays(1), istZone);

            var query = _context.latex_stock_in
                .Where(x => x.Created_on >= startUtc && x.Created_on < endUtc);

            if (!string.IsNullOrEmpty(clientNo))
            {
                query = query.Where(x => x.Client_no == clientNo);
            }

            // Sorting logic
            query = (sortBy.ToLower(), sortDir.ToLower()) switch
            {
                ("client_no", "asc") => query.OrderBy(x => x.Client_no),
                ("client_no", _) => query.OrderByDescending(x => x.Client_no),

                ("final_value", "asc") => query.OrderBy(x => x.Final_value),
                ("final_value", _) => query.OrderByDescending(x => x.Final_value),

                ("created_on", "asc") => query.OrderBy(x => x.Created_on),
                _ => query.OrderByDescending(x => x.Created_on),
            };

            var totalCount = await query.CountAsync();

            var rawEntries = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var entries = rawEntries.Select(x => new
            {
                x.Id,
                x.Client_no,
                x.Total_weight,
                x.Latex_weight,
                x.Can_count,
                x.Sample_drc,
                x.Total_drc,
                x.Dry_rubber,
                x.Dry_rubber_value,
                x.Final_value,
                Created_on = TimeZoneInfo.ConvertTimeFromUtc(x.Created_on, istZone),
                x.Is_drc_added,
                x.processing_fees
            }).ToList();
            var totals = await query
    .GroupBy(x => 1)
    .Select(g => new
    {
        Total_Total_weight = g.Sum(x => x.Total_weight),
        Total_Latex_weight = g.Sum(x => x.Latex_weight),
        Total_Can_count = g.Sum(x => x.Can_count),
        Total_Sample_drc = g.Sum(x => x.Sample_drc),
        Total_Total_drc = g.Sum(x => x.Total_drc),
        Total_Dry_rubber = g.Sum(x => x.Dry_rubber),
        Total_Dry_rubber_value = g.Sum(x => x.Dry_rubber_value),
        Total_Final_value = g.Sum(x => x.Final_value),
        Total_processing_fees = g.Sum(x => x.processing_fees)
    })
    .FirstOrDefaultAsync();
            return Ok(new
            {
                Data = entries,
                Totals = totals,
                Total = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = Math.Ceiling((double)totalCount / pageSize),
                Message = "Report fetched successfully",
                StatusCode = 200
            });
        }
    }
}
