using Microsoft.AspNetCore.Mvc;

namespace CEMS.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult SplashScreen()
        {
            return View();
        }

        public IActionResult LoginPage()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult SendResetCode()
        {
            return RedirectToAction("VerifyResetCode", "Login");
        }

        public IActionResult VerifyResetCode()
        {
            return View();
        }

        public IActionResult ResetPassword()
        {
            return View();
        }
    }
}