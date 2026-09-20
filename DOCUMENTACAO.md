# Documentação técnica — Cadastro de Usuários (C# / ASP.NET Core)

Este documento explica, arquivo por arquivo, o que cada parte do projeto faz, como o C# funciona
nessa aplicação e como a conexão com o banco de dados acontece por baixo dos panos.

---

## 1. Visão geral da arquitetura

O projeto segue o padrão **MVC (Model-View-Controller)**, o padrão de arquitetura padrão do
ASP.NET Core:

```
Requisição do navegador
        │
        ▼
   Controller  ──uses──►  Model  ◄──mapped by──  AppDbContext (EF Core)
        │                                                │
        ▼                                                ▼
      View (.cshtml)                              Banco SQLite (usuarios.db)
        │
        ▼
   HTML final enviado ao navegador
```

- **Model**: representa os dados (a classe `Usuario`).
- **View**: gera o HTML que o usuário vê (arquivos `.cshtml`).
- **Controller**: recebe a requisição, decide o que fazer, busca/salva dados e escolhe qual View
  devolver.

---

## 2. O que cada arquivo faz

### 2.1 Arquivos de configuração e inicialização

| Arquivo | Função |
|---|---|
| `CadastroUsuariosApp.csproj` | Arquivo de projeto do .NET. Declara o framework alvo (`net8.0`) e as dependências (pacotes NuGet): `Microsoft.EntityFrameworkCore.Sqlite`, `.Design` e `.Tools`. É o `package.json` do mundo .NET. |
| `appsettings.json` | Arquivo de configuração em JSON. Guarda a **connection string** do banco (`Data Source=usuarios.db`) e o nível de log. Lido pelo `Program.cs` na inicialização. |
| `Program.cs` | Ponto de entrada da aplicação. É o primeiro código C# executado quando você roda `dotnet run`. Configura os serviços (MVC, banco de dados), monta o pipeline de requisições HTTP e define as rotas. Detalhado na seção 4. |
| `.gitignore` | Diz ao Git quais pastas/arquivos não versionar (`bin/`, `obj/`, `*.db`). Não afeta a execução do site. |

### 2.2 Camada de dados (`Data/`)

| Arquivo | Função |
|---|---|
| `AppDbContext.cs` | A "ponte" entre C# e o banco de dados. Herda de `DbContext` (Entity Framework Core). Expõe `DbSet<Usuario> Usuarios`, que representa a tabela `Usuarios` no banco. Qualquer consulta LINQ feita em `_context.Usuarios` vira SQL automaticamente. |
| `SenhaHelper.cs` | Classe estática utilitária. Tem um único método, `GerarHash(senha)`, que transforma a senha digitada em um hash SHA256 antes de salvar — assim a senha nunca fica gravada em texto puro no banco. |

### 2.3 Camada de domínio (`Models/`)

| Arquivo | Função |
|---|---|
| `Usuario.cs` | Define a **entidade** Usuário: quais campos existem (`Nome`, `Email`, `Senha`, `DataNascimento`, `Telefone`, `DataCadastro`) e as regras de validação de cada um, usando *Data Annotations* (`[Required]`, `[EmailAddress]`, `[StringLength]` etc.). Essa mesma classe é usada em três lugares: como tabela do banco (via EF Core), como modelo das Views (formulários) e como parâmetro dos métodos do Controller. |

### 2.4 Camada de controle (`Controllers/`)

| Arquivo | Função |
|---|---|
| `HomeController.cs` | Controller simples com uma única ação (`Index`), que apenas devolve a página inicial. |
| `UsuariosController.cs` | O controller principal — implementa todo o CRUD. Cada método (`Index`, `Details`, `Create`, `Edit`, `Delete`) corresponde a uma URL e a uma ação sobre os dados. Detalhado na seção 3. |

### 2.5 Camada de apresentação (`Views/`)

| Arquivo | Função |
|---|---|
| `_ViewStart.cshtml` | Roda antes de qualquer View. Define `Layout = "_Layout"`, ou seja, aplica o layout padrão em todas as páginas automaticamente. |
| `_ViewImports.cshtml` | Centraliza `@using` (para não precisar repetir `using CadastroUsuariosApp.Models` em cada View) e habilita os *Tag Helpers* (`asp-for`, `asp-action`, `asp-route-id` etc.). |
| `Shared/_Layout.cshtml` | O "molde" visual de todas as páginas: sidebar roxa, menu de navegação, área de mensagens (`TempData["Mensagem"]`) e o local (`@RenderBody()`) onde o conteúdo de cada View entra. |
| `Shared/_ValidationScriptsPartial.cshtml` | Importa jQuery + jQuery Validation, para a validação dos formulários acontecer no navegador (client-side) além do C# (server-side). |
| `Home/Index.cshtml` | Página inicial: título, descrição do sistema, ficha técnica (`spec-panel`) e botões de atalho. |
| `Usuarios/Index.cshtml` | Lista todos os usuários em uma tabela, com campo de busca. Corresponde ao **R** (Read) do CRUD. |
| `Usuarios/Create.cshtml` | Formulário de cadastro de um novo usuário. Corresponde ao **C** (Create). |
| `Usuarios/Edit.cshtml` | Formulário de edição de um usuário existente. Corresponde ao **U** (Update). |
| `Usuarios/Details.cshtml` | Exibe os dados de um único usuário, somente leitura. |
| `Usuarios/Delete.cshtml` | Tela de confirmação antes de excluir. Corresponde ao **D** (Delete). |

