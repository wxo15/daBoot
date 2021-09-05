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
            return View(user);
        }
    }
}
