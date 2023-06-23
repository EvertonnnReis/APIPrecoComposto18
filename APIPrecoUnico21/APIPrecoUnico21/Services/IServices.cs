using APIPrecoUnico21.Models;

namespace APIPrecoUnico21.Services
{
    public interface IServices
    {
        Task<dynamic> ObterRegistros(string _interface);
        Task EnviarApiSDP(string caminhoArquivo, List<string> listaSku);
        Task SalvarRegistrosEmArquivo(List<RegistroModel> registros, string caminhoArquivo);
    }
}