### 2.6 Arquivos estáticos (`wwwroot/`)

| Arquivo | Função |
|---|---|
| `wwwroot/css/site.css` | Todo o visual do site: cores (paleta roxa), tipografia (Space Grotesk/Inter/IBM Plex Mono), layout da sidebar, estilos de tabela, formulário e botões. Servido diretamente ao navegador — nenhuma linha daqui passa pelo C#. |

> `wwwroot` é uma pasta especial do ASP.NET Core: tudo que está nela é servido como arquivo
> estático (CSS, JS, imagens), sem passar pelo pipeline de MVC.

---

## 3. Como o C# funciona nesse projeto

### 3.1 Do código-fonte à execução

O C# é uma linguagem **compilada**. Quando você roda `dotnet run`:

1. O compilador (`csc`, acionado por `dotnet build`) traduz todos os arquivos `.cs` para
   **IL (Intermediate Language)** — um bytecode intermediário, não o código de máquina final.
2. O **.NET Runtime (CLR)** executa esse IL, compilando-o *just-in-time* (JIT) para código de
   máquina real conforme necessário.
3. Isso é diferente de linguagens interpretadas linha a linha (como o VBScript do ASP clássico) —
   por isso C#/.NET tende a ter melhor performance em tempo de execução.

Os arquivos `.cshtml` são especiais: eles **não são pré-compilados junto com o resto** por padrão
no modo de desenvolvimento — o *Razor View Engine* os compila na primeira vez que são acessados,
misturando a parte HTML com a parte C# (tudo que vem depois de `@`).

### 3.2 O ciclo de vida de uma requisição

Usando como exemplo o clique em "Ver usuários cadastrados":

1. O navegador manda `GET /Usuarios`.
2. O **roteador** (configurado em `Program.cs`) casa essa URL com
   `UsuariosController.Index()`, seguindo a convenção `{controller}/{action}/{id?}`.
3. O método `Index` do controller é executado: ele consulta o banco via `_context.Usuarios`.
4. O resultado (uma `List<Usuario>`) é passado para a View `Usuarios/Index.cshtml` através de
   `return View(usuarios)`.
5. O Razor renderiza o `.cshtml`, substituindo cada `@item.Nome`, `@foreach`, etc. por HTML real.
6. O HTML final (sem nenhum vestígio de C#) é devolvido ao navegador.

### 3.3 Injeção de dependência (Dependency Injection)

Repare que `UsuariosController` recebe o `AppDbContext` assim:

```csharp
public class UsuariosController : Controller
{
    private readonly AppDbContext _context;

    public UsuariosController(AppDbContext context)
    {
        _context = context;
    }
    // ...
}
```

O Controller **não cria** o `AppDbContext` com `new AppDbContext(...)`. Quem cria e "entrega" essa
instância é o **container de injeção de dependência** do ASP.NET Core, configurado em
`Program.cs` com:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
```

Isso é um padrão central do C#/.NET moderno: as classes declaram do que precisam no construtor, e
o framework resolve e injeta automaticamente. Vantagens: código mais testável e desacoplado.

### 3.4 Programação assíncrona (`async`/`await`)

Quase todos os métodos do controller são `async`:

```csharp
public async Task<IActionResult> Index(string? busca)
{
    var usuarios = await query.OrderBy(u => u.Nome).ToListAsync();
    return View(usuarios);
}
```

`await` libera a thread do servidor enquanto espera a resposta do banco de dados (uma operação de
I/O), em vez de bloqueá-la parada. Isso permite que o mesmo servidor atenda muitas requisições
simultâneas com poucas threads — essencial em aplicações web C# de produção.

### 3.5 LINQ (Language Integrated Query)

Trechos como:

```csharp
query = query.Where(u => u.Nome.Contains(busca) || u.Email.Contains(busca));
```

são **LINQ** — uma forma de escrever consultas usando sintaxe C# nativa (em vez de strings SQL
soltas no código). O Entity Framework Core traduz essa expressão para SQL real na hora de
executar (veja seção 4.3).

---

## 4. Como o banco de dados se conecta

### 4.1 A connection string

Tudo começa em `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=usuarios.db"
}
```

`Data Source=usuarios.db` diz ao driver do SQLite para usar (ou criar, se não existir) um arquivo
chamado `usuarios.db` na pasta de execução do projeto. Diferente de um banco cliente-servidor
(como SQL Server ou PostgreSQL), o SQLite é um banco **embutido em arquivo** — não precisa de um
servidor rodando separadamente, o que facilita rodar o projeto em qualquer máquina.

### 4.2 Registro do EF Core no `Program.cs`

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=usuarios.db"));
```

