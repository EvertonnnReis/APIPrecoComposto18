using AbacosWSERP;
using AbacosWSPlataforma;
using Domain.Abacos;
using Domain.Vtex;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PainelGerencial.Domain;
using PainelGerencial.Domain.Abacos;
using PainelGerencial.Domain.PrecificacaoCadastro;
using PainelGerencial.Domain.Vtex;
using PainelGerencial.Repository;
using PainelGerencial.Services;
using PainelGerencial.Utils;
using PainelGerencial.Utils.Enums;
using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DadosPreco = AbacosWSERP.DadosPreco;

namespace PainelGerencial.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class AbacosController : ControllerBase
    {
        private static IAbacosRepository _abacosRepository;
        private static AbacosWSPlataformaSoapClient _abacosWs;
        private const string abacosPlataforma = "48B7078A-609D-4EB5-A677-C25E08D5938A";
        private static IVtexRepository _vtexRepository;
        private const string _csv = @"\\192.168.0.179\inetpub\wwwroot\PainelGerencialAPI\download\csv\";
        private static IAbacosServicos _abacosServicos;

        public AbacosController(IAbacosRepository abacosRepository,
            IWsAbacos wsAbacos,
            IVtexRepository vtexRepository,
            IAbacosServicos abacosServicos)
        {
            _abacosRepository = abacosRepository;
            _abacosWs = wsAbacos.AbacosWs();
            _vtexRepository = vtexRepository;
            _abacosServicos = abacosServicos;
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ProdutoPorCodigoExterno(string codigoExterno)
        {
            var dados = _abacosRepository.GetProsCod(codigoExterno);

            if (dados.Any())
            {
                return Ok(dados);
            } 
            else
            {
                return NotFound();
            }
            
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult CargaProduto(string pedidoCodigoExterno)
        {
            var codigoAbacos = _abacosRepository.GetProdutosPorPedido(pedidoCodigoExterno);

            return Ok(new
            {
                codigo = codigoAbacos.FirstOrDefault().PROS_COD
            });
        }

        [HttpGet]
        [Route("PedidoStatus")]
        public IActionResult PedidoStatus(string pedidoCodigoExterno)
        {
            var statusPedido = _abacosRepository.PedidoStatus(pedidoCodigoExterno);

            return Ok(statusPedido);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult PedidoIntegrado(string pedidoCodigoExterno)
        {
            var pedidoIntegrado = _abacosRepository.PedidoIntegrado(pedidoCodigoExterno);

            return Ok(pedidoIntegrado);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> PedidoNotaFiscal(string pedidoCodigoExterno)
        {
            List<TCOM_PEDSAI> pedido = (List<TCOM_PEDSAI>)await _abacosRepository.PedidoNotaFiscal(pedidoCodigoExterno);

            return Ok(pedido.FirstOrDefault());
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ConsomeProdutosDisponiveis()
        {
            try
            {
                
                 var produtos = await _abacosWs.ProdutosDisponiveisAsync(abacosPlataforma);

                if (produtos.ResultadoOperacao.Tipo == "tdreSucessoSemDados")
                {
                    return NoContent();
                }

                var produtosVm = _abacosServicos.ConverterListaProduto(produtos.Rows);

                //foreach (var item in produtosVm)
                //{
                //    // Atualiza os produtos na VTEX
                //    if (await _abacosServicos.ProdutoOuSku(item))
                //    {
                //        _abacosServicos.ConfirmarRecebimentoProduto(item.ProtocoloProduto);
                //    }
                //}

                var primeiroProduto = produtosVm.Take(1).ToList();
                foreach (var produto in primeiroProduto)
                {
                    if (await _abacosServicos.ProdutoOuSku(produto))
                    {
                        _abacosServicos.ConfirmarRecebimentoProduto(produto.ProtocoloProduto);
                    }
                }

                return Ok(new
                {
                    Quantidade = produtosVm,
                });
            }
            catch (Exception e)
            {
                return NotFound(new { Message = e.Message });
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<bool> EnviarWsdlAbacos(DadosPreco[] dados)
        {
            var retorno = await _abacosServicos.EnviarWsdlAbacos(dados);

            return retorno;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IEnumerable<dynamic>> ListaPrecoListar()
        {
            var lista = _abacosRepository.ListaPrecoListar();

            return lista.ToList();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<bool> AlterarPrecoComponenteKit(DadosItemKit[] componente)
        {
            var retorno = await _abacosServicos.AlterarPrecoComponenteKit(componente);

            return retorno;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<dynamic> ListarPrecoPorDKPai(string produtoDkPai)
        {
            var resposta = await _abacosRepository.ListaPrecoPorDKPai(produtoDkPai);

            return resposta;
        }

        [HttpGet]
        [Route("relatorio-estoque-minimo")]
        public async Task<IActionResult> RelatorioEstoqueMinimo(int filtro)
        {

            var dados = await _abacosRepository.BuscaRelatorioEstoqueMinimo(filtro);

            var total = dados.FirstOrDefault().TOTAL;

            return Ok(new { dados = dados.ToList(), total = total });
        }

        [HttpGet]
        [Route("relatorio-estoque-minimo-download")]
        public async Task<IActionResult> RelatorioProdutosMaisVendidosDownload(int filtro)
        {
            var data = await _abacosRepository.BuscaRelatorioEstoqueMinimo(filtro);

            string conteudo_csv = "Código Externo; Descrição; DISP_CONNECT; VIRTUAL_00K; \n \t";

            foreach (var item in data)
            {
                conteudo_csv += $"{item.DK.ToString()}; {item.DESCRICAO.ToString()}; {item.DISP_CONNECT.ToString()}; {item.VIRTUAL_00K.ToString()}";
                conteudo_csv += "\n \t";
            }

            if (!Directory.Exists(_csv))
            {
                Directory.CreateDirectory(_csv);
            }

            string arquivo = _csv + "produtos_estoque_minimo.csv";
            FileInfo pendentesVendasCsv = new FileInfo(arquivo);

            if (pendentesVendasCsv.Exists)
            {
                using (StreamWriter sw = new StreamWriter(arquivo, false, Encoding.Unicode))
                {
                    sw.WriteLine(conteudo_csv);
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(arquivo, false, Encoding.Unicode))
                {
                    sw.WriteLine(conteudo_csv);
                }
            }

            Response.Headers.Add("Content-type", "application/octet-stream");
            Response.Headers.Add("Content-Disposition", "attachment; Filename="
                + arquivo);

            byte[] input = Encoding.Unicode.GetBytes(conteudo_csv);
            return File(input, System.Net.Mime.MediaTypeNames.Application.Octet, arquivo);
        }


        [HttpGet]
        [Route("relatorio-produtos-mais-vendidos")]
        public async Task<IActionResult> RelatorioProdutosMaisVendidos(int top, string dataInicial, string dataFinal)
        {
            var dados = await _abacosRepository.BuscaRelatorioProdutosMaisVendidos(top, dataInicial, dataFinal);

            return Ok(new { dados = dados.ToList()});
        }

        [HttpGet]
        [Route("relatorio-produtos-mais-vendidos-download")]
        public async Task<IActionResult> RelatorioProdutosMaisVendidosDownload(int top, string dataInicial, string dataFinal)
        {
            var data = await _abacosRepository.BuscaRelatorioProdutosMaisVendidos(top, dataInicial, dataFinal);

            string conteudo_csv = "PROS_COD; PROS_NOM; TOTAL; \n \t";

            foreach (var item in data)
            {
                conteudo_csv += $"{item.PROS_COD.ToString()}; {item.PROS_NOM.ToString()}; {item.total_pedidos.ToString()}";
                conteudo_csv += "\n \t";
            }

            if (!Directory.Exists(_csv))
            {
                Directory.CreateDirectory(_csv);
            }

            string arquivo = _csv + "produtos_mais_vendidos.csv";
            FileInfo pendentesVendasCsv = new FileInfo(arquivo);

            if (pendentesVendasCsv.Exists)
            {
                using (StreamWriter sw = new StreamWriter(arquivo, false, Encoding.Unicode))
                {
                    sw.WriteLine(conteudo_csv);
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(arquivo, false, Encoding.Unicode))
                {
                    sw.WriteLine(conteudo_csv);
                }
            }

            Response.Headers.Add("Content-type", "application/octet-stream");
            Response.Headers.Add("Content-Disposition", "attachment; Filename="
                + arquivo);

            byte[] input = Encoding.Unicode.GetBytes(conteudo_csv);
            return File(input, System.Net.Mime.MediaTypeNames.Application.Octet, arquivo);
        }

    }
}
