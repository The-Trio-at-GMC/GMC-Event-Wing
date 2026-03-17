using Microsoft.AspNetCore.Mvc;

namespace CEMS.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult SplashScreen()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}