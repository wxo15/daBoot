using daBoot.Data;
using daBoot.Models;
using Microsoft.AspNetCore.Authorization;
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

        public class ChartSubModel
        {
            public string DimensionOne { get; set; }
            public int Quantity { get; set; }
        }

        public class StackedViewModel
        {
            public string StackedDimensionOne { get; set; }
            public List<ChartSubModel> LstData { get; set; }
        }

        public class DashboardModel
        {
            public List<StackedViewModel> AssignedTickets { get; set; }
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
                    .Include("Submitter")
                    .Include("Assigner")
                    .Include("Assignee")
                    .Include("Comments")
                    .Include("Priority").FirstOrDefaultAsync(u => u.Id == ticketid);
                foreach (var comment in thisticket.Comments)
                {
                    comment.CommentUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == comment.UserId);
                }
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
                    // cannot be Late, as thats determined by deadline
                    ViewData["StatusCandidate"] = (from status in _db.Status
                                               where status !=thisticket.Status && status.StatusName != "Late"
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
            if (ticket != null && User.Identity.IsAuthenticated)
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
                    UpdateStatusOpenLate();
                }
            }
            return RedirectToAction("Index", new { ticketid });
        }

        [HttpPost("statusupdate")]
        public async Task<IActionResult> UpdateStatus(int ticketid, int statusid)
        {
            var ticket = _db.Tickets.Find(ticketid);
            if (ticket != null && User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                // make sure authenticated user is the assignee
                if (ticket.Assignee == own)
                {
                    ticket.StatusUpdated = DateTime.Now;
                    ticket.StatusId = statusid;
                    _db.SaveChanges();
                    UpdateStatusOpenLate();
                }
            }
            return RedirectToAction("Index", new { ticketid });
        }



        [HttpGet("createticket/{projectid}")]
        public IActionResult CreateTicket(int projectid)
        {
            return View(projectid);
        }

        [HttpPost("createticket")]
        public async Task<IActionResult> CreateTicket(int projectid, string subject, string priority, string description)
        {
            var project = _db.Projects.Find(projectid);
            int? priorityid = _db.Priority.FirstOrDefault(p => p.PriorityName == priority).Id;
            if (project != null && priorityid != null)
            {
                int userid = -1;
                if (User.Identity.IsAuthenticated)
                {
                    var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                    var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                    userid = own.Id;
                }
                Ticket ticket = new Ticket(projectid, subject, (int)priorityid, description, userid);
                _db.Tickets.Add(ticket);
                _db.SaveChanges();
            }
            return Redirect("~/thankyou");
        }

        [HttpGet("thankyou")]
        public IActionResult ThankYou()
        {
            return View();
        }

        [HttpPost("newcomment")]
        public async Task<IActionResult> AddComment(int ticketid, string commentstr)
        {
            Ticket ticket = _db.Tickets.Find(ticketid);
            if (ticket != null && User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                int userid = own.Id;
                // only allow new comment if user is part of the project.
                UserProject userproject = _db.UserProjects.Find(userid, ticket.ProjectId);
                if (userproject != null)
                {
                    Comment comment = new(userid, ticketid, commentstr);
                    _db.Comments.Add(comment);
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("Index", new { ticketid });
        }

        [Authorize]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            
            
            UpdateStatusOpenLate();
            var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
            var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
            // assigned ticket stacked bar chart
            List<StackedViewModel> assignedticketlst = new();
            StackedViewModel openobj = new()
            { StackedDimensionOne = "Open", LstData = new List<ChartSubModel>() {
                            new ChartSubModel()
                            {DimensionOne = "High", Quantity = _db.Tickets.Where(u => u.Assignee == own && u.Status.StatusName == "Open" && u.Priority.PriorityName == "High").Count() },
                            new ChartSubModel()
                            {DimensionOne = "Medium", Quantity = _db.Tickets.Where(u => u.Assignee == own && u.Status.StatusName == "Open" && u.Priority.PriorityName == "Medium").Count() },
                            new ChartSubModel()
                            {DimensionOne = "Low", Quantity = _db.Tickets.Where(u => u.Assignee == own && u.Status.StatusName == "Open" && u.Priority.PriorityName == "Low").Count() }
                    } };
            StackedViewModel lateobj = new()
            { StackedDimensionOne = "Late", LstData = new List<ChartSubModel>() {
                            new ChartSubModel()
                            {DimensionOne = "High", Quantity = _db.Tickets.Where(u => u.Assignee == own && u.Status.StatusName == "Late" && u.Priority.PriorityName == "High").Count() },
                            new ChartSubModel()
                            {DimensionOne = "Medium", Quantity = _db.Tickets.Where(u => u.Assignee == own && u.Status.StatusName == "Late" && u.Priority.PriorityName == "Medium").Count() },
                            new ChartSubModel()
                            {DimensionOne = "Low", Quantity = _db.Tickets.Where(u => u.Assignee == own && u.Status.StatusName == "Late" && u.Priority.PriorityName == "Low").Count() }
                    } };
            assignedticketlst.Add(openobj);
            assignedticketlst.Add(lateobj);

            // role pie chart
            var rolepiemodel = new List<ChartSubModel>();
            rolepiemodel.Add(new ChartSubModel
            {
                DimensionOne = "Lead",
                Quantity = _db.UserProjects.Where(u => u.User == own && u.Role.RoleName == "Lead").Count()
            });
            rolepiemodel.Add(new ChartSubModel
            {
                DimensionOne = "Dev",
                Quantity = _db.UserProjects.Where(u => u.User == own && u.Role.RoleName == "Dev").Count()
            });
            rolepiemodel.Add(new ChartSubModel
            {
                DimensionOne = "Tester",
                Quantity = _db.UserProjects.Where(u => u.User == own && u.Role.RoleName == "Tester").Count()
            });
            // submitted ticket
            var submittedmodel = new List<ChartSubModel>();
            submittedmodel.Add(new ChartSubModel
            {
                DimensionOne = "Submitted",
                Quantity = own.SubmittedTickets.Where(u => u.Status.StatusName == "Submitted").Count()
            });
            submittedmodel.Add(new ChartSubModel
            {
                DimensionOne = "Rejected",
                Quantity = own.SubmittedTickets.Where(u => u.Status.StatusName == "Rejected").Count()
            });
            submittedmodel.Add(new ChartSubModel
            {
                DimensionOne = "Closed",
                Quantity = own.SubmittedTickets.Where(u => u.Status.StatusName == "Closed").Count()
            });
            submittedmodel.Add(new ChartSubModel
            {
                DimensionOne = "Late",
                Quantity = own.SubmittedTickets.Where(u => u.Status.StatusName == "Late").Count()
            });
            submittedmodel.Add(new ChartSubModel
            {
                DimensionOne = "Open",
                Quantity = own.SubmittedTickets.Where(u => u.Status.StatusName == "Open").Count()
            });
            ViewData["assignedticketlst"] = assignedticketlst;
            ViewData["rolepie"] = rolepiemodel;
            ViewData["submittedticketpie"] = submittedmodel;
            return View(assignedticketlst);  
       }  


        public void UpdateStatusOpenLate()
        {
            DateTime dtnow = DateTime.Now;
            int openid = _db.Status.FirstOrDefault(u => u.StatusName == "Open").Id;
            int lateid = _db.Status.FirstOrDefault(u => u.StatusName == "Late").Id;
            foreach (var ticket in _db.Tickets.Include("Status").Where(u => u.Status != null).ToList())
            {
                if (ticket.Status.StatusName == "Open" && ticket.Deadline < dtnow)
                {
                    ticket.StatusId = lateid;
                    _db.SaveChanges();
                }
                if (ticket.Status.StatusName == "Late" && ticket.Deadline > dtnow)
                {
                    ticket.StatusId = openid;
                    _db.SaveChanges();
                }
            };
        }
    }
}
