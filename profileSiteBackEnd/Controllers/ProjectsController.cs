using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using profileSiteBackEnd.Models;
using profileSiteBackEnd.Services;

namespace profileSiteBackEnd.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        #region<private>
        private readonly AppDbContext _db;
        #endregion

        #region<ctor>
        public ProjectsController(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));
        #endregion

        #region <actions>
        [HttpGet]
        public Task<List<Project>> List() =>
       _db.Projects.OrderBy(p => p.Title).ToListAsync();

        [HttpPost]
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Create([FromBody] Project p)
        {
            p.Slug = string.IsNullOrWhiteSpace(p.Slug) ? Guid.NewGuid().ToString("n") : p.Slug;
            if (await _db.Projects.AnyAsync(x => x.Slug == p.Slug)) return Conflict(new { error = "slug exists" });
            _db.Projects.Add(p);
            await _db.SaveChangesAsync();
            return Ok(p);
        }

        [HttpPut("{slug}")]
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Update(string slug, [FromBody] Project p)
        {
            var row = await _db.Projects.FindAsync(slug);
            if (row is null) return NotFound();
            row.Title = p.Title; row.Description = p.Description;
            row.Tech = new(p.Tech); row.RepoUrl = p.RepoUrl; row.LiveUrl = p.LiveUrl;
            row.Highlights = new(p.Highlights);
            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{slug}")]
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Delete(string slug)
        {
            var row = await _db.Projects.FindAsync(slug);
            if (row is null) return NotFound();
            _db.Projects.Remove(row);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        #endregion
    }
}
