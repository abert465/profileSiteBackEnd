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
        // Id is the tiebreaker: entries sharing a start date would otherwise have
        // no defined order, and callers page through this list.
        [HttpGet]
        public Task<List<Experience>> List() =>
            _db.Experiences.OrderByDescending(e => e.Start).ThenByDescending(e => e.Id).ToListAsync();

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var row = await _db.Experiences.FindAsync(id);
            return row is null ? NotFound() : Ok(row);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Create([FromBody] Experience e)
        {
            if (string.IsNullOrWhiteSpace(e.Company) || string.IsNullOrWhiteSpace(e.Role))
                return BadRequest(new { error = "Company and Role are required." });

            _db.Experiences.Add(e);
            await _db.SaveChangesAsync();
            return Ok(e);
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Update(int id, [FromBody] Experience e)
        {
            var row = await _db.Experiences.FindAsync(id);
            if (row is null) return NotFound();

            if (string.IsNullOrWhiteSpace(e.Company) || string.IsNullOrWhiteSpace(e.Role))
                return BadRequest(new { error = "Company and Role are required." });

            row.Company = e.Company; row.Role = e.Role; row.Location = e.Location;
            row.Start = e.Start; row.End = e.End;
            // The client may omit these or send null outright.
            row.Highlights = new(e.Highlights ?? []); row.Tech = new(e.Tech ?? []);
            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{id:int}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Delete(int id)
        {
            var row = await _db.Experiences.FindAsync(id);
            if (row is null) return NotFound();

            _db.Experiences.Remove(row);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        #endregion
    }
}
