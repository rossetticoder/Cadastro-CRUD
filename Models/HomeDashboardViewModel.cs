namespace CadastroUsuariosApp.Models;

public class HomeDashboardViewModel
{
    public int TotalAlunos { get; set; }
    public int NovosUltimos30Dias { get; set; }
    public int SemTelefone { get; set; }
    public List<Usuario> CadastrosRecentes { get; set; } = new();
}
