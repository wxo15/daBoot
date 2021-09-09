using daBoot.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using daBoot.Models;

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

        [HttpGet("myteammember")]
        public async Task<IActionResult> TeamMemberList()
        {
            IEnumerable<Account> objList = null;
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                IEnumerable<int> idlist = _db.Relations.Where(u => u.UserId == own.Id).Select(u => u.TeamMemberId);
                objList = _db.Users.Where(u => idlist.Contains(u.Id));
            }
            return View(objList);
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

        [HttpPost("profile/update")]
        public async Task<IActionResult> RemoveTeamMember(int id, string firstname, string lastname, string email, string profpic)
        {
            if (id == 0 || firstname == null || lastname == null || email == null || profpic == null)
            {
                return Redirect("/profile");
            }
            var existinguser = _db.Users.Find(id);
            if (User.Identity.IsAuthenticated && existinguser != null)
            {
                if (existinguser.Username == User.Claims.FirstOrDefault(c => c.Type == "username").Value)
                {
                    existinguser.FirstName = firstname;
                    existinguser.LastName = lastname;
                    existinguser.EmailAddress = email;
                    existinguser.ProfilePicURL = profpic;
                    _db.Update(existinguser);
                    _db.SaveChanges();
                    // Change existing claim and resign-in
                    var Identity = User.Identity as ClaimsIdentity;
                    Identity.RemoveClaim(Identity.FindFirst(ClaimTypes.Name));
                    Identity.RemoveClaim(Identity.FindFirst("profpic"));
                    Identity.AddClaim(new Claim(ClaimTypes.Name, firstname + " " + lastname));
                    Identity.AddClaim(new Claim("profpic", profpic));
                    var claimsPrincipal = new ClaimsPrincipal(Identity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                }
            }
            return Redirect("/profile");
        }
    }
}
