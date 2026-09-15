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
    public class EducationController : ControllerBase
    {
        #region<private>
        private readonly AppDbContext _db;
        #endregion

        #region <ctor>
        public EducationController(AppDbContext db) => _db = db;
        #endregion

        #region <methods>
        // Id breaks ties: two qualifications ending on the same date would
        // otherwise have no defined order.
        [HttpGet]
        public Task<List<Education>> List() =>
            _db.Educations.OrderByDescending(e => e.End).ThenByDescending(e => e.Id).ToListAsync();

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var row = await _db.Educations.FindAsync(id);
            return row is null ? NotFound() : Ok(row);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Create([FromBody] Education e)
        {
            if (string.IsNullOrWhiteSpace(e.School) || string.IsNullOrWhiteSpace(e.Degree))
                return BadRequest(new { error = "School and Degree are required." });

            _db.Educations.Add(e);
            await _db.SaveChangesAsync();
            return Ok(e);
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Update(int id, [FromBody] Education e)
        {
            var row = await _db.Educations.FindAsync(id);
            if (row is null) return NotFound();

            if (string.IsNullOrWhiteSpace(e.School) || string.IsNullOrWhiteSpace(e.Degree))
                return BadRequest(new { error = "School and Degree are required." });

            row.School  = e.School;
            row.Degree  = e.Degree;
            row.Start   = e.Start;
            row.End     = e.End;
            row.Details = e.Details is null ? null : new List<string>(e.Details);

            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{id:int}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Delete(int id)
        {
            var row = await _db.Educations.FindAsync(id);
            if (row is null) return NotFound();

            _db.Educations.Remove(row);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        #endregion
    }
}
