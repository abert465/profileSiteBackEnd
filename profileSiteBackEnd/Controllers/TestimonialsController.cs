using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using profileSiteBackEnd.Models;

namespace profileSiteBackEnd.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize]
    public class TestimonialsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TestimonialsController(AppDbContext db) => _db = db;

        [HttpGet]
        public Task<List<Testimonial>> List() =>
            _db.Testimonials
               .OrderBy(t => t.Order ?? int.MaxValue)
               .ThenByDescending(t => t.Date ?? DateTime.MinValue)
               .ToListAsync();

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Testimonial t)
        {
            _db.Testimonials.Add(t);
            await _db.SaveChangesAsync();
            return Ok(t);
        }

        [HttpPut("{i:int}")]
        public async Task<IActionResult> UpdateByIndex(int i, [FromBody] Testimonial t)
        {
            var ordered = await _db.Testimonials
                                   .OrderBy(x => x.Order ?? int.MaxValue)
                                   .ThenByDescending(x => x.Date ?? DateTime.MinValue)
                                   .ToListAsync();
            if (i < 0 || i >= ordered.Count) return NotFound();

            var row = await _db.Testimonials.FindAsync(ordered[i].Id);
            if (row is null) return NotFound();

            row.Name = t.Name;
            row.Title = t.Title;
            row.Company = t.Company;
            row.Content = t.Content;
            row.Date = t.Date;
            row.Rating = t.Rating;
            row.IsVisible = t.IsVisible;
            row.Order = t.Order;

            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{i:int}")]
        public async Task<IActionResult> DeleteByIndex(int i)
        {
            var ordered = await _db.Testimonials
                                   .OrderBy(x => x.Order ?? int.MaxValue)
                                   .ThenByDescending(x => x.Date ?? DateTime.MinValue)
                                   .ToListAsync();
            if (i < 0 || i >= ordered.Count) return NotFound();

            _db.Testimonials.Remove(ordered[i]);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
