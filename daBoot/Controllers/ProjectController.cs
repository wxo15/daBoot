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
                     select project).Any())
                {
                    project = await _db.Projects.Include("TeamMembers").FirstOrDefaultAsync(p => p.Id == projectid);
                    foreach (var member in project.TeamMembers)
                    {
                        member.User = await _db.Users.FirstOrDefaultAsync(u => u.Id == member.UserId);
                        member.Role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == member.RoleId);
                    }
                }
            }
            return View(project);
        }
    }
}
