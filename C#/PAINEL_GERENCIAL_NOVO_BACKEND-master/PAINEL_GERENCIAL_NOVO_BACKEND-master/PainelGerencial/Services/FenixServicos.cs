using Newtonsoft.Json;
using PainelGerencial.Controller;
using PainelGerencial.Domain.Fenix;
using PainelGerencial.Repository;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace PainelGerencial.Services
{
    public class FenixServicos : IFenixServicos
    {
        private const string _url = "http://api.dakotaparts.com.br:53236/";
        private readonly ControleAcessoController _controleAcesso;
        private readonly IPrecificacaoCadastroRepository _precificacaoRepository;
        public FenixServicos(
            ControleAcessoController controleAcesso, 
            IPrecificacaoCadastroRepository precificacaoRepository
        )
        {
            _controleAcesso = controleAcesso;
            _precificacaoRepository = precificacaoRepository;
        }
        public async Task<ProdutoComListaPrecoFenixViewModel> BuscarProduto(int listaPreco, string codigoExterno, bool somenteAtivos)
        {
            var client = new RestClient(_url);
            var request = new RestRequest("Produto/Buscar", Method.Get);
            request.AddHeader("Accept", "application/json");
            string token = _controleAcesso.retornaToken().Token.ToString();
            request.AddHeader("Authorization", $"bearer {token}");
            request.AddParameter("codigoExterno", codigoExterno);
            request.AddParameter("listaPreco", listaPreco);
            request.AddParameter("SomenteAtivos", somenteAtivos);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);

            var produto = JsonConvert.DeserializeObject<ProdutoComListaPrecoFenixViewModel>(response.Content);

            return produto;
        }

        public async Task<ProdutoKitComComponentesViewModel> BuscarProdutoKit(int listaPreco, string codigoExterno)
        {
            var client = new RestClient(_url);
            var request = new RestRequest("ProdutoKit/Buscar", Method.Get);
            request.AddHeader("Accept", "application/json");
            string token = _controleAcesso.retornaToken().Token.ToString();
            request.AddHeader("Authorization", $"bearer {token}");
            request.AddParameter("codigoExterno", codigoExterno);
            request.AddParameter("listaPreco", listaPreco);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);

            var produto = JsonConvert.DeserializeObject<ProdutoKitComComponentesViewModel>(response.Content);

            return produto;
        }

        public bool VerificarPrecificacaoRecente(int tabela, string dk)
        {
            var recente = _precificacaoRepository.ObterRecente(tabela, dk);
            return recente != null ? true : false;
        }
    }
}
