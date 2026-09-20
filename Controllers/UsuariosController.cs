using CadastroUsuariosApp.Data;
using CadastroUsuariosApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CadastroUsuariosApp.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios  -> CONSULTAR (listar todos, com busca opcional por nome/e-mail)
        public async Task<IActionResult> Index(string? busca)
        {
            var query = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                query = query.Where(u => u.Nome.Contains(busca) || u.Email.Contains(busca));
            }

            ViewData["Busca"] = busca;
            var usuarios = await query.OrderBy(u => u.Nome).ToListAsync();
            return View(usuarios);
        }

        // GET: Usuarios/Details/5 -> CONSULTAR (detalhe de um registro)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // GET: Usuarios/Create -> INSERIR
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create -> INSERIR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,Email,Senha,DataNascimento,Telefone")] Usuario usuario)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
            {
                ModelState.AddModelError("Email", "Já existe um usuário cadastrado com este e-mail.");
            }

            if (ModelState.IsValid)
            {
                usuario.Senha = SenhaHelper.GerarHash(usuario.Senha);
                usuario.DataCadastro = DateTime.Now;

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                TempData["Mensagem"] = "Aluno cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        // GET: Usuarios/Edit/5 -> ALTERAR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuarios/Edit/5 -> ALTERAR
        // "novaSenha" chega como campo solto do formulário (não faz parte do model
        // binding do Usuario) porque a view de edição nunca exibe nem reenvia o hash
        // salvo — só envia uma nova senha em texto puro se o usuário quiser trocá-la.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,DataNascimento,Telefone,DataCadastro")] Usuario usuarioForm, string? novaSenha)
        {
            if (id != usuarioForm.Id) return NotFound();

            // O campo Senha do model não vem do formulário de edição, então não deve ser validado aqui.
            ModelState.Remove("Senha");

            if (await _context.Usuarios.AnyAsync(u => u.Email == usuarioForm.Email && u.Id != id))
            {
                ModelState.AddModelError("Email", "Já existe outro usuário com este e-mail.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioBanco = await _context.Usuarios.FindAsync(id);
                    if (usuarioBanco == null) return NotFound();

                    usuarioBanco.Nome = usuarioForm.Nome;
                    usuarioBanco.Email = usuarioForm.Email;
                    usuarioBanco.DataNascimento = usuarioForm.DataNascimento;
                    usuarioBanco.Telefone = usuarioForm.Telefone;

                    // Só gera novo hash se o usuário preencheu o campo "Nova senha"
                    if (!string.IsNullOrWhiteSpace(novaSenha))
                    {
                        usuarioBanco.Senha = SenhaHelper.GerarHash(novaSenha);
                    }

                    await _context.SaveChangesAsync();
                    TempData["Mensagem"] = "Aluno atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Usuarios.AnyAsync(u => u.Id == id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(usuarioForm);
        }

        // GET: Usuarios/Delete/5 -> EXCLUIR (confirmação)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuarios/Delete/5 -> EXCLUIR
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                TempData["Mensagem"] = "Aluno excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

