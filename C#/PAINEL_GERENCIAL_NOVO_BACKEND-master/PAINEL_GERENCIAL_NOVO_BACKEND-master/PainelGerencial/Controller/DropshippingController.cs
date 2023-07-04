using Microsoft.AspNetCore.Mvc;
using PainelGerencial.Domain;
using PainelGerencial.Repository;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class DropshippingController: ControllerBase
    {
        private const string codigoGlb = "19D0CF31-213F-415C-B196-0D3529EC5CB5";
        private readonly IDropshippingRepository _repository;
        
        public DropshippingController(IDropshippingRepository repository)
        {
            _repository = repository;
        }


        // Altera o pedido de Pessoa jurídica(CNPJ) para Pessoa Física(CPF)
        [HttpPost]
        [Route("AlteraPedidoCnpjCpf")]
        public IActionResult AlteraPedidoCnpjCpf(string pedidoCodigoExterno)
        {
            int resultado = _repository.AlteraPedidoCnpjCpf(pedidoCodigoExterno);

            if (resultado == 0)
            {
                return BadRequest(new
                {
                    status = "Ocorreu um erro ao processar o pedido."
                });
            }

            return Ok(new
            {
                status = "Pedido alterado com sucesso. "
            });
        }

        // Consulta Pedido Dropshipping
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Pedido(string codigoExterno)
        {
            var pedido = await _repository.PedidoDropshipping(codigoExterno);

            return Ok(pedido);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult NotaFiscal(string codigoExterno)
        {
            var notafiscal = _repository.NotaFiscal(codigoExterno);

            return Ok(notafiscal);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> PedidoFornecedores(string codigoExterno)
        {
            var pedidoFornecedor = await _repository.PedidoFornecedores(codigoExterno);

            return Ok(pedidoFornecedor);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult PedidoFornecedoresAlteraStatus(IList<string> codigoExterno, int status)
        {
            int resultado = 0;
            foreach(var item in codigoExterno)
            {
                resultado = _repository.PedidoFornecedoresAlteraStatus(item, status);
            }
            

            if (resultado == 0)
            {
                return BadRequest(new 
                {
                    status = "Pedido não encontrado."
                });
            }

            return Ok(new
            {
                status = "Status do Pedido alterado com sucesso"
            });
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult PedidosFaturadosSemAndamento()
        {
            List<dynamic> pedidosFaturados = (List<dynamic>)_repository.PedidosFaturadosSemAndamento();

            return Ok(new
            {
                quantidade = pedidosFaturados.Count,
                pedidos = pedidosFaturados
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> IntegraPedidoGLBEtapa1(IList<string> pedidos, string token)
        {
            var naoGlobal = pedidos.Where(x => !x.Contains("-GLB") 
                && !x.Contains("-DTS") 
                && !x.Contains("-LOC")).ToList();

            if (naoGlobal.Any())
            {
                return BadRequest(new
                {
                    status = "Pedido não Global"
                });
            }

            const string url = "http://api.dakotaparts.com.br:59690/";
            var client = new RestClient(url);
            var request = new RestRequest(resource: "TesteJobs/EnviarPedidoParaFornecedorDropLista", method: Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", token);
            request.AddJsonBody(pedidos);
            RestResponse response = await client.ExecuteAsync(request);

            return Ok(response.Content);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> IntegraPedidoGLBEtapa2_3(IList<string> pedidos, string token)
        {
            var naoGlobal = pedidos.Where(x => !x.Contains("-GLB")
                && !x.Contains("-DTS")
                && !x.Contains("-LOC")).ToList();

            if (naoGlobal.Any())
            {
                return BadRequest(new
                {
                    status = "Pedido não Global"
                });
            }

            const string url = "http://api.dakotaparts.com.br:59690/TesteJobs/VerificarFaturamentoNoAbacosDosPedidosGLBLista";
            var client = new RestClient(url);
            var request = new RestRequest(resource: "", method: Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", token);
            request.AddJsonBody(pedidos);
            RestResponse response = await client.ExecuteAsync(request);
            
            return Ok(response.Content);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> IntegraPedidoGLBEtapa4(IList<string> pedidos, string token)
        {
            var naoGlobal = pedidos.Where(x => !x.Contains("-GLB")
                && !x.Contains("-DTS")
                && !x.Contains("-LOC")).ToList();

            if (naoGlobal.Any())
            {
                return BadRequest(new
                {
                    status = "Pedido não Global"
                });
            }

            const string url = "http://api.dakotaparts.com.br:59690/";
            var client = new RestClient(url);
            var request = new RestRequest(resource: "TesteJobs/EnviarNotaFiscalListaParaFornecedor", method: Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", token);
            request.AddJsonBody(pedidos);
            RestResponse response = await client.ExecuteAsync(request);

            return Ok(response.Content);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> IntegrarPedidoGLB(IList<string> pedidos, string token)
        {
            var naoGlobal = pedidos.Where(x => !x.Contains("-GLB")
                && !x.Contains("-DTS")
                && !x.Contains("-LOC")).ToList();

            if (naoGlobal.Any())
            {
                return BadRequest(new 
                {
                    status = "Pedido não Global"
                });
            }

            foreach(string codigoExterno in pedidos)
            {
                var pedidoFornecedor = await _repository.PedidoFornecedores(codigoExterno);
                List<string> lista = new List<string>();
                lista.Add(codigoExterno);

                IActionResult retorno;
                if (pedidoFornecedor.FirstOrDefault().pedidoStatus.Codigo == 1)
                {
                    retorno = await this.IntegraPedidoGLBEtapa1(lista, token);
                    pedidoFornecedor = await _repository.PedidoFornecedores(codigoExterno);
                }

                if (pedidoFornecedor.FirstOrDefault().pedidoStatus.Codigo == 2 ||
                    pedidoFornecedor.FirstOrDefault().pedidoStatus.Codigo == 3)
                {
                    retorno = await this.IntegraPedidoGLBEtapa2_3(lista, token);
                    pedidoFornecedor = await _repository.PedidoFornecedores(codigoExterno);
                }

                if (pedidoFornecedor.FirstOrDefault().pedidoStatus.Codigo == 4)
                {
                    retorno = await this.IntegraPedidoGLBEtapa4(lista, token);
                    pedidoFornecedor = await _repository.PedidoFornecedores(codigoExterno);
                }
            }

            return Ok(new
            {
                status = "Pedido faturado."
            });
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult VoltaStatusPedidosSemNotaFiscal()
        {
            var pedidos = _repository.PedidosSemNotaFiscal();

            int quantidade = _repository.VoltaStatusPedidosSemNotaFiscal();

            return Ok(new
            {
                quantidade = quantidade,
                pedidos = pedidos,
            });
        }

        [HttpPut]
        [Route("[action]")]
        public IActionResult InserePedidoDropshippingGlb(string pedidoCodigoExterno)
        {
            var pedidos = new List<string>();
            pedidos.Add(pedidoCodigoExterno);

            var naoGlobal = pedidos.Where(x => !x.Contains("-GLB")
                && !x.Contains("-DTS")
                && !x.Contains("-LOC")).ToList();
            {
                return BadRequest(new 
                {
                    status = "Pedido não GLOBAL"
                });
            }

            Pedido_Model pedido = new Pedido_Model();
            pedido.CodigoExterno = pedidoCodigoExterno.Substring(0, pedidoCodigoExterno.IndexOf("-"));
            pedido.PedidoStatusCodigo = 1;
            pedido.PedidoConnect = 0;
            pedido.DataAtualizacaoRegistro = DateTime.Now;
            pedido.DataCriacaoRegistro = DateTime.Now;

            pedido.pedidoFornecedores = new PedidoFornecedor_Model
            {
                PedidoFornecedorCodigo = pedidoCodigoExterno,
                PedidoCodigo = pedido.CodigoExterno,
                FornecedorCodigo = new Guid(codigoGlb),
                PedidoStatusCodigo = 1,
                DataAtualizacaoRegistro = DateTime.Now,
                DataCriacaoRegistro = DateTime.Now,
            };

            if (!_repository.ExistePedido(pedido.CodigoExterno))
            {
                _repository.InserePedido(pedido);
            }
            
            if (!_repository.ExistePedidoFornecedores(pedidoCodigoExterno))
            {
                _repository.InserePedidoFornecedores(pedido.pedidoFornecedores);
            }
            
            _repository.InserePedidoFornecedorProdutos(pedidoCodigoExterno);

            return Ok(pedido);
        }
    }
}
