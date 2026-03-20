using Microsoft.AspNetCore.Mvc;

namespace CEMS.Controllers
{

    public class AdminController : Controller
    {
        public IActionResult AdminDashboard()
        {
            return View();
        }
    }
}