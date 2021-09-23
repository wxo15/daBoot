using daBoot.Data;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            return View();
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

    }
}
