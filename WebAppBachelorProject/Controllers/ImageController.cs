using Microsoft.AspNetCore.Mvc;

namespace WebAppBachelorProject.Controllers
{
    public class ImageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
