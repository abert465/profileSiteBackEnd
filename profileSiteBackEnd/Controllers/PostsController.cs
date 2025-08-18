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
    public class PostsController : ControllerBase
    {
        #region<private>
        private readonly AppDbContext _db;
        #endregion

        #region <ctor>
        public PostsController(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));
        #endregion

        #region <methods>
        [HttpGet]
        public Task<List<Post>> List() =>
            _db.Posts.OrderByDescending(p => p.Published).ToListAsync();

        [HttpPost]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Create([FromBody] Post p)
        {
            p.Slug = string.IsNullOrWhiteSpace(p.Slug) ? Guid.NewGuid().ToString("n") : p.Slug;
            if (p.Published == default) p.Published = DateTime.UtcNow;
            if (await _db.Posts.AnyAsync(x => x.Slug == p.Slug)) return Conflict(new { error = "slug exists" });
            _db.Posts.Add(p);
            await _db.SaveChangesAsync();
            return Ok(p);
        }

        [HttpPut("{slug}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Update(string slug, [FromBody] Post p)
        {
            var row = await _db.Posts.FindAsync(slug);
            if (row is null) return NotFound();
            row.Title = p.Title; row.Excerpt = p.Excerpt;
            if (p.Published != default) row.Published = p.Published;
            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{slug}")]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Delete(string slug)
        {
            var row = await _db.Posts.FindAsync(slug);
            if (row is null) return NotFound();
            _db.Posts.Remove(row);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        #endregion
    }
}
