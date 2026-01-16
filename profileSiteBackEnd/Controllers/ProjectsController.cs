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
        private readonly IImageService _imageService;
        #endregion

        #region<ctor>
        public ProjectsController(AppDbContext db, IImageService imageService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }
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

        [HttpPost("{slug}/image")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> UploadImage(string slug, [FromForm] IFormFile image)
        {
            var project = await _db.Projects.FindAsync(slug);
            if (project is null) return NotFound();

            if (image == null || image.Length == 0)
                return BadRequest(new { error = "No image file provided" });

            if (!_imageService.ValidateImage(image))
                return BadRequest(new { error = "Invalid image file. Max 5MB, formats: jpg, png, webp, gif" });

            if (!string.IsNullOrEmpty(project.ImageUrl))
            {
                await _imageService.DeleteProjectImageAsync(project.ImageUrl);
            }

            project.ImageUrl = await _imageService.SaveProjectImageAsync(image, slug);
            await _db.SaveChangesAsync();

            return Ok(new { imageUrl = project.ImageUrl });
        }

        [HttpDelete("{slug}/image")]
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> DeleteImage(string slug)
        {
            var project = await _db.Projects.FindAsync(slug);
            if (project is null) return NotFound();

            if (!string.IsNullOrEmpty(project.ImageUrl))
            {
                await _imageService.DeleteProjectImageAsync(project.ImageUrl);
                project.ImageUrl = null;
                await _db.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete("{slug}")]
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Delete(string slug)
        {
            var row = await _db.Projects.FindAsync(slug);
            if (row is null) return NotFound();

            if (!string.IsNullOrEmpty(row.ImageUrl))
            {
                await _imageService.DeleteProjectImageAsync(row.ImageUrl);
            }

            _db.Projects.Remove(row);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        #endregion
    }
}
