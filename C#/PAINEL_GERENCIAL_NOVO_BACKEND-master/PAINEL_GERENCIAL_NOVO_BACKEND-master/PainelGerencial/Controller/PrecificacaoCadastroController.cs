using AbacosWSERP;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PainelGerencial.Domain.Fenix;
using PainelGerencial.Domain.PrecificacaoCadastro;
using PainelGerencial.Domain.ShoppingDePrecos;
using PainelGerencial.Helpers;
using PainelGerencial.Repository;
using PainelGerencial.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Method = RestSharp.Method;

namespace PainelGerencial.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class PrecificacaoCadastroController : ControllerBase
    {
        private readonly IPrecificacaoCadastroRepository _precificacaoCadastroRepository;
        private readonly IAbacosRepository _abacosRepository;
        private readonly IAbacosServicos _abacosServicos;
        private readonly ControleAcessoController _controleAcesso;
        private readonly IFenixServicos _fenixSvc;
        private readonly ISolicitacaoTemporariaServicos _solicitacaotemporariaServico;
        private const string _url = "http://api.dakotaparts.com.br/PrecificacaoCadastro";
        private const string _urlFenix = "http://api.dakotaparts.com.br:53236/";

        public PrecificacaoCadastroController(
            IPrecificacaoCadastroRepository precificacaoCadastroRepository, 
            IAbacosRepository abacosRepository,
            ControleAcessoController controleAcesso,
            IAbacosServicos abacosServicos,
            IFenixServicos fenixServicos,
            ISolicitacaoTemporariaServicos solicitacaoTemporariaServico)
        {
            _precificacaoCadastroRepository = precificacaoCadastroRepository;
            _abacosRepository = abacosRepository;
            _controleAcesso = controleAcesso;
            _abacosServicos = abacosServicos;
            _fenixSvc = fenixServicos;
            _solicitacaotemporariaServico = solicitacaoTemporariaServico;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> SolicitacaoKitPendente(string produtoCodigoExterno)
        {
            var precificacao = await _precificacaoCadastroRepository.SolicitacaoKitPendente(produtoCodigoExterno);

            return Ok(new
            {
                kits = precificacao
            });
        }

        [HttpPost]
        [Route("[action]")]
        public DadosPreco[] PrepararDadosSolicitacaoKit(SolicitacoesKit_Model solicitacao)
        {
            var dadosPreco = new DadosPreco
            {
                CodigoProduto = solicitacao.ProdutoCodigoExterno,
                CodigoProdutoPai = solicitacao.ProdutoCodigoExternoPai,
                NomeLista = _abacosRepository.ConverterListaPrecos(solicitacao.ProdutoListaPrecoCodigo),
                PrecoTabela = Convert.ToDouble(solicitacao.SolicitacoesKitComponentes.Sum(x => x.ProdutoPrecoTabelaUnitarioNovo * x.Quantidade)),
                PrecoPromocional = Convert.ToDouble(solicitacao.SolicitacoesKitComponentes.Sum(x => x.ProdutoPrecoPromocionalUnitarioNovo)),
                DataInicioPromocao = "01012017",
                DataTerminoPromocao = "01012050"
            };
            return new[] { dadosPreco };
        }

        [HttpGet]
        [Route("[action]")]
        private DadosPreco[] PrepararDadosSolicitacao(Solicitacao_Model solicitacao)
        {
            var dadosPreco = new DadosPreco
            {
                CodigoProduto = solicitacao.ProdutoCodigoExterno,
                CodigoProdutoPai = solicitacao.ProdutoCodigoExternoPai,
                NomeLista = _abacosRepository.ConverterListaPrecos(solicitacao.ProdutoListaPrecoCodigo),
                PrecoTabela = Convert.ToDouble(solicitacao.ProdutoPrecoTabelaNovo),
                PrecoPromocional = Convert.ToDouble(solicitacao.ProdutoPrecoPromocionalNovo),
                DataInicioPromocao = "01012017",
                DataTerminoPromocao = "01012050"
            };
            return new[] { dadosPreco };
        }

        [HttpPost]
        [Route("[action]")]
        public string RegistrarAvaliacao(AvaliacaoSolicitacao_Model avaliacao)
        {
            const string recurso = "/Solicitacao/RegistrarAvaliacao";

            var client = new RestClient(_url);
            var request = new RestRequest(recurso, Method.Put);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            string token = _controleAcesso.retornaToken().Token.ToString();
            request.AddHeader("Authorization", $"bearer {token}");
            request.AddBody(avaliacao);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            return response.Content;
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult FinalizarSolicitacao(Solicitacao_Model solicitacao)
        {
            var avaliacao = new AvaliacaoSolicitacao_Model
            {
                Codigo = solicitacao.Codigo,
                AprovadorResponsavelEmail = solicitacao.AprovadorResponsavelEmail,
                Observacao = solicitacao.Observacao,
                ProdutoNomeReduzidoNovo = solicitacao.ProdutoNomeReduzidoNovo,
                SolicitacaoStatusCodigo = (int)SolicitacoesStatusCodigos.Finalizado
            };

            RegistrarAvaliacao(avaliacao);

            return Ok();
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<dynamic> ListarSolicitacoesPreco(int pagina = 1, int deslocamento = 50)
        {
            var solicitacoes = await _precificacaoCadastroRepository.ListarSolicitacoes(
                ordenarPor: "",
                pagina: pagina,
                deslocamento: deslocamento,
                produtoCodigoExternoPai: null,
                solicitacaoTipoCodigo: (int)SolicitacoesTiposCodigos.Preco,
                solicitacaoStatusCodigo: (int)SolicitacoesStatusCodigos.Aprovado,
                produtoListaPrecoCodigo: null,
                produtoGrupoCodigo: null,
                produtoGrupoCodigos: "",
                somentePrecosReduzidos: false,
                solicitanteResponsavelEmail: "",
                aprovadorResponsavelEmail: "",
                dataInicial: DateTime.MinValue,
                dataFinal: DateTime.Now,
                somentePais: false);

            return new { 
                resultados = solicitacoes.Count(), 
                pagina = pagina,
                solicitacoes = solicitacoes 
            };
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AtualizarPrecos()
        {
            int atualizados = 0;
            int naoAtualizados = 0;
            int kits = 0;

            var tempo = new Stopwatch();
            tempo.Start();

            try
            {
                var solicitacoes = await _precificacaoCadastroRepository.ListarSolicitacoes(
                    ordenarPor: "",
                    pagina: 1,
                    deslocamento: 1000,
                    produtoCodigoExternoPai: null,
                    solicitacaoTipoCodigo: (int)SolicitacoesTiposCodigos.Preco,
                    solicitacaoStatusCodigo: (int)SolicitacoesStatusCodigos.Aprovado,
                    produtoListaPrecoCodigo: null,
                    produtoGrupoCodigo: null,
                    produtoGrupoCodigos: "",
                    somentePrecosReduzidos: false,
                    solicitanteResponsavelEmail: "",
                    aprovadorResponsavelEmail: "",
                    dataInicial: DateTime.MinValue,
                    dataFinal: DateTime.Now,
                    somentePais: false);

                foreach (var solicitacao in solicitacoes)
                {
                    var possuiKits = PossuiKits(solicitacao.ProdutoCodigoExterno);
                    //if (!possuiKits)
                    {
                        var dadosProdutoAtualizar = PrepararDadosSolicitacao(solicitacao);
                        var retorno = await _abacosServicos.EnviarWsdlAbacos(dadosProdutoAtualizar);
                        if (!retorno)
                        {
                            naoAtualizados++;
                            continue;
                        }
                        atualizados++;
                    }
                    //else
                    //{
                    //    kits++;
                    //}

                    FinalizarSolicitacao(solicitacao);
                }

                tempo.Stop();
            }
            catch(Exception ex)
            {
                return Ok(new
                {
                    tempoDecorrido = tempo.Elapsed,
                    mensagem = ex.Message
                });
            }
            
            return Ok(new { 
                Atualizados = atualizados,
                NaoAtualizados = naoAtualizados,
                kits = kits,
                tempoDecorrido = tempo.Elapsed,
            });
        }

        [HttpGet]
        [Route("[action]")]
        public bool PossuiKits(string ProdutoCodigoExterno)
        {
            
            const string recurso = "Produto/Listar";

            var client = new RestClient(_urlFenix);
            var request = new RestRequest(recurso, Method.Get);
            request.AddHeader("Accept", "application/json");
            string token = _controleAcesso.retornaToken().Token.ToString();
            request.AddHeader("Authorization", $"bearer {token}");

            request.AddParameter("somenteAtivos", true);
            request.AddParameter("somenteKits", true);
            request.AddParameter("CodigosExternos", ProdutoCodigoExterno);

            RestResponse response = client.Execute(request);
            
            var resultado = JsonConvert.DeserializeObject<IEnumerable<Solicitacao_Model>>(response.Content);

            return resultado.Any();
        }

        [HttpGet]
        [Route("[action]")]
        public Task<IEnumerable<SolicitacoesKit_Model>> ListarSolicitacoesKit(
            string? produtoCodigoExternoPai,
            int? produtoListaPrecoCodigo,
            int? produtoGrupoCodigo,
            string? solicitanteResponsavelEmail,
            string? aprovadorResponsavelEmail,
            int pagina = 1,
            int deslocamento = 50)
        {
            var lista = _precificacaoCadastroRepository.ListarSolicitacoesKit(
                produtoCodigoExternoPai,
                produtoListaPrecoCodigo,
                produtoGrupoCodigo,
                solicitanteResponsavelEmail,
                aprovadorResponsavelEmail,
                pagina,
                deslocamento);

            return lista;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AtualizarPrecosKit()
        {
            int atualizados = 0;
            int naoAtualizados = 0;
            var tempo = new Stopwatch();
            tempo.Start();

            var solicitacoes = await _precificacaoCadastroRepository.ListarSolicitacoesKit(
                pagina: 1,
                deslocamento: 10000,
                produtoCodigoExternoPai: null,
                produtoListaPrecoCodigo: null,
                produtoGrupoCodigo: null,
                solicitanteResponsavelEmail: null,
                aprovadorResponsavelEmail: null);

            foreach (var solicitacao in solicitacoes)
            {
                var dadosProdutoAtualizar = PrepararDadosSolicitacaoKit(solicitacao);
                var retorno = await _abacosServicos.EnviarWsdlAbacos(dadosProdutoAtualizar);
                if (!retorno)
                {
                    naoAtualizados++;
                    continue;
                }

                var componenteAbacos = PrepararDadosSolicitacaoKitComponente(solicitacao);
                var teste = await _abacosServicos.AlterarPrecoComponenteKit(componenteAbacos);
                int atualizou = _precificacaoCadastroRepository.FinalizaSolicitacaoKit(solicitacao.Codigo);
                if (!teste && atualizou > 0)
                {
                    naoAtualizados++;
                    continue;
                }
                atualizados++;
            }
            tempo.Stop();

            return Ok(new
            {
                Atualizados = atualizados,
                NaoAtualizados = naoAtualizados,
                tempoDecorrido = tempo.Elapsed,
            });
        }

        [HttpGet]
        [Route("[action]")]
        public DadosItemKit[] PrepararDadosSolicitacaoKitComponente(SolicitacoesKit_Model solicitacoes)
        {
            List<DadosItemKit> dadosPreco = new List<DadosItemKit>();
            foreach (SolicitacoesKitComponentes_Model componente in solicitacoes.SolicitacoesKitComponentes)
            {
                var precoItem = new DadosItemKit
                {
                    CodigoProduto = componente.ProdutoCodigoExterno,
                    CodigoProdutoKit = solicitacoes.ProdutoCodigoExterno,
                    ListaPreco = _abacosRepository.ConverterListaPrecos(solicitacoes.ProdutoListaPrecoCodigo),
                    ValorItem = componente.ProdutoPrecoTabelaUnitarioNovo.Value,
                    ValorItemPromocional = componente.ProdutoPrecoPromocionalUnitarioNovo.Value,
                };
                dadosPreco.Add(precoItem);
            }
            
            return dadosPreco.ToArray();
        }

        [HttpPost]
        [Route("Solicitacao/[action]")]
        public async Task<IActionResult> GerarSolicitacoesTemporarias([FromBody]RequisicaoSolicitacaoComKit requisicao)
        {
            if (requisicao.email == null)
            {
                return BadRequest(new { mensagem = "O email não pode ser nulo " });
            }
            _precificacaoCadastroRepository.GerarSolicitacoesTemporarias(requisicao.codigoPai, requisicao.email, requisicao.tipoKit, requisicao.tabela[0]);
            
            var solitacoes = await _precificacaoCadastroRepository.ListaSolicitacoesTemporarias(requisicao.email);

            return Ok(solitacoes);
        }

        [HttpGet]
        [Route("Solicitacao/[action]")]
        public async Task<IEnumerable<SolicitacaoTemporaria_Model>> ListaSolicitacoesTemporarias(string emailSolicitante)
        {
            var solitacoes = (List<SolicitacaoTemporaria_Model>)await _precificacaoCadastroRepository.ListaSolicitacoesTemporarias(emailSolicitante);

            return solitacoes;
        }

        [HttpPost]
        [Route("Solicitacao/[action]")]
        public async Task<IActionResult> SalvarAlteracaoTemporaria(List<SolicitacaoTemporaria_Model> solicitacoes)
        {
            foreach(var solicitacao in solicitacoes)
            {
                _solicitacaotemporariaServico.SalvarAlteracaoTemporaria(solicitacao);
            }

            return Ok(new
            {
                mensagem = "Solicitação temporária atualizada."
            });
        }

        [HttpPost]
        [Route("Solicitacao/[action]")]
        public async Task<IActionResult> CancelarERemoverPorUsuario(string email)
        {
            int atualizado = await _precificacaoCadastroRepository.CancelarERemoverPorUsuario(email);

            return Ok( atualizado = atualizado );
        }

        [HttpPost]
        [Route("Solicitacao/[action]")]
        public async Task<IActionResult> ImportarTemporarios(string email, int aplicarPrecoVendavel)
        {
            try
            {
                if (email == null || email == "")
                {
                    return BadRequest(new
                    {
                        mensagem = "Email inválido."
                    });
                }

                if (await _solicitacaotemporariaServico.VerificaPrecoZerado(email))
                {
                    return BadRequest(new
                    {
                        mensagem = "Não é possível precificar com valor zerado"
                    });
                }

                _solicitacaotemporariaServico.ConfirmarSolicitacoesTemporarias(email, aplicarPrecoVendavel);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("Solicitacao/[action]")]
        public async Task<IActionResult> ListaPrecos()
        {
            var lista = await _precificacaoCadastroRepository.ListarListaPrecos();

            return Ok(new { quantidade = lista.Count(), dados = lista });
        }

        [HttpGet]
        [Route("Solicitacao/[action]")]
        public async Task<IActionResult> ListaLojas()
        {
            var lista = await _precificacaoCadastroRepository.ListarLojas();

            return Ok(new { quantidade = lista.Count(), dados = lista });
        }

        [HttpPost]
        [Route("Solicitacao/[action]")]
        public async Task<IActionResult> AtualizaListaPreco([FromBody]IList<ListaPrecos_Model> listaPrecos)
        {
            int qtd = 0;
            foreach(var lista in listaPrecos)
            {
                qtd += await _precificacaoCadastroRepository.AtualizaListaPreco(lista);
            }
            
            return Ok(new { quantidade = qtd });
        }

        [HttpGet]
        [Route("Solicitacao/[action]")]
        public async Task<IActionResult> SolicitacoesListar(string Email, string CodigoExternoPai, DateTime? DataInicio, DateTime? DataFim, int? Plataforma, int pagina = 1)
        {
            var precificacoes = await _precificacaoCadastroRepository.Precificacoes(Email, CodigoExternoPai, DataInicio, DataFim, Plataforma, pagina);

            return Ok(new { 
                quantidade = precificacoes?.Count(),
                pagina = pagina,
                total = precificacoes.Any() ? precificacoes.FirstOrDefault().Total : 0,
                dados = precificacoes 
            });
        }

        [HttpPost]
        [Route("Solicitacao/[action]")]
        public async Task<IActionResult> AvaliarPreco([FromBody] ParametrosAvaliarPreco_Model parametros)
        {
            int avaliacoes = 0;
            foreach(var Codigo in parametros.Codigos)
            {
                // Localiza a Precificacao
                var _precos = await _precificacaoCadastroRepository.PrecificacaoPorId(Codigo);
                
                // Se estiver aprovado, atualiza no Abacos
                // Inicio
                bool retorno = false;
                if (parametros.Aprovado)
                {
                    var _dadosPreco = PrecificacaoCadastroHelper.PrepararDadosSolicitacao(_precos.FirstOrDefault());
                    PrecificacaoCadastroHelper.EnviarWsdlAbacos(_dadosPreco);
                }
                // Fim

                avaliacoes += await _precificacaoCadastroRepository.AvaliarPreco(Codigo, parametros.Aprovado, parametros.EmailAprovador);
            };

            var precificacoes = await _precificacaoCadastroRepository.Precificacoes(null, "", null, null, null, 1);

            return Ok(new
            {
                quantidade = precificacoes?.Count(),
                pagina = 1,
                total = precificacoes.Any() ? precificacoes.FirstOrDefault().Total : 0,
                dados = precificacoes
            });
        }

        [HttpGet]
        [Route("Solicitacao/[action]")]
        public async Task<IActionResult> RelatorioPrecificacoes(
            string Email,
            string CodigoExternoPai,
            string? CodigoExterno,
            DateTime? DataInicio,
            DateTime? DataFim,
            int? Plataforma,
            bool? aprovado,
            bool? pendente,
            int pagina = 1)
        {
            var dados = await _precificacaoCadastroRepository.RelatorioPrecificacoes(
                Email,
                CodigoExternoPai,
                CodigoExterno,
                DataInicio,
                DataFim,
                Plataforma,
                aprovado,
                pendente,
                pagina);

            if (dados.Any())
            {
                return Ok(new
                {
                    quantidade = dados.Count(),
                    pagina = pagina,
                    total = dados.FirstOrDefault().Total(),
                    dados = dados
                });
            }
            else
            {
                return NoContent();
            }
        }

    }
}
