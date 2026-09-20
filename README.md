# Cadastro de Usuários — C# (ASP.NET Core MVC)

Sistema web de **cadastro de usuários com CRUD completo** (Inserir, Consultar, Alterar, Excluir),
desenvolvido em **C#** com **ASP.NET Core MVC**, **Entity Framework Core** e banco de dados **SQLite**.

## Stack utilizada

| Camada | Tecnologia |
|---|---|
| Linguagem | C# (.NET 8) |
| Framework Web | ASP.NET Core MVC |
| Acesso a dados | Entity Framework Core |
| Banco de dados | SQLite (arquivo local `usuarios.db`, criado automaticamente) |
| Front-end | Razor Views + CSS próprio (sem Bootstrap) |

Essa combinação (C# + ASP.NET Core + SQL Server/SQLite) é exatamente o cenário citado no
enunciado do seminário. Aqui usamos SQLite no lugar do SQL Server para que o projeto rode em
qualquer computador sem precisar instalar um servidor de banco separado — a troca para SQL
Server exige apenas mudar a connection string e o pacote NuGet
(`Microsoft.EntityFrameworkCore.SqlServer`).

## Design

A interface **Ativa** adapta o cadastro para a rotina de uma academia. O painel inicial mostra totais reais de alunos, cadastros dos últimos 30 dias, registros sem telefone e os cinco cadastros mais recentes. A navegação usa uma barra lateral escura e cartões em cores suaves. O layout é responsivo.

## Estrutura do projeto

```
CadastroUsuariosApp/
├── Controllers/
│   ├── HomeController.cs       -> página inicial
│   └── UsuariosController.cs   -> CRUD (Index, Create, Edit, Details, Delete)
├── Models/
│   └── Usuario.cs              -> entidade Usuario (com validações via Data Annotations)
├── Data/
│   ├── AppDbContext.cs         -> contexto do Entity Framework Core
│   └── SenhaHelper.cs          -> hash de senha (SHA256, fins didáticos)
├── Views/
│   ├── Home/                   -> página inicial
│   ├── Usuarios/               -> Index, Create, Edit, Details, Delete
│   └── Shared/                 -> layout (_Layout.cshtml) e partial de validação
├── wwwroot/css/site.css        -> estilos do design system
├── Program.cs                  -> configuração da aplicação (DI, EF Core, rotas)
├── appsettings.json            -> connection string do banco
└── CadastroUsuariosApp.csproj  -> dependências do projeto
```

## Como rodar

Pré-requisito: [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado.

```bash
cd CadastroUsuariosApp
dotnet restore
dotnet run
```

O terminal vai mostrar algo como `Now listening on: https://localhost:5001`.
Abra esse endereço no navegador — o banco `usuarios.db` (SQLite) é criado automaticamente
na primeira execução, já com a tabela de usuários.

## Funcionalidades (mapeadas ao requisito do seminário)

- **Cadastro de usuários**: formulário em `/Usuarios/Create` com validação (nome, e-mail único,
  senha com hash SHA256, data de nascimento, telefone).
- **Inserir**: `POST /Usuarios/Create`
- **Consultar**: `GET /Usuarios` (lista com busca por nome/e-mail) e `GET /Usuarios/Details/{id}`
- **Alterar**: `GET/POST /Usuarios/Edit/{id}`
- **Excluir**: `GET /Usuarios/Delete/{id}` (confirmação) + `POST /Usuarios/Delete/{id}`

## Observações para a apresentação

- O e-mail é validado como único no banco (não permite dois cadastros com o mesmo e-mail).
- A senha nunca é salva em texto puro — é armazenada como hash SHA256 (em produção real, o
  recomendado seria BCrypt/Argon2 ou o ASP.NET Core Identity).
- `db.Database.EnsureCreated()` em `Program.cs` cria o banco automaticamente sem precisar rodar
  migrations manualmente — ótimo para demonstração; em um projeto maior, usaria-se
  `dotnet ef migrations add` + `dotnet ef database update`.

