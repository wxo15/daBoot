using daBoot.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace daBoot.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProfileController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("profile/{username?}")]
        public async Task<IActionResult> Index([FromRoute] string username = "")
        {
            
            if (username == "")
            {
                if (User.Identity.IsAuthenticated)
                {
                    username = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                }
            }
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                ViewData["IsFriend"] = _db.Relations.Find(own.Id, user.Id);
            } else
            {
                ViewData["IsFriend"] = null;
            }
            return View(user);
        }

        [HttpPost("addteammember/{userid}")]
        public async Task<string> AddTeamMember(int userid)
        {
            if (User.Identity.IsAuthenticated && _db.Users.Find(userid) != null)
            {
                var username = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
                _db.Relations.Add(new Models.Relation {UserId = user.Id, TeamMemberId = userid });
                _db.SaveChanges();
                return "Success";
            }
            return "Failed";
        }

        [HttpPost("rmvteammember/{userid}")]
        public async Task<string> RemoveTeamMember(int userid)
        {
            if (User.Identity.IsAuthenticated && _db.Users.Find(userid) != null)
            {
                var username = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
                _db.Relations.Remove(new Models.Relation { UserId = user.Id, TeamMemberId = userid });
                _db.SaveChanges();
                return "Success";
            }
            return "Failed";
        }

    }
}
