using Microsoft.AspNetCore.Mvc;

namespace CadastroUsuariosApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
