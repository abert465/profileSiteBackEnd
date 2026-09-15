using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using profileSiteBackEnd.Models;
using profileSiteBackEnd.Services;

namespace profileSiteBackEnd.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize]
    public class CertificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CertificationsController(AppDbContext db) => _db = db;

        // Id breaks ties: undated certifications all collapse to the same sort
        // key, so without it their relative order is undefined.
        [HttpGet]
        public Task<List<Certification>> List() =>
            _db.Certifications
               .OrderByDescending(c => c.Issued ?? DateTime.MinValue)
               .ThenByDescending(c => c.Id)
               .ToListAsync();

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var row = await _db.Certifications.FindAsync(id);
            return row is null ? NotFound() : Ok(row);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Create([FromBody] Certification c)
        {
            if (string.IsNullOrWhiteSpace(c.Name))
                return BadRequest(new { error = "Name is required." });

            _db.Certifications.Add(c);
            await _db.SaveChangesAsync();
            return Ok(c);
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Update(int id, [FromBody] Certification c)
        {
            var row = await _db.Certifications.FindAsync(id);
            if (row is null) return NotFound();

            if (string.IsNullOrWhiteSpace(c.Name))
                return BadRequest(new { error = "Name is required." });

            row.Name    = c.Name;
            row.Issuer  = c.Issuer;
            row.Issued  = c.Issued;
            row.Expires = c.Expires;

            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{id:int}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Delete(int id)
        {
            var row = await _db.Certifications.FindAsync(id);
            if (row is null) return NotFound();

            _db.Certifications.Remove(row);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
