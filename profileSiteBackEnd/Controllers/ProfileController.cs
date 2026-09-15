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
    public class ProfileController : ControllerBase
    {
        #region <private>
        private readonly AppDbContext _db;
        #endregion

        #region <ctor>
        public ProfileController(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));
        #endregion

        #region <methods>
        [HttpGet]
        public Task<Profile?> Get() =>
      _db.Profiles.Include(p => p.Links).FirstOrDefaultAsync();

        [HttpPut]
        [ServiceFilter(typeof(ValidateAntiforgeryHeaderAttribute))]
        public async Task<IActionResult> Put([FromBody] Profile p)
        {
            var cur = await _db.Profiles.Include(x => x.Links).FirstOrDefaultAsync();
            if (cur is null)
            {
                _db.Profiles.Add(p);
            }
            else
            {
                cur.Name = p.Name; cur.Title = p.Title; cur.Tagline = p.Tagline; cur.Summary = p.Summary;
                cur.Location = p.Location; cur.Email = p.Email; cur.Github = p.Github; cur.Linkedin = p.Linkedin;
                cur.Skills = new(p.Skills);
                cur.Links.Clear();
                foreach (var l in p.Links) cur.Links.Add(new Link { Label = l.Label, Url = l.Url });
            }
            await _db.SaveChangesAsync();
            return Ok(p);
        }
        #endregion
    }
}
