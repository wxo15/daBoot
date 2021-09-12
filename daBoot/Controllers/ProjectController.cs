using daBoot.Data;
using daBoot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace daBoot.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProjectController(ApplicationDbContext db)
        {
            _db = db;
        }

        public class ProjectRole
        {
            public Project Project { get; set; }
            public string Role { get; set; }
        }

        [HttpGet("myprojects")]
        public async Task<IActionResult> ProjectList()
        {
            IEnumerable<ProjectRole> objList = null;
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                objList = (from upr in _db.UserProjects
                           join account in _db.Users on upr.UserId equals account.Id
                           join project in _db.Projects on upr.ProjectId equals project.Id
                           join role in _db.Roles on upr.RoleId equals role.Id
                           where account == own 
                           select new ProjectRole { Project = project, Role = role.RoleName });
            }
            return View(objList);
        }

        [HttpGet("project/{projectid}")]
        public async Task<IActionResult> Index([FromRoute] int projectid)
        {
            Project project = null;
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                if ((from upr in _db.UserProjects
                     join account in _db.Users on upr.UserId equals account.Id
                     join proj in _db.Projects on upr.ProjectId equals proj.Id
                     where account == own && proj.Id == projectid
                     select proj).Any())
                {
                    project = await _db.Projects.Include("TeamMembers").FirstOrDefaultAsync(p => p.Id == projectid);
                    foreach (var member in project.TeamMembers)
                    {
                        member.User = await _db.Users.FirstOrDefaultAsync(u => u.Id == member.UserId);
                        member.Role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == member.RoleId);
                    }
                    ViewData["UserRole"] = (from upr in _db.UserProjects
                                           join account in _db.Users on upr.UserId equals account.Id
                                           join proj in _db.Projects on upr.ProjectId equals proj.Id
                                           join role in _db.Roles on upr.RoleId equals role.Id
                                           where account == own && proj.Id == projectid
                                           select role.RoleName).FirstOrDefault().ToString();
                }
            }
            return View(project);
        }

        [HttpPost("project/{projectid}/promote/{userid}")]
        public async Task<string> Promote(int projectid, int userid)
        {
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                var target = _db.Users.Find(userid);
                // make sure both own and target users have
                if ((from upr in _db.UserProjects
                     join account in _db.Users on upr.UserId equals account.Id
                     join proj in _db.Projects on upr.ProjectId equals proj.Id
                     where (account == own || account == target) && proj.Id == projectid
                     select proj).Count() == 2)
                {
                    var ownrole = _db.UserProjects.Find(own.Id, projectid);
                    var targetrole = _db.UserProjects.Find(target.Id, projectid);
                    if (ownrole.RoleId == 1 && targetrole.RoleId == 2)
                    {
                        ownrole.RoleId += 1;
                        targetrole.RoleId -= 1;
                    }
                    else if (ownrole.RoleId < targetrole.RoleId)
                    {
                        targetrole.RoleId -= 1;
                    }
                    _db.SaveChanges();
                    return "Success";
                }
            }
            return "Failed";
        }

        [HttpPost("project/{projectid}/demote/{userid}")]
        public async Task<string> Demote(int projectid, int userid)
        {
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                var target = _db.Users.Find(userid);
                // make sure both own and target users have
                if ((from upr in _db.UserProjects
                     join account in _db.Users on upr.UserId equals account.Id
                     join proj in _db.Projects on upr.ProjectId equals proj.Id
                     where (account == own || account == target) && proj.Id == projectid
                     select proj).Count() == 2)
                {
                    var ownrole = _db.UserProjects.Find(own.Id, projectid);
                    var targetrole = _db.UserProjects.Find(target.Id, projectid);
                    if (ownrole.RoleId < targetrole.RoleId && targetrole.RoleId == 3)
                    {
                        _db.UserProjects.Remove(targetrole);
                    }
                    else if (ownrole.RoleId < targetrole.RoleId)
                    {
                        targetrole.RoleId += 1;
                    }
                    _db.SaveChanges();
                    return "Success";
                }
            }
            return "Failed";
        }
    }
}
