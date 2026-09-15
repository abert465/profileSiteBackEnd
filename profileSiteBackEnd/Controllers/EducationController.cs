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
        [HttpGet]
        public Task<List<Education>> List() =>
            _db.Educations.OrderByDescending(e => e.End).ToListAsync();

        [HttpPost]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Create([FromBody] Education e)
        {
            _db.Educations.Add(e);
            await _db.SaveChangesAsync();
            return Ok(e);
        }

        [HttpPut("{i:int}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> UpdateByIndex(int i, [FromBody] Education e)
        {
            var ordered = await _db.Educations.OrderByDescending(x => x.End).ToListAsync();
            if (i < 0 || i >= ordered.Count) return NotFound();

            var row = await _db.Educations.FindAsync(ordered[i].Id);
            if (row is null) return NotFound();

            row.School  = e.School;
            row.Degree  = e.Degree;
            row.Start   = e.Start;
            row.End     = e.End;
            row.Details = e.Details is null ? null : new List<string>(e.Details);

            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{i:int}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> DeleteByIndex(int i)
        {
            var ordered = await _db.Educations.OrderByDescending(x => x.End).ToListAsync();
            if (i < 0 || i >= ordered.Count) return NotFound();

            _db.Educations.Remove(ordered[i]);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        #endregion
    }
}
