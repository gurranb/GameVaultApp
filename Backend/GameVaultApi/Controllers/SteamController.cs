using Microsoft.AspNetCore.Mvc;

namespace GameVaultApi.Controllers
{
    public class SteamController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
