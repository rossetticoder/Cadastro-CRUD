using System.ComponentModel.DataAnnotations;

namespace CadastroUsuariosApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        [Display(Name = "Nome completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "A senha deve ter no mínimo 4 caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de nascimento")]
        public DateTime DataNascimento { get; set; }

        [Phone(ErrorMessage = "Informe um telefone válido.")]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [Display(Name = "Data de cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}
