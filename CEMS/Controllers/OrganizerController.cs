using Microsoft.AspNetCore.Mvc;

namespace CEMS.Controllers;

public class OrganizerController: Controller
{
    public IActionResult OrganizerDashboard()
    {
        return View();
    }
}