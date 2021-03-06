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
                var own = await _db.Users.Include("TeamMembers").FirstOrDefaultAsync(u => u.Username == ownusername);
                if ((from upr in _db.UserProjects
                     join account in _db.Users on upr.UserId equals account.Id
                     join proj in _db.Projects on upr.ProjectId equals proj.Id
                     where account == own && proj.Id == projectid
                     select proj).Any())
                {
                    project = await _db.Projects
                        .Include("TeamMembers")
                        .Include("Tickets")
                        .FirstOrDefaultAsync(p => p.Id == projectid);
                    foreach (var member in project.TeamMembers)
                    {
                        member.User = await _db.Users.FirstOrDefaultAsync(u => u.Id == member.UserId);
                        member.Role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == member.RoleId);
                    }
                    foreach (var ticket in project.Tickets)
                    {
                        ticket.Priority = await _db.Priority.FirstOrDefaultAsync(p => p.Id == ticket.PriorityId);
                        ticket.Status = await _db.Status.FirstOrDefaultAsync(s => s.Id == ticket.StatusId);
                    }
                    ViewData["UserRole"] = (from upr in _db.UserProjects
                                           join account in _db.Users on upr.UserId equals account.Id
                                           join proj in _db.Projects on upr.ProjectId equals proj.Id
                                           join role in _db.Roles on upr.RoleId equals role.Id
                                           where account == own && proj.Id == projectid
                                           select role.RoleName).FirstOrDefault().ToString();
                    IEnumerable<int> idlist = _db.Relations.Where(u => u.UserId == own.Id).Select(u => u.TeamMemberId);
                    ViewData["YourTeamMembers"] = _db.Users.Where(u => idlist.Contains(u.Id) && !project.TeamMembers.Select(u => u.UserId).Contains(u.Id)).OrderBy(u => u.FirstName + " " + u.LastName);
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
                // make sure both own and target users have a role in this project
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
                        _db.Notifications.Add(new Notification("<a href=\"/profile/" + own.Username + "\"> " + own.FirstName + " " + own.LastName + "</a> promoted you to " + _db.Roles.Find(targetrole.RoleId).RoleName + " for project: <a href=\"/project/" + projectid + "\"> " + _db.Projects.Find(projectid).Name + "</a>.",
                            userid,
                            null,
                            null
                        ));
                    }
                    else if (ownrole.RoleId < targetrole.RoleId)
                    {
                        targetrole.RoleId -= 1;
                        _db.Notifications.Add(new Notification("<a href=\"/profile/" + own.Username + "\"> " + own.FirstName + " " + own.LastName + "</a> promoted you to " + _db.Roles.Find(targetrole.RoleId).RoleName + " for project: <a href=\"/project/" + projectid + "\"> " + _db.Projects.Find(projectid).Name + "</a>.",
                            userid,
                            null,
                            null
                        ));
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
                // make sure both own and target users have a role in this project
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
                        _db.Notifications.Add(new Notification("<a href=\"/profile/" + own.Username + "\"> " + own.FirstName + " " + own.LastName + "</a> removed you from project: <a href=\"/project/" + projectid + "\"> " + _db.Projects.Find(projectid).Name + "</a>.",
                            userid,
                            null,
                            null
                        ));
                    }
                    else if (ownrole.RoleId < targetrole.RoleId)
                    {
                        targetrole.RoleId += 1;
                        _db.Notifications.Add(new Notification("<a href=\"/profile/" + own.Username + "\"> " + own.FirstName + " " + own.LastName + "</a> demoted you to " + _db.Roles.Find(targetrole.RoleId).RoleName + " for project: <a href=\"/project/" + projectid + "\"> " + _db.Projects.Find(projectid).Name + "</a>.",
                            userid,
                            null,
                            null
                        ));
                    }
                    _db.SaveChanges();
                    return "Success";
                }
            }
            return "Failed";
        }

        [HttpPost("project/{projectid}/invite/{userid}")]
        public async Task<string> Invite(int projectid, int userid)
        {
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                var target = _db.Users.Find(userid);
                // make sure both own and target users have a role in this project
                if (target != null && (from upr in _db.UserProjects
                     join account in _db.Users on upr.UserId equals account.Id
                     join proj in _db.Projects on upr.ProjectId equals proj.Id
                     where account == own && proj.Id == projectid
                     select proj).Count() == 1 && (from upr in _db.UserProjects
                                                   join account in _db.Users on upr.UserId equals account.Id
                                                   join proj in _db.Projects on upr.ProjectId equals proj.Id
                                                   where account == target && proj.Id == projectid
                                                   select proj).Count() == 0)
                {
                    var newUserProject = new UserProject(userid, projectid, 3); 
                    _db.UserProjects.Add(newUserProject);
                    _db.Notifications.Add(new Notification("<a href=\"/profile/" + own.Username + "\"> " + own.FirstName + " " + own.LastName + "</a> invited you to project: <a href=\"/project/" + projectid + "\"> " + _db.Projects.Find(projectid).Name + "</a>.",
                        userid,
                        null,
                        null
                    ));
                    _db.SaveChanges();
                    return "Success";
                }
            }
            return "Failed";
        }

        [HttpPost("createproject")]
        public async Task CreateProject(string projectname)
        {
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);

                var newproject = new Project(projectname);
                _db.Projects.Add(newproject);
                _db.SaveChanges();

                int projectId = newproject.Id;
                var newuserproject = new UserProject(own.Id, projectId, 1);
                _db.UserProjects.Add(newuserproject);
                _db.SaveChanges();
                Redirect("~/myprojects");
            }
        }

    }
}
