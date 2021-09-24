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
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _db;

        public NotificationController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("mynotifications")]
        public async Task<IActionResult> NotificationList()
        {
            IEnumerable<Notification> objList = null;
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                objList = _db.Notifications.Where(u => u.NotificationUser == own).OrderByDescending(u => u.NotificationTimeStamp);
            }
            return View(objList);
        }

        [HttpPost("notification/getcount")]
        public int GetNotificationCount()
        {
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = _db.Users.FirstOrDefault(u => u.Username == ownusername);
                int count = _db.Notifications.Where(u => u.NotificationUser == own && u.IsRead == false).Count();
                return count;
            }
            else
            {
                return 0;
            }
        }

        [HttpPost("notification/{readunread}/{notificationid}")]
        public async Task<string> ReadUnread(string readunread, int notificationid)
        {
            if (User.Identity.IsAuthenticated && (readunread == "read" || readunread == "unread"))
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                var target = _db.Notifications.Find(notificationid);
                
                if (target != null && target.NotificationUser == own)
                {
                    if (target.IsRead && readunread == "unread")
                    {
                        target.IsRead = false;
                    }
                    else if (!target.IsRead && readunread == "read")
                    {
                        target.IsRead = true;
                    }
                    _db.SaveChanges();
                    return "Success";
                }
            }
            return "Failed";
        }

        [HttpPost("notification/readall")]
        public async Task<string> ReadAll()
        {
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                var targets = _db.Notifications.Where(u => u.NotificationUser == own && u.IsRead == false);

                foreach (var target in targets)
                {
                    target.IsRead = true;
                }
                _db.SaveChanges();
                return "Success";
            }
            return "Failed";
        }

        [HttpPost("notification/posted/{notificationid}")]
        public async Task<string> Posted(int notificationid)
        {
            if (User.Identity.IsAuthenticated)
            {
                var ownusername = User.Claims.FirstOrDefault(c => c.Type == "username").Value;
                var own = await _db.Users.FirstOrDefaultAsync(u => u.Username == ownusername);
                var target = _db.Notifications.Find(notificationid);

                if (target != null && target.NotificationUser == own)
                {
                    int startIndex = 0;
                    if (target.NotificationString.Length - 18 > 0) 
                    {
                        startIndex = target.NotificationString.Length - 18;
                    }
                    if (target.NotificationString.Substring(startIndex) != "[Action performed]")
                    {
                        target.NotificationString += " [Action performed]";
                        target.PostAction = null;
                    }
                    _db.SaveChanges();
                    return "Success";
                }
            }
            return "Failed";
        }

    }
}
