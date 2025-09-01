using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using profileSiteBackEnd.Models;
using profileSiteBackEnd.Services;
using System.Security.AccessControl;

namespace profileSiteBackEnd.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize]
    public class SkillsController : ControllerBase
    {
        #region <private>
        private readonly AppDbContext _db;
        private async Task<int?> GetProfileIdAsync() =>
            await _db.Profiles.Select(p => (int)p.Id).FirstOrDefaultAsync();
        #endregion

        #region <ctor>
        public SkillsController(AppDbContext db) => _db = db;
        #endregion

        #region <methods>
        [HttpGet]
        public async Task<ActionResult<List<Skill>>> List()
        {
            var pid = await GetProfileIdAsync();
            if (pid is null) return NotFound("No profile found");
            var rows = await _db.Skills
                .Where(s => s.ProfileId == pid)
                .OrderBy(s => s.Order ?? int.MaxValue).ThenBy(s => s.Name)
                .ToListAsync();
            return Ok(rows);
        }

        public record UpsertSkillDto(string Name, bool? IsVisible, int? Order);

        [HttpPost]
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Create([FromBody] UpsertSkillDto dto)
        {
            var pid = await GetProfileIdAsync();
            if (pid is null) return BadRequest(new { error = "Create a profile first." });

            var name = (dto.Name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { error = "Name is required" });

            var exists = await _db.Skills.AnyAsync(s => s.ProfileId == pid && s.Name == name);
            if (exists) return Conflict(new { error = "Skill already exists" });

            var row = new Skill { ProfileId = pid.Value, Name = name, IsVisible = dto.IsVisible ?? true, Order = dto.Order };
            _db.Skills.Add(row);
            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpPut("{id:int}")]
        //[ServiceFilter(typeof(profileSiteBackEnd.Services.ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Update(int id, [FromBody] UpsertSkillDto dto)
        {
            var row = await _db.Skills.FindAsync(id);
            if (row is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var name = dto.Name.Trim();
                if (!string.Equals(name, row.Name, StringComparison.Ordinal))
                {
                    var dup = await _db.Skills.AnyAsync(s => s.ProfileId == row.ProfileId && s.Name == name && s.Id != id);
                    if (dup) return Conflict(new { error = "Another skill already has that name" });
                    row.Name = name;
                }
            }

            if (dto.IsVisible is not null) row.IsVisible = dto.IsVisible.Value;
            row.Order = dto.Order;

            await _db.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{id:int}")]
        //[ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Delete(int id)
        {
            var row = await _db.Skills.FindAsync(id);
            if (row is null) return NotFound();
            _db.Skills.Remove(row);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        #endregion
    }
}
