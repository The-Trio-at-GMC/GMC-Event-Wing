using CEMS.Data;
using CEMS.Models;
using CEMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace CEMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        //Injecting Services and Db
        public LoginController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        //Returning views simply
        public IActionResult SplashScreen() => View();

        public IActionResult LoginPage() => View();

        public IActionResult ForgotPassword() => View();
        
        [HttpPost]
        public IActionResult SignUp(string email, string username, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View("LoginPage");
            }

            var exists = _context.Users.Any(u => u.Email == email && !u.IsDeleted);

            if (exists)
            {
                ViewBag.Error = "Email already exists!";
                return View("LoginPage");
            }

            var user = new User
            {
                Email = email,
                Username = username,
                Password = BCrypt.Net.BCrypt.HashPassword(password) //Hashing's been applied here
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("LoginPage");
        }

        [HttpPost]
        public IActionResult LoginPage(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == email && !u.IsDeleted);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password!";
                return View();
            }
            
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ViewBag.Error = "Invalid email or password!";
                return View();
            }
            
            return RedirectToAction("LoginPage");
        }

        [HttpPost]
        public IActionResult SendResetLink(string email)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == email && !u.IsDeleted);

            if (user == null)
            {
                ViewBag.Error = "Email not found!";
                return View("ForgotPassword");
            }

            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            _context.SaveChanges();

            var resetLink = Url.Action(
                "ResetPassword",
                "Login",
                new { token = user.ResetToken },
                Request.Scheme
            );

            var body = $@"
                <h2>Password Reset</h2>
                <p>Click below link to reset your password:</p>
                <a href='{resetLink}'>Reset Password</a>
                <p>Warning: This link expires in 15 minutes.</p>
            ";

            _emailService.SendEmail(user.Email, "GMC Event Wing: Reset Your Password", body);

            return RedirectToAction("LoginPage");
        }

        public IActionResult ResetPassword(string token)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.ResetToken == token &&
                u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                return Content("Invalid or expired link.");
            }

            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string password, string confirmPassword, string token)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.ResetToken == token &&
                u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                return Content("Invalid request.");
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View();
            }
            
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);

            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            _context.SaveChanges();

            return RedirectToAction("LoginPage");
        }
    }
}