- `builder.Configuration.GetConnectionString("DefaultConnection")` lê a string do
  `appsettings.json`.
- `options.UseSqlite(...)` diz ao Entity Framework Core qual **provedor de banco** usar (existe
  o equivalente `UseSqlServer`, `UseNpgsql` para PostgreSQL, `UseMySql`, etc. — trocar de banco é,
  na maioria dos casos, trocar essa linha + o pacote NuGet).
- Isso registra o `AppDbContext` no container de DI, disponível para ser injetado em qualquer
  controller (como visto na seção 3.3).

### 4.3 Do LINQ ao SQL

O `AppDbContext` (seção 2.2) expõe:

```csharp
public DbSet<Usuario> Usuarios { get; set; }
```

Esse `DbSet<Usuario>` representa a tabela `Usuarios`. Quando o controller escreve:

```csharp
await _context.Usuarios.Where(u => u.Nome.Contains(busca)).ToListAsync();
```

o Entity Framework Core (um **ORM** — Object-Relational Mapper) traduz isso, nos bastidores, em
algo equivalente a:

```sql
SELECT * FROM Usuarios WHERE Nome LIKE '%valor da busca%';
```

O mesmo vale para as outras operações do CRUD:

| Código C# (LINQ / EF Core) | SQL equivalente gerado |
|---|---|
| `_context.Add(usuario); await _context.SaveChangesAsync();` | `INSERT INTO Usuarios (...) VALUES (...)` |
| `await _context.Usuarios.FindAsync(id)` | `SELECT * FROM Usuarios WHERE Id = @id` |
| `usuarioBanco.Nome = novoNome; await _context.SaveChangesAsync();` | `UPDATE Usuarios SET Nome = @novoNome WHERE Id = @id` |
| `_context.Usuarios.Remove(usuario); await _context.SaveChangesAsync();` | `DELETE FROM Usuarios WHERE Id = @id` |

Ou seja: **nenhuma linha de SQL foi escrita à mão** no projeto — tudo é gerado pelo EF Core a
partir do código C#. Essa é a principal vantagem de usar um ORM.

### 4.4 Criação automática do banco

Em `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}
```

`EnsureCreated()` verifica se o arquivo `usuarios.db` e a tabela `Usuarios` já existem; se não
existirem, cria ambos com base nas classes mapeadas (`Usuario`). É uma forma simplificada de gerar
o esquema do banco, boa para projetos didáticos. Em um projeto maior/profissional, o mais comum é
usar **Migrations** (`dotnet ef migrations add NomeDaMigration` + `dotnet ef database update`),
que permitem versionar e evoluir o esquema do banco ao longo do tempo, algo que `EnsureCreated()`
não suporta.

### 4.5 Restrição de unicidade do e-mail

Em `AppDbContext.cs`:

```csharp
modelBuilder.Entity<Usuario>()
    .HasIndex(u => u.Email)
    .IsUnique();
```

Isso cria um **índice único** na coluna `Email` diretamente no banco — é uma segunda camada de
proteção contra e-mails duplicados, além da checagem manual feita no controller
(`_context.Usuarios.AnyAsync(u => u.Email == usuario.Email)`).

---

## 5. Resumo do fluxo completo (exemplo: cadastrar um usuário)

1. Usuário preenche o formulário em `Create.cshtml` e clica em "Salvar".
2. O navegador envia `POST /Usuarios/Create` com os dados do formulário.
3. `UsuariosController.Create(Usuario usuario)` recebe os dados já convertidos em um objeto
   `Usuario` (o *model binding* do ASP.NET Core faz isso automaticamente).
4. O controller valida os dados (`ModelState.IsValid`, usando as regras definidas em `Usuario.cs`).
5. A senha é transformada em hash (`SenhaHelper.GerarHash`).
6. `_context.Add(usuario)` + `await _context.SaveChangesAsync()` geram e executam o `INSERT` no
   `usuarios.db`.
7. O controller redireciona (`RedirectToAction`) para `Index`, que busca a lista atualizada e
   mostra o novo usuário na tabela.

Esse ciclo — **Model define os dados → View coleta/exibe → Controller orquestra → EF Core
traduz para SQL → SQLite persiste** — se repete, com variações, em todas as operações do CRUD.
