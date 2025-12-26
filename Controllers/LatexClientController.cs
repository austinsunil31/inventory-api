using Inventory.API.Models.DTOs;
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
                    c.Created_at,
                    c.IsHandledByClient
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

        [HttpPost("add")]
        public IActionResult AddLatexStockEntry([FromBody] LatexStockInDto request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    Message = "Invalid request",
                    StatusCode = 400
                });
            }


            // Calculations
            decimal waterWeightPerCan = 1.1m;
            decimal latexWeight = request.Total_weight - (waterWeightPerCan * request.Can_count);

            latexWeight = latexWeight < 0 ? 0 : latexWeight;

            decimal totalDrc = 0;
            decimal dryRubber = 0;
            decimal ratePerKg = 0;
            decimal dryRubberValue = 0;
            decimal deduction = 0;
            decimal finalDeduction = 0;
            decimal finalValue = 0;
            if(request.Sample_Drc != null)
            {
                totalDrc = (decimal)(request.Sample_Drc * 5);
                dryRubber = (totalDrc * latexWeight) / 100;
                ratePerKg = 167;
                dryRubberValue = dryRubber * ratePerKg;
                if (request.isHandledByClient)
                {
                    deduction = 10;
                }
                else if (!request.isHandledByClient)
                {
                    deduction = 15;
                }
                else
                {
                    deduction = 10;
                }
                finalDeduction = dryRubber * deduction;
                finalValue = dryRubberValue - finalDeduction;
            }

            var stockIn = new LatexStockIn
            {
                Client_no = request.Client_no,
                Total_weight = request.Total_weight,
                Latex_weight = latexWeight,
                Can_count = request.Can_count,
                Sample_drc = 0,
                Total_drc = totalDrc,
                Dry_rubber = dryRubber,
                Dry_rubber_value = dryRubberValue,
                Final_value = finalValue,
                Created_on = DateTime.UtcNow,
                processing_fees = (int)finalDeduction
            };

            _context.latex_stock_in.Add(stockIn);
            _context.SaveChanges();

            return Ok(new
            {
                Data = stockIn,
                Message = "Latex stock entry added successfully",
                StatusCode = 200
            });
        }

        [HttpGet("gettodayentries")]
        public IActionResult GetTodayEntries()
        {
            // STEP 1: Convert today's IST range to UTC (for DB filtering)
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var todayIst = TimeZoneInfo.ConvertTime(DateTime.UtcNow, istZone).Date;
            var todayUtcStart = TimeZoneInfo.ConvertTimeToUtc(todayIst, istZone);
            var tomorrowUtcStart = todayUtcStart.AddDays(1);

            // STEP 2: Fetch from DB using UTC range
            var rawEntries = _context.latex_stock_in
                .Where(x => x.Created_on >= todayUtcStart && x.Created_on < tomorrowUtcStart)
                .ToList();

            if (!rawEntries.Any())
            {
                return NotFound(new
                {
                    Message = "No entries found for today",
                    StatusCode = 404
                });
            }

            // STEP 3: Convert UTC → IST for UI display
            var entries = rawEntries
                .Select(x => new
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
                })
                .OrderByDescending(x => x.Created_on)
                .ToList();

            return Ok(new
            {
                Data = entries,
                Message = "Today's entries fetched successfully",
                StatusCode = 200
            });
        }


        [HttpPut("update-sample-drc/{id}")]
        public IActionResult UpdateSampleDrc(int id, [FromBody] DTOs.UpdateSampleDrcDto request)
        {
            if (request == null || request.Sample_Drc == null)
            {
                return BadRequest(new
                {
                    Message = "Sample DRC value required",
                    StatusCode = 400
                });
            }

            var stockIn = _context.latex_stock_in.FirstOrDefault(s => s.Id == id);
            Console.WriteLine(stockIn.Client_no);

            var clientDetails = _context.latex_clients.FirstOrDefault(s => s.Client_no == stockIn.Client_no);
            Console.WriteLine(clientDetails.IsHandledByClient);

            if (stockIn == null)
            {
                return NotFound(new
                {
                    Message = "Stock entry not found",
                    StatusCode = 404
                });
            }

            stockIn.Sample_drc = request.Sample_Drc;

            decimal totalDrc = (decimal)(request.Sample_Drc * 5);
            decimal dryRubber = (totalDrc * stockIn.Latex_weight) / 100;
            decimal ratePerKg = 167;
            decimal dryRubberValue = dryRubber * ratePerKg;

            decimal deduction;

            if ((bool)clientDetails.IsHandledByClient)
            {
                deduction = 10;
            }
            else
            {
                deduction = 15;
            }

            decimal finalDeduction = dryRubber * deduction;
            decimal finalValue = dryRubberValue - finalDeduction;

            // Save updated values
            stockIn.Total_drc = totalDrc;
            stockIn.Dry_rubber = dryRubber;
            stockIn.Dry_rubber_value = dryRubberValue;
            stockIn.Final_value = finalValue;
            stockIn.processing_fees = (int)finalDeduction;

            _context.SaveChanges();

            return Ok(new
            {
                Data = stockIn,
                Message = "Sample DRC updated successfully",
                StatusCode = 200
            });
        }

        [HttpPut("update-stock")]
        public IActionResult UpdateStock([FromBody] DTOs.UpdateStockDto request)
        {
            if (request == null || request.Id <= 0)
            {
                return BadRequest(new
                {
                    Message = "Invalid request data",
                    StatusCode = 400
                });
            }

            var stockIn = _context.latex_stock_in.FirstOrDefault(s => s.Id == request.Id);

            if (stockIn == null)
            {
                return NotFound(new
                {
                    Message = "Stock entry not found",
                    StatusCode = 404
                });
            }

            // Update only the editable values
            stockIn.Total_weight = request.Total_weight;
            stockIn.Can_count = request.Can_count;


            _context.SaveChanges();

            return Ok(new
            {
                Data = stockIn,
                Message = "Stock entry updated successfully",
                StatusCode = 200
            });
        }

        [HttpGet("getentriesbydate")]
        public IActionResult GetEntriesByDate([FromQuery] string date)
        {
            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var selectedIstDate = DateTime.Parse(date).Date;

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(selectedIstDate, istZone);
            var endUtc = startUtc.AddDays(1);

            var rawEntries = _context.latex_stock_in
                .Where(x => x.Created_on >= startUtc && x.Created_on < endUtc)
                .ToList();

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
            }).OrderByDescending(x => x.Created_on)
              .ToList();

            return Ok(new
            {
                Data = entries,
                Message = "Entries fetched successfully",
                StatusCode = 200
            });
        }


    }
}
