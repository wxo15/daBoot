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
                ViewData["IsMember"] = _db.Relations.Find(own.Id, user.Id);
            } else
            {
                ViewData["IsMember"] = null;
            }
            return View(user);
        }

        [HttpGet("myteammembers")]
        public async Task<IActionResult> TeamMemberList()
        {
            IEnumerable<Account> objList = null;
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                IEnumerable<int> idlist = _db.Relations.Where(u => u.UserId == own.Id).Select(u => u.TeamMemberId);
                objList = _db.Users.Where(u => idlist.Contains(u.Id)).OrderBy(u => u.FirstName + " " + u.LastName);
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
                Relation obj = new () { UserId = user.Id, TeamMemberId = userid };
                if (!_db.Relations.Contains(obj))
                {
                    _db.Relations.Add(obj);
                    Relation revobj = new() { UserId = userid, TeamMemberId = user.Id };
                    if (!_db.Relations.Contains(revobj))
                    {
                        _db.Notifications.Add(new Notification("<a href=\"/profile/" + user.Username + "\"> " + user.FirstName + " " + user.LastName + "</a> added you as a team member. Add him/her back!",
                            userid,
                            null,
                            "addteammember/" + user.Id
                        ));
                    }
                }
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

        [HttpGet("search/")]
        public async Task<IActionResult> SearchResult(string searchstr)
        {
            var searchTerms = searchstr.Split();
            IEnumerable<SearchItemResult> objList = null;
            if (searchTerms.Length == 2)
            {
                objList = (from account in _db.Users
                        where account.FirstName.Contains(searchTerms[0]) && account.LastName.Contains(searchTerms[1])
                        select new SearchItemResult { Account = account, IsMember = false });
                if (User.Identity.IsAuthenticated)
                {
                    var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                    var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                    objList = (from account in _db.Users
                               join relation in _db.Relations on new { a = own.Id, b = account.Id } equals new { a = relation.UserId, b = relation.TeamMemberId }
                               into gj
                               from sub in gj.DefaultIfEmpty()
                               where account.FirstName.Contains(searchTerms[0]) && account.LastName.Contains(searchTerms[1]) && account != own
                               select new SearchItemResult { Account = account, IsMember = sub.TeamMember.Username != null });
                }
            }
            else
            {
                objList = (from account in _db.Users
                        where account.FirstName.Contains(searchTerms[0]) || account.LastName.Contains(searchTerms[0])
                        select new SearchItemResult { Account = account, IsMember = false });
                if (User.Identity.IsAuthenticated)
                {
                    var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                    var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                    objList = (from account in _db.Users
                               join relation in _db.Relations on new { a = own.Id, b = account.Id } equals new { a = relation.UserId, b = relation.TeamMemberId }
                               into gj
                               from sub in gj.DefaultIfEmpty()
                               where (account.FirstName.Contains(searchTerms[0]) || account.LastName.Contains(searchTerms[0]) || account.Username.Contains(searchTerms[0])) && account != own
                               select new SearchItemResult { Account = account, IsMember = sub.TeamMember.Username != null });
                }
            }
            return View(objList);
        }

        public class SearchItemResult
        {
            public Account Account { get; set; }
            public bool IsMember { get; set; }
        }

    }
}
