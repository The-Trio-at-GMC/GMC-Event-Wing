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

        public LoginController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult SplashScreen() => View();

        public IActionResult LoginPage() => View();

        public IActionResult ForgotPassword() => View();
        
        //Admin seeding
        public void SeedData()
        {
            if (!_context.Roles.Any())
            {
                _context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "Organizer" },
                    new Role { Name = "Student" }
                );
                _context.SaveChanges();
            }

            if (!_context.Users.Any(u => u.Email == "adminbruh@gmail.com"))
            {
                var admin = new User
                {
                    Email = "adminbruh@gmail.com",
                    Username = "Admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@123")
                };

                _context.Users.Add(admin);
                _context.SaveChanges();

                var adminRole = _context.Roles.First(r => r.Name == "Admin");

                _context.UserRoles.Add(new UserRole
                {
                    UserId = admin.Id,
                    RoleId = adminRole.Id
                });

                _context.SaveChanges();
            }
        }
        
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
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            
            var studentRole = _context.Roles.First(r => r.Name == "Student");

            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = studentRole.Id
            });

            _context.SaveChanges();

            return RedirectToAction("LoginPage");
        }

        [HttpPost]
        public IActionResult LoginPage(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && !u.IsDeleted);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ViewBag.Error = "Invalid email or password!";
                return View();
            }

            //Determining user's role before heading towards the specific dashboard
            var role = (from ur in _context.UserRoles
                        join r in _context.Roles on ur.RoleId equals r.Id
                        where ur.UserId == user.Id
                        select r.Name).FirstOrDefault();

            //Storing in session for role-based navigation
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("UserId", user.Id.ToString());

            //Checking if the device is remembered
            var rememberedToken = Request.Cookies["RememberDevice"];

            if (!string.IsNullOrEmpty(rememberedToken) &&
                rememberedToken == user.DeviceToken &&
                user.DeviceTokenExpiry > DateTime.UtcNow)
            {
                return RedirectToAction("LoginSuccess");
            }
            
            if (user.DeviceTokenExpiry != null &&
                user.DeviceTokenExpiry <= DateTime.UtcNow)
            {
                user.DeviceToken = null;
                user.DeviceTokenExpiry = null;
                _context.SaveChanges();
            }

            //Generating OTP
            var otp = new Random().Next(100000, 999999).ToString();

            user.OTP = otp;
            user.OTPExpiry = DateTime.UtcNow.AddMinutes(5);

            _context.SaveChanges();

            //Sending email for OTP
            var body = $"Your OTP for 2FA is: <b>{otp}</b>";

            _emailService.SendEmail(user.Email, "Verify it's You", body);
            
            //Storing email temporarily
            TempData["UserEmail"] = user.Email;

            return RedirectToAction("VerifyOtp");
        }

        [HttpPost]
        public IActionResult SendResetLink(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && !u.IsDeleted);

            if (user == null)
            {
                ViewBag.Error = "Email not found!";
                return View("ForgotPassword");
            }

            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            _context.SaveChanges();

            var resetLink = Url.Action("ResetPassword", "Login", 
                new { token = user.ResetToken }, Request.Scheme);

            var body = $@"
                <h2>Password Reset</h2>
                <p>Click below link to reset your password:</p>
                <a href='{resetLink}'>Reset Password</a>
                <p>Warning: This link expires in 15 minutes.</p>
            ";

            _emailService.SendEmail(user.Email, "Reset Your Password", body);

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

        public IActionResult VerifyOtp()
        {
            TempData.Keep("UserEmail");
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string code, bool rememberDevice)
        {
            var email = TempData["UserEmail"]?.ToString();

            if (email == null)
            {
                return RedirectToAction("LoginPage");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || user.OTP != code || user.OTPExpiry < DateTime.UtcNow)
            {
                ViewBag.Error = "Invalid or expired OTP!";
                return View();
            }

            //Clearing OTP
            user.OTP = null;
            user.OTPExpiry = null;
            _context.SaveChanges();

            //Saving cookie in the browser
            if (rememberDevice)
            {
                var deviceToken = Guid.NewGuid().ToString();

                user.DeviceToken = deviceToken;
                user.DeviceTokenExpiry = DateTime.UtcNow.AddDays(1); //1-day validity

                _context.SaveChanges();

                Response.Cookies.Append("RememberDevice", deviceToken,
                    new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(1),
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        IsEssential = true
                    });
            }
            
            return RedirectToAction("LoginSuccess");
        }

        public IActionResult LoginSuccess()
        {
            //Role-based redirection
            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Admin")
                return RedirectToAction("AdminDashboard", "Admin");

            if (role == "Organizer")
                return RedirectToAction("OrganizerDashboard", "Organizer");

            return Content("Hey, Student! Timro dashboard Bhawsie sanga chha ki uta bata push vayesi dekhauchhu, la?");
        }
    }
}