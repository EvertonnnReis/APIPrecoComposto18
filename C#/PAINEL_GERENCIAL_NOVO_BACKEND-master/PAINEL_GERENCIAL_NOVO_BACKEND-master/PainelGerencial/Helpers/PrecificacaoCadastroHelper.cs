using AbacosWSERP;
using PainelGerencial.Domain.PrecificacaoCadastro;
using System;
using System.Linq;

namespace PainelGerencial.Helpers
{
    public class PrecificacaoCadastroHelper
    {
        public static DadosPreco[] PrepararDadosSolicitacao(Precificacao_Model solicitacao)
        {
            var dadosPreco = new DadosPreco
            {
                CodigoProduto = solicitacao.ProdutoCodigoExterno,
                CodigoProdutoPai = solicitacao.ProdutoCodigoExternoPai,
                NomeLista = solicitacao.ListaPreco.Nome,
                PrecoTabela = Convert.ToDouble(solicitacao.ProdutoPrecoTabelaNovo),
                PrecoPromocional = Convert.ToDouble(solicitacao.ProdutoPrecoPromocionalNovo),
                DataInicioPromocao = "01012017",
                DataTerminoPromocao = "01012050"
            };
            return new[] { dadosPreco };
        }

        public static bool EnviarWsdlAbacos(DadosPreco[] dados)
        {
            var svc = new AbacosWSERPSoapClient(AbacosWSERPSoapClient.EndpointConfiguration.AbacosWSERPSoap12);
            var resposta = svc.AtualizarPrecoAsync("214137DA-076F-4E3B-934E-A4029A5C9022", dados);
            var produto = resposta.Result.Rows.ToList().FirstOrDefault();

            return produto?.Resultado.Tipo == "tdreSucesso";
        }
    }
}
