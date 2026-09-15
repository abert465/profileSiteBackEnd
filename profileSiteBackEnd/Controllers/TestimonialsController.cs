using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using profileSiteBackEnd.Models;
using profileSiteBackEnd.Services;

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
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Create([FromBody] Testimonial t)
        {
            _db.Testimonials.Add(t);
            await _db.SaveChangesAsync();
            return Ok(t);
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Update(int id, [FromBody] Testimonial t)
        {
            var row = await _db.Testimonials.FindAsync(id);
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

        [HttpDelete("{id:int}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Delete(int id)
        {
            var row = await _db.Testimonials.FindAsync(id);
            if (row is null) return NotFound();

            _db.Testimonials.Remove(row);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
