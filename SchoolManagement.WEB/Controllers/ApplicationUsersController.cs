using Microsoft.AspNetCore.Mvc;

namespace SchoolManagement.WEB.Controllers
{
    public class ApplicationUsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
