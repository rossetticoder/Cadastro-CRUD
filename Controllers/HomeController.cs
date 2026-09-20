using CadastroUsuariosApp.Data;
using CadastroUsuariosApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroUsuariosApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var inicio = DateTime.Now.AddDays(-30);
            var model = new HomeDashboardViewModel
            {
                TotalAlunos = await _context.Usuarios.CountAsync(),
                NovosUltimos30Dias = await _context.Usuarios.CountAsync(u => u.DataCadastro >= inicio),
                SemTelefone = await _context.Usuarios.CountAsync(u => u.Telefone == null || u.Telefone == ""),
                CadastrosRecentes = await _context.Usuarios.OrderByDescending(u => u.DataCadastro).Take(5).ToListAsync()
            };
            return View(model);
        }
    }
}
