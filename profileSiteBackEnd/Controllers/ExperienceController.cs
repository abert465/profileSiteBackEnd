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
    public class ExperienceController : ControllerBase
    {
        #region <private>
        private readonly AppDbContext _db;
        #endregion

        #region <ctor>
        public ExperienceController(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));
        #endregion

        #region <methods>
        [HttpGet]
        public Task<List<Experience>> List() =>
            _db.Experiences.OrderByDescending(e => e.Start).ToListAsync();

        [HttpPost]
       // [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Create([FromBody] Experience e)
        {
            _db.Experiences.Add(e);
            await _db.SaveChangesAsync();
            return Ok(e);
        }

        [HttpPut("{i:int}")]
       // [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> UpdateByIndex(int i, [FromBody] Experience e)
        {
            var ordered = await _db.Experiences.OrderByDescending(x => x.Start).ToListAsync();
            if (i < 0 || i >= ordered.Count) return NotFound();
            var row = await _db.Experiences.FindAsync(ordered[i].Id);
            if (row is null) return NotFound();

            row.Company = e.Company; row.Role = e.Role; row.Location = e.Location;
            row.Start = e.Start; row.End = e.End;
            row.Highlights = new(e.Highlights); row.Tech = new(e.Tech);
            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{i:int}")]
       // [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> DeleteByIndex(int i)
        {
            var ordered = await _db.Experiences.OrderByDescending(x => x.Start).ToListAsync();
            if (i < 0 || i >= ordered.Count) return NotFound();
            _db.Experiences.Remove(ordered[i]);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        #endregion
    }
}
