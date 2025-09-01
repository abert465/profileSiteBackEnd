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

        [HttpGet]
        public Task<List<Certification>> List() =>
            _db.Certifications
               .OrderByDescending(c => c.Issued ?? DateTime.MinValue)
               .ToListAsync();

        [HttpPost]
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Create([FromBody] Certification c)
        {
            _db.Certifications.Add(c);
            await _db.SaveChangesAsync();
            return Ok(c);
        }

        [HttpPut("{i:int}")]
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> UpdateByIndex(int i, [FromBody] Certification c)
        {
            var ordered = await _db.Certifications
                                   .OrderByDescending(x => x.Issued ?? DateTime.MinValue)
                                   .ToListAsync();
            if (i < 0 || i >= ordered.Count) return NotFound();

            var row = await _db.Certifications.FindAsync(ordered[i].Id);
            if (row is null) return NotFound();

            row.Name    = c.Name;
            row.Issuer  = c.Issuer;
            row.Issued  = c.Issued;
            row.Expires = c.Expires;

            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{i:int}")]
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> DeleteByIndex(int i)
        {
            var ordered = await _db.Certifications
                                   .OrderByDescending(x => x.Issued ?? DateTime.MinValue)
                                   .ToListAsync();
            if (i < 0 || i >= ordered.Count) return NotFound();

            _db.Certifications.Remove(ordered[i]);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
