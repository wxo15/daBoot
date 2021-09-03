using daBoot.Data;
using daBoot.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace daBoot.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }


        [HttpGet("home/accessdenied")]
        public IActionResult Denied()
        {
            return View();
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("home/login")]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Validate(string username, string password, string returnUrl)
        {
            Account user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            /*int KeySize = 32;
            int SaltSize = 16;
            int IterationNum = 10000;
            var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                IterationNum,
                HashAlgorithmName.SHA256);
            var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
            var salt = Convert.ToBase64String(algorithm.Salt);
            string str = $"{IterationNum}.{KeySize}.{SaltSize}.{salt}.{key}";
            */
            if (user == null)
            {
                ViewData["ReturnUrl"] = returnUrl;
                TempData["Error"] = "Username/password invalid.";
                return View("login");
            }

            var parts = user.Password.Split('.', 5);
            var IterationNum = Convert.ToInt32(parts[0]);
            var KeySize = Convert.ToInt32(parts[1]);
            var SaltSize = Convert.ToInt32(parts[2]);
            var salt = Convert.FromBase64String(parts[3]);
            var key = parts[4];

            var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                IterationNum,
                HashAlgorithmName.SHA256);
            var keyToCheck = Convert.ToBase64String(algorithm.GetBytes(KeySize));

            if (!keyToCheck.SequenceEqual(key))
            {
                ViewData["ReturnUrl"] = returnUrl;
                TempData["Error"] = "Username/password invalid.";
                return View("login");
            }
            var claims = new List<Claim>();
            claims.Add(new Claim("username", username));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
            claims.Add(new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName));
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);
            return Redirect(returnUrl);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password, string firstname, string lastname, string email, string returnUrl)
        {
            if (username == null || password == null || firstname == null || lastname == null || email == null)
            {
                ViewData["ReturnUrl"] = returnUrl;
                TempData["Error"] = "All fields required.";
                return View("login");
            }

            Account user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user != null)
            {
                ViewData["ReturnUrl"] = returnUrl;
                TempData["Error"] = "Username already exist.";
                return View("login");
            }

            int KeySize = 32;
            int SaltSize = 16;
            int IterationNum = 10000;
            var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                IterationNum,
                HashAlgorithmName.SHA256);
            var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
            var salt = Convert.ToBase64String(algorithm.Salt);
            string str = $"{IterationNum}.{KeySize}.{SaltSize}.{salt}.{key}";

            var obj = new Account(username, str, firstname, lastname, email);
            _db.Users.Add(obj);
            _db.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim("username", username),
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, firstname + " " + lastname)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);
            return Redirect(returnUrl);
        }

        [Authorize]
        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
