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
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TicketController(ApplicationDbContext db)
        {
            _db = db;
        }


        [HttpGet("ticket/{ticketid}")]
        public async Task<IActionResult> Index([FromRoute] int ticketid)
        {
            Ticket ticket = null;
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = _db.Users.FirstOrDefault(u => u.Username == ownusername);
                Ticket thisticket = await _db.Tickets
                    .Include("Status")
                    .Include("Priority")
                    .Include("Assigner")
                    .Include("Assignee")
                    .Include("Priority").FirstOrDefaultAsync(u => u.Id == ticketid);
                if (thisticket != null && (from upr in _db.UserProjects
                     join account in _db.Users on upr.UserId equals account.Id
                     join proj in _db.Projects on upr.ProjectId equals proj.Id
                     where account == own && proj.Id == thisticket.ProjectId
                     select proj).Any())
                {
                    ViewData["AssigneeCandidate"] = (from upr in _db.UserProjects
                                            join account in _db.Users on upr.UserId equals account.Id
                                            join proj in _db.Projects on upr.ProjectId equals proj.Id
                                            join role in _db.Roles on upr.RoleId equals role.Id
                                            where proj.Id == thisticket.ProjectId && role.Id <= 2 && account != thisticket.Assignee
                                            select account).OrderBy(u => u.FirstName + " " + u.LastName);
                    ViewData["StatusCandidate"] = (from status in _db.Status
                                               where status !=thisticket.Status
                                               select status).OrderBy(u => u.Id);
                    ViewData["UserRole"] = (from upr in _db.UserProjects
                                            join account in _db.Users on upr.UserId equals account.Id
                                            join proj in _db.Projects on upr.ProjectId equals proj.Id
                                            join role in _db.Roles on upr.RoleId equals role.Id
                                            where account == own && proj.Id == thisticket.ProjectId
                                            select role.RoleName).FirstOrDefault().ToString();
                    ticket = thisticket;
                }
            }
            return View(ticket);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignTicket(int ticketid, int userid, DateTime deadline)
        {
            var ticket = _db.Tickets.Find(ticketid);
            if (ticket == null || User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                var target = _db.Users.Find(userid);
                // make sure both own and target users have a role in this project
                if (((from upr in _db.UserProjects
                      join account in _db.Users on upr.UserId equals account.Id
                      join proj in _db.Projects on upr.ProjectId equals proj.Id
                      where account == own && proj.Id == ticket.ProjectId && upr.RoleId <= 2
                      select proj).Count() == 1 && own == target) || 
                    ((from upr in _db.UserProjects
                     join account in _db.Users on upr.UserId equals account.Id
                     join proj in _db.Projects on upr.ProjectId equals proj.Id
                     where (account == own || account == target) && own != target && proj.Id == ticket.ProjectId && upr.RoleId <= 2
                     select proj).Count() == 2))
                {
                    ticket.AssignerId = own.Id;
                    ticket.AssigneeId = target.Id;
                    ticket.AssignedDateTime = DateTime.Now;
                    ticket.Deadline = deadline;
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("Index", new { ticketid });
        }

    }
}
