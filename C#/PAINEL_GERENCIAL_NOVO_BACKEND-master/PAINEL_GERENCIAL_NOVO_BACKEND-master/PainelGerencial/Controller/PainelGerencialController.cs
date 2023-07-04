using AbacosWSERP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
using PainelGerencial.Domain;
using PainelGerencial.Repository;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ubiety.Dns.Core;

namespace PainelGerencial.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PainelGerencialController : ControllerBase
    {
        private readonly IPainelGerencialRepository _painelGerencialRepository;
        private readonly INovosNegociosRepository _novosNegociosRepository;
        private const string _csv = @"\\192.168.0.179\inetpub\wwwroot\PainelGerencialAPI\download\csv\";

        public PainelGerencialController(
            IPainelGerencialRepository painelGerencialRepository,
            INovosNegociosRepository novosNegociosRepository)
        {
            _painelGerencialRepository = painelGerencialRepository;
            _novosNegociosRepository = novosNegociosRepository;
        }

        [HttpGet]
        [Route("faturamento/pre-analise")]
        public IActionResult PreAnalise()
        {
            try
            {
                var dados = _painelGerencialRepository.PreAnalise();
                var PedidoMaisAntigo = _painelGerencialRepository.PedidoMaisAntigoPreAnalise();
                var dataAntigaList = PedidoMaisAntigo.Select(c => { c.data_antiga.ToString(); return c; }).FirstOrDefault();

                dados.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    c.pedido_mais_antigo = dataAntigaList.data_antiga;
                    return c;
                }).ToList();

                return Ok(dados.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/a-faturar")]
        [Route("operacao/nao-bipados")]
        public IActionResult AFaturar()
        {
            try
            {
                var data = _painelGerencialRepository.AFaturar();
                var pedidoMaisAntigo = _painelGerencialRepository.PedidoMaisAntigoFaturamentoAFaturar();
                var dataAntigaList = pedidoMaisAntigo.Select(c => { c.data = c.data.ToString(); return c; }).FirstOrDefault();

                data.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    c.pedido_mais_antigo = dataAntigaList.data;
                    return c;
                }).ToList();

                return Ok(JsonConvert.SerializeObject(data.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/faturados")]
        public IActionResult Faturados()
        {
            try
            {
                var data = _painelGerencialRepository.Faturados();
                data.Select(c =>
                {
                    c.quantidade = c.quantidade?.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    c.pedido_mais_antigo = c.pedido_mais_antigo?.ToString("dd/MM/yyyy");
                    return c;
                }).ToList();

                return Ok(JsonConvert.SerializeObject(data.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/pendentes-vendas")]
        [Route("operacao/pendentes")]
        public object PendentesVendas()
        {
            try
            {
                var data = _painelGerencialRepository.PendentesVendas();

                var produtos = new
                {
                    quantidade = data.Count(),
                    produtos = data.Sum(x => (double?)x.valor_produtos),
                    ticket_medio = data.Average(x => (double?)x.valor_produtos),
                    data_mais_antiga = data.Min(x => x.PEDS_DAT_CAD),
                };

                var normais = from item in data
                              where item.dropshipping == "NORMAL"
                              group item by item.dropshipping into grupo
                              select new
                              {
                                  quantidade = grupo.GroupBy(x => x.PEDS_EXT_COD).Count(),
                                  produtos = grupo.Sum(x => (double?)x.valor_produtos),
                                  vendas = grupo.DistinctBy(x => x.PEDS_EXT_COD).Sum(x => (double?)x.valor_vendas),
                                  ticket_medio = grupo.DistinctBy(x => x.PEDS_EXT_COD).Average(x => (double?)x.valor_vendas),
                                  data_mais_antiga = grupo.Min(x => x.PEDS_DAT_CAD),
                              };

                var drop = from item in data
                           where item.dropshipping == "DROP"
                           group item by item.dropshipping into grupo
                           select new
                           {
                               quantidade = grupo.GroupBy(x => x.PEDS_EXT_COD).Count(),
                               produtos= grupo.Sum(x => (double?)x.valor_produtos),
                               vendas = grupo.DistinctBy(x => x.PEDS_EXT_COD).Sum(x => (double?)x.valor_vendas),
                               ticket_medio = grupo.DistinctBy(x => x.PEDS_EXT_COD).Average(x => (double?)x.valor_vendas),
                               data_mais_antiga= grupo.Min(x => x.PEDS_DAT_CAD),
                           };

                var cross = from item in data
                            where item.dropshipping == "CROSS"
                            group item by item.dropshipping into grupo
                            select new
                            {
                                quantidade = grupo.GroupBy(x => x.PEDS_EXT_COD).Count(),
                                produtos = grupo.Sum(x => (double?)x.valor_produtos),
                                vendas = grupo.DistinctBy(x => x.PEDS_EXT_COD).Sum(x => (double?)x.valor_vendas),
                                ticket_medio = grupo.DistinctBy(x => x.PEDS_EXT_COD).Average(x => (double?)x.valor_vendas),
                                data_mais_antiga = grupo.Min(x => x.PEDS_DAT_CAD),
                            };

                var pendencia_total = data.Count();
                var pendencia_data_mais_antiga = data.Min(x => x.PEDS_DAT_CAD);

                var resultado = new
                {
                    // Pendencias
                    pendencia_total = pendencia_total == 0 ? "0" : pendencia_total.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR")),
                    pendencia_data_mais_antiga = pendencia_data_mais_antiga == null ? null : pendencia_data_mais_antiga.ToString("dd/MM/yyyy"),
                    // Produtos
                    quantidade_produtos = produtos == null ? "0" : produtos.quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR")),
                    produtos_produtos = produtos == null ? "0" : produtos.produtos?.ToString("C"),
                    ticket_medio_produtos = produtos == null ? "0" : produtos.ticket_medio?.ToString("C"),
                    data_mais_antiga_produtos = produtos == null ? "0" : produtos.data_mais_antiga.ToString("dd/MM/yyyy"),
                    // Normal
                    quantidade_normal = normais.FirstOrDefault() == null ? "0" : normais.FirstOrDefault().quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR")),
                    produtos_normal = normais.FirstOrDefault() == null ? "0" : normais.FirstOrDefault().produtos == null ? 0.ToString("C") : normais.FirstOrDefault().produtos?.ToString("C"),
                    vendas_normal = normais.FirstOrDefault() == null || normais.FirstOrDefault().vendas == null ? 0.ToString("C") : normais.FirstOrDefault().vendas?.ToString("C"),
                    ticket_medio_normal = normais.FirstOrDefault() == null || normais.FirstOrDefault().ticket_medio == null ? 0.ToString("C") : normais.FirstOrDefault().ticket_medio?.ToString("C"),
                    data_mais_antiga_normal = normais.FirstOrDefault() == null ? null : normais.FirstOrDefault().data_mais_antiga.ToString("dd/MM/yyyy"),
                    // Dropshipping
                    quantidade_drop = drop.FirstOrDefault() == null ? "0" : drop.FirstOrDefault().quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR")),
                    produtos_drop = drop.FirstOrDefault() == null ? "0" : drop.FirstOrDefault().produtos == null ? 0.ToString("C") : drop.FirstOrDefault().produtos?.ToString("C"),
                    vendas_drop = drop.FirstOrDefault() == null || drop.FirstOrDefault().vendas == null ? 0.ToString("C") : drop.FirstOrDefault().vendas?.ToString("C"),
                    ticket_medio_drop = drop.FirstOrDefault() == null || drop.FirstOrDefault().ticket_medio == null ? 0.ToString("C") : drop.FirstOrDefault().ticket_medio?.ToString("C"),
                    pedido_mais_antigo_drop = drop.FirstOrDefault() == null ? null : drop.FirstOrDefault().data_mais_antiga.ToString("dd/MM/yyyy"),
                    // Crossdocking
                    quantidade_cross = cross.FirstOrDefault().quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR")),
                    produtos_cross = cross.FirstOrDefault().produtos == null ? 0.ToString("C") : cross.FirstOrDefault().produtos?.ToString("C"),
                    vendas_cross = cross.FirstOrDefault().vendas == null ? 0.ToString("C") : cross.FirstOrDefault().vendas?.ToString("C"),
                    ticket_medio_cross = cross.FirstOrDefault().ticket_medio == null ? 0.ToString("C") : cross.FirstOrDefault().ticket_medio?.ToString("C"),
                    pedido_mais_antigo_cross = cross.FirstOrDefault() == null ? null : cross.FirstOrDefault().data_mais_antiga.ToString("dd/MM/yyyy"),
            };


                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/pendentes-vendas-download")]
        public IActionResult PendentesVendasDownload(string tipoPendencia)
        {
            var data = _painelGerencialRepository.PendentesVendas();
            var pendencias = data;

            if (tipoPendencia == "normal" || tipoPendencia == "drop" || tipoPendencia == "cross")
            {
                pendencias = data.Where(item => item.dropshipping == tipoPendencia.ToUpper())
                                .DistinctBy(item => item.PEDS_EXT_COD)
                                .ToList();
            }

            string conteudo_csv = "CodigoExterno; CodigoInterno; CodigoItem; CodigoProdutoExterno; TotalProdutos; TotalPedido; ValorEncargos; TipoVenda; DataCadastro; Marca; \n \t";

            foreach (var item in pendencias)
            {
                conteudo_csv += $"{item.PEDS_EXT_COD.ToString()}; {item.PEDS_COD.ToString()}; {item.ITEP_COD.ToString()}; {item.PROS_EXT_COD.ToString()}; {item.valor_produtos.ToString()}; {item.valor_vendas.ToString()}; {item.CPLP_VAL_ENCFIX.ToString()}; {item.dropshipping.ToString()}; {item.PEDS_DAT_CAD.ToString()}; {item.MARP_NOM.ToString()}";
                conteudo_csv += "\n \t";
            }

            if (!Directory.Exists(_csv))
            {
                Directory.CreateDirectory(_csv);
            }

            string arquivo = $"pendentes_vendas_{tipoPendencia}.csv";
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
        [Route("faturamento/faturamento-hoje")]
        public IActionResult FaturamentoHoje()
        {
            try
            {
                var data = _painelGerencialRepository.FaturamentoHoje();
                data.Select(c =>
                {
                    c.quantidade = c.quantidade?.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.val_mercadorias = c.val_mercadorias == null ? 0.ToString("C") : c.val_mercadorias.ToString("C");
                    c.valor_frete = c.valor_frete == null ? 0.ToString("C") : c.valor_frete.ToString("C");
                    c.valor_encargos = c.valor_encargos == null ? 0.ToString("C") : c.valor_encargos.ToString("C");
                    c.custo_liquido_mercadorias = c.custo_liquido_mercadorias == null ? 0.ToString("C") : c.custo_liquido_mercadorias.ToString("C");
                    c.custo_frete = c.custo_frete == null ? 0.ToString("C") : c.custo_frete.ToString("C");
                    c.valor_margem = c.valor_margem == null ? 0.ToString("C") : c.valor_margem.ToString("C");
                    c.percentual_margem = c.percentual_margem == null ? 0.ToString() : c.percentual_margem.ToString();
                    c.valor_icms = c.valor_icms == null ? 0.ToString("C") : c.valor_icms.ToString("C");
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    c.pedido_mais_antigo = c.pedido_mais_antigo?.ToString("dd/MM/yyyy");
                    return c;
                }).ToList();

                return Ok(data.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/faturamento-ontem")]
        public IActionResult FaturamentoOntem()
        {
            try
            {
                var data = _painelGerencialRepository.FaturamentoOntem();

                data.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    return c;
                }).ToList();

                return Ok(data.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/faturamento-ultimos-7-dias")]
        public IActionResult FaturamentoUltimos7Dias()
        {
            try
            {
                var data = _painelGerencialRepository.FaturamentoUltimos7Dias();

                var FaturamentoHoje = _painelGerencialRepository.FaturamentoHoje();

                var quantidadeHJ = FaturamentoHoje.Select(x => x.quantidade).FirstOrDefault();
                var novovalor = data.Select(x => x.quantidade).FirstOrDefault() + quantidadeHJ;

                var valorTotalHj = FaturamentoHoje.Select(x => x.valor_total == null ? 0 : x.valor_total).FirstOrDefault();
                var valorTotal = data.Select(x => x.valor_total).FirstOrDefault() + valorTotalHj;

                var ticketMedioHj = FaturamentoHoje.Select(x => x.ticket_medio == null ? 0 : x.ticket_medio).FirstOrDefault();
                var ticketMedio = (data.Select(x => x.ticket_medio == null ? 0 : x.ticket_medio).FirstOrDefault() + ticketMedioHj) / 2;

                data.Select(c =>
                {
                    c.quantidade = novovalor.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.val_mercadorias = c.val_mercadorias == null ? 0.ToString("C") : c.val_mercadorias.ToString("C");
                    c.valor_frete = c.valor_frete == null ? 0.ToString("C") : c.valor_frete.ToString("C");
                    c.valor_encargos = c.valor_encargos == null ? 0.ToString("C") : c.valor_encargos.ToString("C");
                    c.valor_total = (valorTotal).ToString("C");
                    c.custo_liquido_mercadorias = c.custo_liquido_mercadorias == null ? 0.ToString("C") : c.custo_liquido_mercadorias.ToString("C");
                    c.valor_icms = c.valor_icms == null ? 0.ToString("C") : c.valor_icms.ToString("C");
                    c.custo_frete = c.custo_frete = c.custo_frete == null ? 0.ToString("C") : c.custo_frete.ToString("C");
                    c.valor_margem = c.valor_margem == null ? 0.ToString("C") : c.valor_margem.ToString("C");
                    c.percentual_margem = c.percentual_margem.ToString("N2", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.ticket_medio = ticketMedio.ToString("C");
                    return c;
                }).ToList();

                return Ok(data.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/faturamento-mes")]
        public IActionResult FaturamentoMes()
        {
            try
            {
                var data = _painelGerencialRepository.FaturamentoMes();

                var FaturamentoHoje = _painelGerencialRepository.FaturamentoHoje();

                var quantidadeHJ = FaturamentoHoje.Select(x => x.quantidade).FirstOrDefault();
                var novovalor = int.Parse(data.Select(x => x.quantidade).FirstOrDefault()) + quantidadeHJ;


                var valorTotalHj = FaturamentoHoje.Select(x => x.valor_total == null ? 0 : x.valor_total).FirstOrDefault();
                var valorTotal = data.Select(x => x.valor_total == null ? 0 : x.valor_total).FirstOrDefault() + valorTotalHj;

                var ticketMediolHj = FaturamentoHoje.Select(x => x.ticket_medio == null ? 0 : x.ticket_medio).FirstOrDefault();
                var ticketMedio = (data.Select(x => x.ticket_medio == null ? 0 : x.ticket_medio).FirstOrDefault() + ticketMediolHj) / 2;

                data.Select(c =>
                {
                    c.quantidade = novovalor.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.val_mercadorias = c.val_mercadorias == null ? 0.ToString("C") : c.val_mercadorias.ToString("C");
                    c.valor_frete = c.valor_frete == null ? 0.ToString("C") : c.valor_frete.ToString("C");
                    c.valor_encargos = c.valor_encargos == null ? 0.ToString("C") : c.valor_encargos.ToString("C");
                    c.valor_total = valorTotal.ToString("C");
                    c.custo_liquido_mercadorias = c.custo_liquido_mercadorias == null ? 0.ToString("C") : c.custo_liquido_mercadorias.ToString("C");
                    c.valor_icms = c.valor_icms == null ? 0.ToString("C") : c.valor_icms.ToString("C");
                    c.custo_frete = c.custo_frete == null ? 0.ToString("C") : c.custo_frete.ToString("C");
                    c.valor_margem = c.valor_margem == null ? 0.ToString("C") : c.valor_margem.ToString("C");
                    c.percentual_margem = c.percentual_margem;
                    c.ticket_medio = ticketMedio.ToString("C");
                    return c;
                }).ToList();


                return Ok(data.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/pedidos-a-processar-pela-API")]
        public IActionResult PedidosAProcessarPelaAPI()
        {
            try
            {
                var dados = _painelGerencialRepository.PedidosAProcessarPelaAPI();
                dados.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    return c;
                }).ToList();

                if (dados.Count() == 0)
                {
                    return Ok(new Painel_Model_quantidade());
                }



                return Ok(dados.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }


        [HttpGet]
        [Route("faturamento/pedidos-a-corrigir-estoque")]
        public IActionResult PedidosACorrigirEstoque()
        {
            try
            {
                var data = _painelGerencialRepository.PedidosACorrigirEstoque();
                List<dynamic> pedidoMaisAntigo = (List<dynamic>)_painelGerencialRepository.PedidoMaisAntigoACorrigirEstoque();

                data.Select(c =>
                {
                    c.quantidade = c.quantidade?.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    c.pedido_mais_antigo = pedidoMaisAntigo.Count == 0 ? "" : pedidoMaisAntigo.FirstOrDefault().data;
                    return c;
                }).ToList();

                return Ok(data.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        /*
        [HttpGet]
        [Route("faturamento-hora")]
        public IActionResult FaturamentoPorHora()
        {
            try
            {
                var data = _painelGerencialRepository.FaturamentoPorHora();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("pedidos-aguardando-separacao")]
        public IActionResult PedidosAguardandoSeparacao()
        {
            try
            {
                var data = _painelGerencialRepository.PedidosAguardandoSeparacao();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }
        

        [HttpGet]
        [Route("perfil-dos-pedidos-faturados")]
        public IActionResult PerfilDosPedidosFaturados()
        {
            try
            {
                var data = _painelGerencialRepository.PerfilDosPedidosFaturados();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("perfil-dos-pedidos-a-faturar")]
        public IActionResult PerfilDosPedidosAFaturar()
        {
            try
            {
                var data = _painelGerencialRepository.PerfilDosPedidosAFaturar();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }
        */

        [HttpGet]
        [Route("faturamento/pendentes-outras-saidas")]
        public IActionResult PendentesOutrasSaidas()
        {
            try
            {
                var data = _painelGerencialRepository.PendentesOutrasSaidas();
                data.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    return c;
                }).ToList();

                return Ok(data.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/pendentes-devolucoes")]
        public IActionResult PendentesDevolucoes()
        {
            try
            {
                var data = _painelGerencialRepository.PendentesDevolucoes();
                data.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.valor_total = (c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C"));
                    c.ticket_medio = (c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C"));
                    return c;
                }).ToList();

                return Ok(data.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }


        [HttpPost]
        [Route("usuarios/login")]
        public IActionResult UsuariosLogin()
        {
            try
            {
                var request = HttpContext.Request;
                var stream = new StreamReader(request.Body);
                var body = stream.ReadToEndAsync();

                var json = body.Result;

                var jsonConvert = JsonConvert.DeserializeObject<Login>(json);

                var email = jsonConvert.email;
                var senha = jsonConvert.senha;
                var dados = _painelGerencialRepository.UsuariosLogin(email, senha);

                var usuario = new Sessao()
                {
                    erros = dados.ToList().Count == 0,
                    sessao = dados.ToList().Count > 0,
                    dados = (dados.Count() > 0 ? dados.FirstOrDefault() : ""),
                    mensagem = (dados.Count() > 0
                        ? "Seja bem-vindo ao Painel Gerencial"
                        : ((email.Trim().Length == 0 || senha.Trim().Length == 0)
                            ? "Por favor, insira o seu e-mail<br />Por favor, insira a senha"
                            : "E-mail e/ou Senha Incorretos!")
                    )
                };


                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpPost]
        [Route("usuarios/logout/{id}")]
        public IActionResult UsuariosLogout(int id)
        {
            try
            {
                var logout = _painelGerencialRepository.UsuariosLogout(id);

                var usuario = new Sessao()
                {
                    mensagem = (logout ? "Logout realizado com Sucesso!" : "Erro ao fazer logout!")
                };
                return Ok(usuario);

            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet, HttpPost]
        [Route("usuarios/verifica-sessao/{id}")]
        public IActionResult UsuariosVerificaSessao(int id)
        {
            try
            {
                var ip = HttpContext.Request.Headers["X-Forwarded-For"];
                var isSessao = _painelGerencialRepository.UsuariosVerificaSessao(id, ip);

                var usuario = new Sessao()
                {
                    mensagem = (isSessao ? "Bem vindo!" : "Acesso não autorizado")
                };

                if (isSessao)
                {
                    return Ok(usuario);
                }
                else
                {
                    return Unauthorized(usuario);
                }


            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("usuarios/listar")]
        public IActionResult UsuariosListar()
        {
            try
            {
                var dados = _painelGerencialRepository.UsuariosListar();
                var usuario_Model = new Usuario_Model();
                usuario_Model.relatorio = dados.ToList();
                usuario_Model.quantidade_registros = dados.LongCount();

                return Ok(usuario_Model);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("usuarios/dados/{id}")]
        public IActionResult UsuariosDados(int id)
        {
            try
            {
                var dados = _painelGerencialRepository.UsuariosDados(id);

                dados.Select(c =>
                {
                    c.id = c.id.ToString();
                    c.nome = c.nome;
                    c.email = c.email;
                    c.id_setor = c.id_setor.ToString();
                    c.id_grupo = c.id_grupo.ToString();
                    c.ativo = c.ativo;
                    return c;
                }).ToList();

                return Ok(dados.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("usuarios/grupos")]
        public IActionResult UsuariosGrupos()
        {
            try
            {
                var dados = _painelGerencialRepository.UsuariosGrupos();
                dados.Select(c => { c.id = c.id.ToString(); c.nome = c.nome; return c; }).ToList();

                var grupo = new Grupo();
                grupo.grupos = dados.ToList();

                return Ok(grupo);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("usuarios/setores")]
        public IActionResult UsuariosSetores()
        {
            try
            {
                var dados = _painelGerencialRepository.UsuariosSetores();
                dados.Select(c => { c.id = c.id.ToString(); c.nome = c.nome; return c; }).ToList();

                var setor = new Setor();
                setor.setores = dados.ToList();

                return Ok(setor);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpPost]
        [Route("usuarios/cadastrar")]
        public IActionResult UsuariosCadastrar(Usuario usuario)
        {
            try
            {
                var listaErros = new List<string>();


                if (usuario.nome == null || usuario.nome.Length == 0)
                {
                    listaErros.Add("Por favor, insira o nome");
                }

                if (usuario.id == null)
                {
                    if (usuario.senha == null || usuario.senha.Length == 0)
                    {
                        listaErros.Add("Por favor, insira a senha");
                    }

                    // Usuario existe
                    List<Usuario> existe = (List<Usuario>)_painelGerencialRepository.UsuarioPorEmail(usuario.email);
                    if (existe.Any())
                    {
                        listaErros.Add("Usuário já existe.");
                    }
                }

                var retorno = new Retorno();



                if (listaErros.Count == 0)
                {
                    var gravou = _painelGerencialRepository.UsuariosCadastrar(usuario);
                    if (gravou > 0)
                    {
                        retorno.erros = false;
                        retorno.mensagem = "Usuário cadastrado com Sucesso";
                    }
                }
                else
                {
                    retorno.erros = true;
                    retorno.mensagem = string.Join("<br />", listaErros);
                }

                return Ok(retorno);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("usuarios/excluir/{id}")]
        public IActionResult UsuariosExcluir(int id)
        {
            try
            {
                _painelGerencialRepository.UsuariosExcluir(id);

                return Ok(new
                {
                    mensagem = "Usuário excluido com Sucesso"
                });
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }


        [HttpGet]
        [Route("operacao/faturamento-por-hora")]
        public IActionResult FaturamentoPorHora()
        {
            try
            {
                var dados = _painelGerencialRepository.FaturamentoPorHora();
                dados.Select(c => { c.valor = c.valor; return c; });
                var acumulado = 0;

                // Percorre a lista e atribui os valores
                var listaRelatorio = new ListaRelatorio();
                listaRelatorio.relatorio = new List<Relatorio_Model>();
                foreach (var item in dados)
                {
                    var relatorio_Model = new Relatorio_Model();
                    relatorio_Model.hora = item.hora.ToString() + " horas";
                    relatorio_Model.valor = item.valor;
                    relatorio_Model.quantidade = item.quantidade;
                    acumulado += item.quantidade;
                    relatorio_Model.acumulado = acumulado;

                    listaRelatorio.relatorio.Add(relatorio_Model);
                }


                return Ok(listaRelatorio);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }


        [HttpGet]
        [Route("faturamento/despachados")]
        public IActionResult FaturamentoDespachados()
        {
            try
            {
                var dados = _painelGerencialRepository.FaturamentoDespachados();

                dados.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    return c;
                }).ToList();

                return Ok(dados.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }


        [HttpGet]
        [Route("faturamento/nao-despachados")]
        public IActionResult FaturamentoNaoDespachados()
        {
            try
            {
                var dados = _painelGerencialRepository.FaturamentoNaoDespachados();

                var totais = from item in dados
                             select new
                             {
                                 quantidade = dados.Count().ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR")),
                                 valor_total = dados.Sum(x => (double?)x.valor_total)?.ToString("C"),
                                 ticket_medio = dados.Average(x => (double?)x.valor_total)?.ToString("C"),
                             };

                //totais.Select(c =>
                //{
                //    c.quantidade = c.quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                //    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                //    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                //    return c;
                //}).ToList();

                return Ok(totais.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/nao-despachados-download")]
        public IActionResult FaturamentoNaoDespachadosDownload()
        {
            var data = _painelGerencialRepository.FaturamentoNaoDespachados();

            string conteudo_csv = "TotalPedido; CodigoExterno; " +
                "CodigoInterno; ValorEncargos; " +
                "DataCadastro; UnidadeNegocio; \n \t";

            foreach (var item in data)
            {
                conteudo_csv += $"{item.valor_total.ToString("C")}; {item.PEDS_EXT_COD.ToString()}; " +
                    $"{item.PEDS_COD.ToString()}; {item.CPLP_VAL_ENCFIX.ToString("C")}; " +
                    $"{item.PEDS_DAT_CAD.ToString()}; {item.UNIN_COD.ToString()}";
                conteudo_csv += "\n \t";
            }

            if (!Directory.Exists(_csv))
            {
                Directory.CreateDirectory(_csv);
            }

            string arquivo = _csv + "nao_despachados.csv";
            FileInfo naoDespachadosCsv = new FileInfo(arquivo);

            if (naoDespachadosCsv.Exists)
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
        [Route("operacao/grafico/pedidos-aguardando-separacao")]
        public IActionResult PedidosAguardandoSeparacao()
        {
            try
            {
                var dados = _painelGerencialRepository.PedidosAguardandoSeparacao();

                var pedidos_Aguardando_Separacao = new Pedidos_Aguardando_Separacao();
                foreach (var item in dados)
                {
                    if (item.status.Equals("aguardando_separacao"))
                    {
                        pedidos_Aguardando_Separacao.aguardando_separacao = item.quantidade.ToString();
                    }

                    if (item.status.Equals("em_separacao"))
                    {
                        pedidos_Aguardando_Separacao.em_separacao = item.quantidade.ToString();
                    }

                }
                var pedidos_Aguardando_Separacao_Model = new Pedidos_Aguardando_Separacao_Model();
                pedidos_Aguardando_Separacao_Model.pedidos_aguardando_separacao = pedidos_Aguardando_Separacao;

                return Ok(pedidos_Aguardando_Separacao_Model);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("operacao/grafico/perfil-dos-pedidos-faturados")]
        public IActionResult PerfilDosPedidosFaturados()
        {
            try
            {
                var dados = _painelGerencialRepository.PerfilDosPedidosFaturados();

                var mono = 0;
                var duplo = 0;
                var multiplo = 0;

                foreach (var item in dados)
                {
                    var linhas = item.linhas;

                    if (linhas == 1)
                    {
                        mono++;
                    }
                    else if (linhas == 2)
                    {
                        duplo++;
                    }
                    else
                    {
                        multiplo++;
                    }
                }

                var total = mono + duplo + multiplo;

                Perfil_Dos_Pedidos_Faturados perfil_Dos_Pedidos_Faturados = new Perfil_Dos_Pedidos_Faturados();
                perfil_Dos_Pedidos_Faturados.quantidade_mono = mono;
                perfil_Dos_Pedidos_Faturados.quantidade_duplo = duplo;
                perfil_Dos_Pedidos_Faturados.quantidade_multiplo = multiplo;
                perfil_Dos_Pedidos_Faturados.quantidade_total = total;
                perfil_Dos_Pedidos_Faturados.mono = perfil_Dos_Pedidos_Faturados.Porcentagem_Grafico(mono, total);
                perfil_Dos_Pedidos_Faturados.duplo = perfil_Dos_Pedidos_Faturados.Porcentagem_Grafico(duplo, total);
                perfil_Dos_Pedidos_Faturados.multiplo = perfil_Dos_Pedidos_Faturados.Porcentagem_Grafico(multiplo, total);

                var perfil_Dos_Pedidos_Faturados_Model = new Perfil_Dos_Pedidos_Faturados_Model();
                perfil_Dos_Pedidos_Faturados_Model.perfil_dos_pedidos_faturados = perfil_Dos_Pedidos_Faturados;



                return Ok(perfil_Dos_Pedidos_Faturados_Model);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("operacao/grafico/perfil-dos-pedidos-a-faturar")]
        public IActionResult PerfilDosPedidosAFaturar()
        {
            try
            {
                var dados = _painelGerencialRepository.PerfilDosPedidosAFaturar();

                var mono = 0;
                var duplo = 0;
                var multiplo = 0;

                foreach (var item in dados)
                {
                    var linhas = item.linhas;

                    if (linhas == 1)
                    {
                        mono++;
                    }
                    else if (linhas == 2)
                    {
                        duplo++;
                    }
                    else
                    {
                        multiplo++;
                    }
                }

                var total = mono + duplo + multiplo;

                Perfil_Dos_Pedidos_A_Faturar perfil_Dos_Pedidos_A_Faturar = new Perfil_Dos_Pedidos_A_Faturar();
                perfil_Dos_Pedidos_A_Faturar.quantidade_mono = mono;
                perfil_Dos_Pedidos_A_Faturar.quantidade_duplo = duplo;
                perfil_Dos_Pedidos_A_Faturar.quantidade_multiplo = multiplo;
                perfil_Dos_Pedidos_A_Faturar.quantidade_total = total;
                perfil_Dos_Pedidos_A_Faturar.mono = Perfil_Dos_Pedidos_A_Faturar.Porcentagem_Grafico(mono, total);
                perfil_Dos_Pedidos_A_Faturar.duplo = Perfil_Dos_Pedidos_A_Faturar.Porcentagem_Grafico(duplo, total);
                perfil_Dos_Pedidos_A_Faturar.multiplo = Perfil_Dos_Pedidos_A_Faturar.Porcentagem_Grafico(multiplo, total);

                var perfil_Dos_Pedidos_A_Faturar_Model = new Perfil_Dos_Pedidos_A_Faturar_Model();
                perfil_Dos_Pedidos_A_Faturar_Model.perfil_dos_pedidos_a_faturar = perfil_Dos_Pedidos_A_Faturar;



                return Ok(perfil_Dos_Pedidos_A_Faturar_Model);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }


        [HttpGet]
        [Route("captacao")]
        public IActionResult Captacao()
        {
            try
            {
                var dados_lv = _painelGerencialRepository.CaptacaoLojaVirtualIntegracaoVtex();
                var dados_mp = _painelGerencialRepository.CaptacaoMarketPlaceIntegracaoVtex();
                var dados_mpc = _painelGerencialRepository.CaptacaoMarketPlaceConnectIntegracaoVtex();
                var dados_ml = _painelGerencialRepository.CaptacaoMercadoLivreIntegracaoVtex();
                var pedidos_digitados = _painelGerencialRepository.CaptacaoPedidosDigitadosIntegracaoVtex();


                var ticket_medio_total =
                    ((double)dados_lv.FirstOrDefault().valor_total
                    + (double)dados_mp.FirstOrDefault().valor_total
                    + (double)dados_mpc.FirstOrDefault().valor_total
                    + (double)dados_ml.FirstOrDefault().valor_total)
                    /
                    ((double)dados_lv.FirstOrDefault().quantidade
                    + (double)dados_mp.FirstOrDefault().quantidade
                    + (double)dados_mpc.FirstOrDefault().quantidade
                    + (double)dados_ml.FirstOrDefault().quantidade);

                var porcentagem_lv = Perfil_Dos_Pedidos_A_Faturar
                    .Porcentagem_Grafico((decimal)dados_lv.FirstOrDefault().valor_total,
                                         (decimal)dados_lv.FirstOrDefault().valor_total
                                         + (decimal)dados_mp.FirstOrDefault().valor_total
                                         + (decimal)dados_mpc.FirstOrDefault().valor_total
                                         + (decimal)dados_ml.FirstOrDefault().valor_total);
                var porcentagem_mp = Perfil_Dos_Pedidos_A_Faturar
                    .Porcentagem_Grafico(dados_mp.FirstOrDefault().valor_total,
                                         (decimal)dados_lv.FirstOrDefault().valor_total
                                         + (decimal)dados_mp.FirstOrDefault().valor_total
                                         + (decimal)dados_mpc.FirstOrDefault().valor_total
                                         + (decimal)dados_ml.FirstOrDefault().valor_total);
                var porcentagem_mpc = Perfil_Dos_Pedidos_A_Faturar
                    .Porcentagem_Grafico((decimal)dados_mpc.FirstOrDefault().valor_total,
                                         (decimal)dados_lv.FirstOrDefault().valor_total
                                         + (decimal)dados_mp.FirstOrDefault().valor_total
                                         + (decimal)dados_mpc.FirstOrDefault().valor_total
                                         + (decimal)dados_ml.FirstOrDefault().valor_total);
                var porcentagem_ml = Perfil_Dos_Pedidos_A_Faturar
                    .Porcentagem_Grafico((decimal)dados_ml.FirstOrDefault().valor_total,
                                         (decimal)dados_lv.FirstOrDefault().valor_total
                                         + (decimal)dados_mp.FirstOrDefault().valor_total
                                         + (decimal)dados_mpc.FirstOrDefault().valor_total
                                         + (decimal)dados_ml.FirstOrDefault().valor_total);

                Captacao_Model captacao_Model = new Captacao_Model();
                captacao_Model.total = new Total
                {
                    quantidade = (
                        (double)dados_lv.FirstOrDefault().quantidade +
                        (double)dados_mp.FirstOrDefault().quantidade +
                        (double)dados_mpc.FirstOrDefault().quantidade +
                        (double)dados_ml.FirstOrDefault().quantidade
                    ).ToString("N0"),
                    valor_total = ((double)dados_lv.FirstOrDefault().valor_total +
                                   (double)dados_mp.FirstOrDefault().valor_total +
                                   (double)dados_mpc.FirstOrDefault().valor_total +
                                   (double)dados_ml.FirstOrDefault().valor_total).ToString("C"),
                    ticket_medio = ticket_medio_total.ToString("C"),
                    porcentagem = "100.0"
                };

                captacao_Model.loja_virtual = dados_lv.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString();
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    c.porcentagem = porcentagem_lv;
                    return c;
                }).FirstOrDefault();
                captacao_Model.market_place = dados_mp.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString();
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    c.porcentagem = porcentagem_mp;
                    return c;
                }).FirstOrDefault();
                captacao_Model.market_place_connect = dados_mpc.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString();
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    c.porcentagem = porcentagem_mpc;
                    return c;
                }).FirstOrDefault();
                captacao_Model.mercado_livre = dados_ml.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString();
                    c.valor_total = c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C");
                    c.ticket_medio = c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C");
                    c.porcentagem = porcentagem_ml;
                    return c;
                }).FirstOrDefault();
                captacao_Model.pedidos_digitados = pedidos_digitados.Select(c =>
                {
                    c.quantidade = c.quantidade.ToString();
                    c.valor_total = (c.valor_total == null ? 0.ToString("C") : c.valor_total.ToString("C"));
                    c.ticket_medio = (c.ticket_medio == null ? 0.ToString("C") : c.ticket_medio.ToString("C"));
                    return c;
                }).FirstOrDefault();




                return Ok(captacao_Model);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("faturamento/credito-aprovado")]
        public IActionResult FaturamentoCreditoAprovado()
        {
            try
            {
                var dados = _painelGerencialRepository.FaturamentoCreditoAprovado();

                dados.Select(c =>
                {
                    c.credito_aprovado = c.quantidade.ToString("N0", CultureInfo.CreateSpecificCulture("pt-BR"));
                    return c;
                }).ToList();

                return Ok(dados.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpPost]
        [Route("inserir-cotacao")]
        public IActionResult InserirCotacao(Cotacao_Model cotacao)
        {
            try
            {
                var dados = _painelGerencialRepository.AbacosInsereCotacao(cotacao);

                return Ok(dados);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("pronto-para-integrar-00k")]

        public IActionResult PedidosProntoParaIntegar00k()
        {
            DateTime dataHj = DateTime.Now;

            var client = new RestClient("http://api.00k.srv.br/");
            var request = new RestRequest("orders", Method.Get);

            request.AddHeader("Authorization", "eyJzdG9yZUlkIjogIjk5Iiwic3RvcmVOYW1lc3BhY2UiOiAiY29ubmVjdF9wYXJ0cyIsInN0b3JlRGF0YWJhc2UiOiAibG9qYTk5In0=");
            request.AddParameter("erp_sent", "false");
            request.AddParameter("status", "accepted_payment");
            request.AddParameter("with_pendency", "false");
            request.AddParameter("start", $"{dataHj.AddDays(-30).ToString("yyyy-MM-dd 00:00")}", ParameterType.QueryString);
            request.AddParameter("end", $"{dataHj.ToString("yyyy-MM-dd 23:59")}", ParameterType.QueryString);
            request.AddParameter("page", 1);

            RestResponse response = client.Execute(request);

            int TotalPedidos = JsonConvert.DeserializeObject<dynamic>(response.Content).total;
            if (TotalPedidos <= 50)
            {
                return Ok(new { total = TotalPedidos, cor = "#43A047" });
            }
            else if (TotalPedidos > 50 && TotalPedidos <= 100)
            {
                return Ok(new { total = TotalPedidos, cor = "#FFB300" });
            }
            else
            {
                return Ok(new { total = TotalPedidos, cor = "#D32F2F" });
            }
        }

        [HttpGet]
        [Route("pendencias-00k")]

        public IActionResult PedidosPendencias00k()
        {
            DateTime dataHj = DateTime.Now;

            var client = new RestClient("http://api.00k.srv.br/");
            var request = new RestRequest("orders", Method.Get);

            request.AddHeader("Authorization", "eyJzdG9yZUlkIjogIjk5Iiwic3RvcmVOYW1lc3BhY2UiOiAiY29ubmVjdF9wYXJ0cyIsInN0b3JlRGF0YWJhc2UiOiAibG9qYTk5In0=");
            request.AddParameter("erp_sent", "false");
            request.AddParameter("status", "awaiting_payment");
            request.AddParameter("with_pendency", "true");
            request.AddParameter("start", $"{dataHj.AddDays(-30).ToString("yyyy-MM-dd 00:00")}", ParameterType.QueryString);
            request.AddParameter("end", $"{dataHj.ToString("yyyy-MM-dd 23:59")}", ParameterType.QueryString);
            request.AddParameter("page", 1);

            RestResponse response = client.Execute(request);

            int TotalPedidos = JsonConvert.DeserializeObject<dynamic>(response.Content).total;
            
            if (TotalPedidos <= 30)
            {
                return Ok(new { total = TotalPedidos, cor = "#43A047" });
            }
            else if (TotalPedidos > 30 && TotalPedidos <= 50)
            {
                return Ok(new { total = TotalPedidos, cor = "#FFB300" });
            }
            else
            {
                return Ok(new { total = TotalPedidos, cor = "#D32F2F" });
            }
        }

        [HttpGet]
        [Route("pronto-para-manuseio-vtex")]

        public IActionResult PedidosProntoParaManuseioVtex()
        {
            string VtexAppToken = "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB";
            string VtexAppKey = "vtexappkey-connectparts-GOFEYM";

            var client = new RestClient("http://connectparts.vtexcommercestable.com.br/");
            var request = new RestRequest("api/oms/pvt/orders/", Method.Get);

            request.AddHeader("x-vtex-api-appToken", VtexAppToken);
            request.AddHeader("x-vtex-api-appKey", VtexAppKey);
            request.AddParameter("orderBy", "creationDate,asc");
            request.AddParameter("f_status", "ready-for-handling");
            request.AddParameter("per_page", 1);
            request.AddParameter("page", 1);

            RestResponse response = client.Execute(request);

            int TotalPedidos = JsonConvert.DeserializeObject<dynamic>(response.Content).paging.total;

            if (TotalPedidos <= 25)
            {
                return Ok(new { total = TotalPedidos, cor = "#43A047" });
            }
            else if (TotalPedidos > 25 && TotalPedidos <= 50)
            {
                return Ok(new { total = TotalPedidos, cor = "#FFB300" });
            }
            else
            {
                return Ok(new { total = TotalPedidos, cor = "#D32F2F" });
            }
        }

        [HttpGet]
        [Route("api-consulting")]

        public IActionResult PedidosAPIConsulting()
        {
            var dados = _painelGerencialRepository.PedidosAPIConsulting().Result.FirstOrDefault();

            int quantidade = dados != null ? dados.quantidade : 0;
            var dataMaisAntiga = dados?.DtMaisAntiga != null ? dados.DtMaisAntiga.ToString("dd/MM/yy HH:mm") : "Não Existe";
            var dataMaisNova = dados?.DtMaisNova != null ? dados.DtMaisNova.ToString("dd/MM/yy HH:mm") : "Não Existe";
            var dataUltimaReplicacao = dados?.DtUltimaReplicacao != null ? dados.DtUltimaReplicacao.ToString("dd/MM/yy HH:mm") : "Não Existe";
            
            if (quantidade <= 25)
            {
                return Ok(new { quantidade = quantidade, dataMaisAntiga = dataMaisAntiga, dataMaisNova = dataMaisNova, dataUltimaReplicacao = dataUltimaReplicacao, cor = "#43A047" });
            }
            else if (quantidade > 25 && quantidade <= 50)
            {
                return Ok(new { quantidade = quantidade, dataMaisAntiga = dataMaisAntiga, dataMaisNova = dataMaisNova, dataUltimaReplicacao = dataUltimaReplicacao, cor = "#FFB300" });
            }
            else
            {
                return Ok(new { quantidade = quantidade, dataMaisAntiga = dataMaisAntiga, dataMaisNova = dataMaisNova, dataUltimaReplicacao = dataUltimaReplicacao, cor = "#D32F2F" });
            }
        }

        [HttpGet]
        [Route("cubos")]

        public IActionResult Cubos()
        {
            try 
            {
                var dados = _painelGerencialRepository.Cubos().Result.FirstOrDefault();
                var status = dados.Status;

                if (status == "Failed")
                {
                    return Ok(new { status = "Falha", cor = "#D32F2F" });
                }
                else
                {
                    return Ok(new { status = "Executado", cor = "#43A047"});
                }
            }
            catch(Exception ex)
            {
                return Ok(new { status = "Falha de Atualização", cor = "#FFB300" });
            }
        }

        [HttpGet]
        [Route("replicacao-data-warehouse")]

        public IActionResult ReplicacaoDataWarehouse()
        {
            try 
            {
                var dados = _painelGerencialRepository.ReplicacaoDataWarehouse().Result.FirstOrDefault();
                var status = dados.Status;

                if (status == "Failed")
                {
                    return Ok(new { status = "Falha", cor = "#D32F2F" });
                }
                else
                {
                    return Ok(new { status = "Executado", cor = "#43A047" });
                }
            }
            catch(Exception ex)
            {
                return Ok(new { status = "Falha de Atualização", cor = "#FFB300" });
            }
        }

        [HttpGet]
        [Route("envio-intelipost")]

        public IActionResult EnvioIntelipost()
        {
            try 
            {
                var dados = _painelGerencialRepository.EnvioIntelipost().Result.FirstOrDefault();
                DateTime dataAgora = DateTime.Now;
                DateTime dataNula = new DateTime(0001, 01, 01);
                DateTime dataInicio = Convert.ToDateTime(dados.INICIO);
                DateTime dataFim = Convert.ToDateTime(dados.FIM);

                if ((dataInicio.AddMinutes(1440) < dataAgora) && dataFim.Year == dataNula.Year)
                {
                    return Ok(new { status = "Falha", cor = "#D32F2F" });
                }
                else if (dataInicio < dataFim || dataInicio == dataFim)
                {
                    return Ok(new { status = "Executado", cor = "#43A047" });
                }
                else
                {
                    return Ok(new { status = "Processando", cor = "#1976D2" });
                }
            }
            catch(Exception ex)
            {
                return Ok(new { status = "Falha de Atualização", cor = "#FFB300", error = ex});
            }
        }

        [HttpGet]
        [Route("integracao-00k")]

        public IActionResult Integracao00k()
        {
            try 
            {
                var dados = _painelGerencialRepository.Integracao00k().Result;

                var totalIntegradosHoje = dados.Count();
                var dataComparacao = DateTime.Now;
                var dataUltimaIntegracao = dados.FirstOrDefault().DataCadastro;

                if (dataUltimaIntegracao.AddMinutes(20) < dataComparacao)
                {
                    return Ok(new { status = "Offline | ", total = totalIntegradosHoje+" | ", data = dataUltimaIntegracao.ToString("dd/MM HH:mm"), cor = "#B71C1C" });
                }
                else if (dataUltimaIntegracao.AddMinutes(20 / 2) < dataComparacao)
                {
                    return Ok(new { status = "Online | ", total = totalIntegradosHoje + " | ", data = dataUltimaIntegracao.ToString("dd/MM HH:mm"), cor = "#FFB300" });
                }
                else
                {
                    return Ok(new { status = "Online | ", total = totalIntegradosHoje+" | ", data = dataUltimaIntegracao.ToString("dd/MM HH:mm"), cor = "#43A047" });
                }
            }
            catch(Exception ex)
            {
                return Ok(new { status = "Falha de Atualização", cor = "#FFB300" });
            }
        }

        [HttpGet]
        [Route("integracao-vtex")]

        public IActionResult IntegracaoVtex()
        {
            try 
            {
                var dados = _painelGerencialRepository.IntegracaoVtex().Result;
                var totalIntegradosHoje = dados.Count();
                var dataComparacao = DateTime.Now;
                var dataUltimaIntegracao = dados.FirstOrDefault().DataCadastro;

                if (dataUltimaIntegracao.AddMinutes(10) < dataComparacao)
                {
                    return Ok(new { status = "Offline | ", total = totalIntegradosHoje + " | ", data = dataUltimaIntegracao.ToString("dd/MM HH:mm"), cor = "#D32F2F" });
                }
                else if (dataUltimaIntegracao.AddMinutes(10 / 2 ) < dataComparacao)
                {
                    return Ok(new { status = "Online | ", total = totalIntegradosHoje + " | ", data = dataUltimaIntegracao.ToString("dd/MM HH:mm"), cor = "#FFB300" });
                }
                else
                {
                    return Ok(new { status = "Online | ", total = totalIntegradosHoje + " | ", data = dataUltimaIntegracao.ToString("dd/MM HH:mm"), cor = "#43A047" });
                }
                
            }
            catch(Exception ex)
            {
                return Ok(new { status = "Falha de Atualização", cor = "#FFB300" });
            }
        }

        [HttpGet]
        [Route("pedidos-integrados-por-hora-via-00k")]

        public IActionResult PedidosIntegradosPorHoraVia00k()
        {
            var dados = _painelGerencialRepository.PedidosIntegradosPorHoraVia00k().Result;

            return Ok(new { dados = dados });
        }

        [HttpGet]
        [Route("pedidos-integrados-por-hora-via-vtex")]

        public IActionResult PedidosIntegradosPorHoraViaVtex()
        {
            var dados = _painelGerencialRepository.PedidosIntegradosPorHoraViaVtex().Result;

            return Ok(new { dados = dados });
        }

        [HttpGet]
        [Route("relatorio-pedidos-por-periodo")]
        public async Task<IActionResult> RelatorioPedidosPorPeriodo(DateTime dataInicial, DateTime dataFinal, int deslocamento, int limite)
        {
            if (dataInicial == DateTime.MinValue || dataFinal == DateTime.MinValue)
            {
                return BadRequest(new { msg = "Selecione a data inicial e final" });
            }
            var dados = (await _painelGerencialRepository.RelatorioPedidosPorPeriodo(dataInicial, dataFinal, deslocamento, limite));

            return Ok(new { dados = dados });
        }

        [HttpGet]
        [Route("relatorio-pedidos-por-periodo-download")]
        public IActionResult RelatorioPedidosPorPeriodoDownload(DateTime dataInicial, DateTime dataFinal, int deslocamento, int limite)
        {
            var dados = _painelGerencialRepository.RelatorioPedidosPorPeriodo(dataInicial, dataFinal, deslocamento, limite).Result;

            string conteudo_csv = "Status; Data da Compra; Codigo Interno; Codigo Externo; Valor do Pedido; Data do Faturamento \n \t";

            foreach (var item in dados)
            {
                var Data_Faturamento = item.Data_Faturamento == null ? "Não faturado" : item.Data_Faturamento.ToString();
                conteudo_csv += $"{item.Status.ToString()}; {item.Data_Compra.ToString()}; {item.Cod_Interno.ToString()}; {item.Cod_Interno.ToString()}; {item.Valor_Pedido.ToString()}; {Data_Faturamento}";
                conteudo_csv += "\n \t";
            }

            if (!Directory.Exists(_csv))
            {   
                Directory.CreateDirectory(_csv);
            }

            string arquivo = "relatorio_pedidos_por_periodo.csv";
            FileInfo relatorioPedidosPorPeriodoCsv = new FileInfo(arquivo);

            if (relatorioPedidosPorPeriodoCsv.Exists)
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
            Response.Headers.Add("Content-Disposition", "attachment; Filename="+ arquivo);

            byte[] input = Encoding.Unicode.GetBytes(conteudo_csv);
            return File(input, System.Net.Mime.MediaTypeNames.Application.Octet, arquivo);
        }

        [HttpGet]
        [Route("relatorio-saldo-por-codigo")]
        public IActionResult RelatorioSaldoPorCodigo(String dk)
        {
            var dados = _painelGerencialRepository.RelatorioSaldoPorCodigo(dk).Result;

            return Ok(new { dados = dados });
        }

        [HttpGet]
        [Route("relatorio-saldo-por-saldo-1")]
        public IActionResult RelatorioSaldoPorSaldo1()
        {
            var dados = _painelGerencialRepository.RelatorioSaldoPorSaldo1().Result;

            return Ok(new { dados = dados });
        }

        [HttpGet]
        [Route("relatorio-saldo-por-saldo-1-download")]
        public IActionResult RelatorioSaldoPorSaldo1Download()
        {
            var dados = _painelGerencialRepository.RelatorioSaldoPorSaldo1().Result;

            string conteudo_csv = "DK; Descrição; Saldo Total \n \t";

            foreach (var item in dados)
            {
                conteudo_csv += $"{item.DK.ToString()}; {item.DESCRICAO.ToString()}; {item.SALDO_TOTAL.ToString()}";
                conteudo_csv += "\n \t";
            }

            if (!Directory.Exists(_csv))
            {
                Directory.CreateDirectory(_csv);
            }

            string arquivo = "relatorio_saldo_por_saldo_1.csv";
            FileInfo relatorioSaldoPorSaldo1Csv = new FileInfo(arquivo);

            if (relatorioSaldoPorSaldo1Csv.Exists)
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
            Response.Headers.Add("Content-Disposition", "attachment; Filename=" + arquivo);

            byte[] input = Encoding.Unicode.GetBytes(conteudo_csv);
            return File(input, System.Net.Mime.MediaTypeNames.Application.Octet, arquivo);
        }

        [HttpGet]
        [Route("relatorio-preco-por-marketplace")]
        public IActionResult RelatorioPrecoPorMarketplace(string codigo, int deslocamento, int limite)
        {
            var dados = _painelGerencialRepository.RelatorioPrecoPorMarketplace(codigo, deslocamento, limite).Result;

            return Ok(new { dados = dados });
        }

        [HttpGet]
        [Route("relatorio-preco-por-codigo")]
        public IActionResult RelatorioPrecoPorCodigo(string dk)
        {
            var dados = _painelGerencialRepository.RelatorioPrecoPorCodigo(dk).Result;
            int total = dados.Count();

            return Ok(new { total = total, dados = dados });
        }

        [HttpGet]
        [Route("codigo-lista-de-preco")]
        public IActionResult CodigoListaDePreco()
        {
            var dados = _painelGerencialRepository.CodigoListaDePreco().Result;

            return Ok(new { dados = dados });
        }

        [HttpGet]
        [Route("relatorio-pendentes-vendas-por-periodo")]
        public IActionResult RelatorioPendentesVendasPorPeriodo(DateTime dataInicial, DateTime dataFinal, int deslocamento, int limite)
        {
            var dados = _painelGerencialRepository.RelatorioPendentesVendasPorPeriodo(dataInicial, dataFinal, deslocamento, limite).Result;

            var total = dados.Count() == 0 ? 0 : dados.FirstOrDefault().Total;
            var dataPedidoMaisAntigo = dados.Count() == 0 ? "Não há pendências nessa data" : dados.Min(x => x.PEDS_DAT_CAD);
            var dataPedidoMaisNovo = dados.Count() == 0 ? "Não há pendências nessa data" : dados.Max(x => x.PEDS_DAT_CAD);
            
            return Ok(new { total = total, dataPedidoMaisAntigo = dataPedidoMaisAntigo, dataPedidoMaisNovo = dataPedidoMaisNovo, dados = dados });
        }

        [HttpGet]
        [Route("relatorio-pendentes-vendas-data")]
        public IActionResult RelatorioPendentesVendasData()
        {
            var dados = _painelGerencialRepository.RelatorioPendentesVendasData().Result;
            var dataPedidoMaisAntigo = dados.FirstOrDefault().PEDS_MAIS_ANTIGO.ToString("dd/MM/yyyy HH:mm:ss");
            var dataPedidoMaisNovo = dados.FirstOrDefault().PEDS_MAIS_NOVO.ToString("dd/MM/yyyy HH:mm:ss");

            return Ok(new { dataPedidoMaisAntigo = dataPedidoMaisAntigo, dataPedidoMaisNovo = dataPedidoMaisNovo });
        }

        [HttpGet]
        [Route("relatorio-pendentes-vendas-por-periodo-grafico")]
        public IActionResult RelatorioPendentesVendasPorPeriodoGrafico(DateTime dataInicial, DateTime dataFinal)
        {
            var dados = _painelGerencialRepository.RelatorioPendentesVendasPorPeriodoGrafico(dataInicial, dataFinal).Result;
            if (dados.Any())
            {
                var dataMaisAntiga = dados?.FirstOrDefault().DATA;
                var dataMaisNova = dados?.LastOrDefault().DATA;
            }
            var totalDias = ((dataFinal - dataInicial).TotalDays) + 1;

            return Ok(new { totalDias = totalDias, dados = dados  });
        }

        [HttpGet]
        [Route("relatorio-pendentes-vendas-por-periodo-download")]
        public IActionResult RelatorioPendentesVendasPorPeriodoDownload(DateTime dataInicial, DateTime dataFinal, int deslocamento, int limite)
        {
            var dados = _painelGerencialRepository.RelatorioPendentesVendasPorPeriodo(dataInicial, dataFinal, deslocamento, limite).Result;

            string conteudo_csv = "Código Externo do Pedido; Código Interno do Pedido; Código do Item; Código do Produto; Valor Total; Data do Cadastro \n \t";

            foreach (var item in dados)
            {
                conteudo_csv += $"{item.PEDS_EXT_COD.ToString()}; {item.PEDS_COD.ToString()}; {item.ITEP_COD.ToString()}; {item.PROS_EXT_COD.ToString()}; {item.valor_total}; {item.PEDS_DAT_CAD}";
                conteudo_csv += "\n \t";
            }

            if (!Directory.Exists(_csv))
            {
                Directory.CreateDirectory(_csv);
            }

            string arquivo = "relatorio_pendencias_vendas_por_periodo.csv";
            FileInfo relatorioPendentesVendasPorPeriodoCsv = new FileInfo(arquivo);

            if (relatorioPendentesVendasPorPeriodoCsv.Exists)
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
            Response.Headers.Add("Content-Disposition", "attachment; Filename=" + arquivo);

            byte[] input = Encoding.Unicode.GetBytes(conteudo_csv);
            return File(input, System.Net.Mime.MediaTypeNames.Application.Octet, arquivo);
        }

        [HttpGet]
        [Route("relatorio-pendentes-vendas-por-codigo")]

        public IActionResult RelatorioPendentesVendasPorCodigo(string codigo)
        {
            var dados = _painelGerencialRepository.RelatorioPendentesVendasPorCodigo(codigo).Result;

            return Ok(new { dados = dados });
        }

        [HttpGet]
        [Route("relatorio-pendentes-vendas-por-codigo-download")]

        public IActionResult RelatorioPendentesVendasPorCodigoDownload(string codigo)
        {
            var dados = _painelGerencialRepository.RelatorioPendentesVendasPorCodigo(codigo).Result;

            string conteudo_csv = "Código Externo do Pedido; Código Interno do Pedido; Código do Item; Código do Produto; Valor Total; Data do Cadastro \n \t";

            foreach (var item in dados)
            {
                conteudo_csv += $"{item.PEDS_EXT_COD.ToString()}; {item.PEDS_COD.ToString()}; {item.ITEP_COD.ToString()}; {item.PROS_EXT_COD.ToString()}; {item.valor_total}; {item.PEDS_DAT_CAD}";
                conteudo_csv += "\n \t";
            }

            if (!Directory.Exists(_csv))
            {
                Directory.CreateDirectory(_csv);
            }

            string arquivo = "relatorio_pendencias_vendas_por_codigo.csv";
            FileInfo relatorioPendentesVendasPorCodigoCsv = new FileInfo(arquivo);

            if (relatorioPendentesVendasPorCodigoCsv.Exists)
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
            Response.Headers.Add("Content-Disposition", "attachment; Filename=" + arquivo);

            byte[] input = Encoding.Unicode.GetBytes(conteudo_csv);
            return File(input, System.Net.Mime.MediaTypeNames.Application.Octet, arquivo);
        }

        [HttpGet]
        [Route("relatorio-primeiro-envio-nucci")]
        public IActionResult RelatorioPrimeiroEnvioNucci()
        {
            var pedido = _painelGerencialRepository.RelatorioPrimeiroEnvioNucci().Result;
            var quantidade = pedido.Count();

            if (quantidade <= 30)
            {
                return Ok(new { total = quantidade, pedidos = pedido, cor = "#43A047" });
            }
            else if (quantidade > 30 && quantidade <= 50)
            {
                return Ok(new { total = quantidade, pedidos = pedido, cor = "#FFB300" });
            }
            else
            {
                return Ok(new { total = quantidade, pedidos = pedido, cor = "#D32F2F" });
            }
        }

        [HttpGet]
        [Route("relatorio-primeiro-envio-com-erro-nucci")]

        public IActionResult RelatorioPrimeiroEnvioComErroNucci()
        {
            var pedido = _painelGerencialRepository.RelatorioPrimeiroEnvioComErroNucci().Result;
            var quantidade = pedido.Count();

            if (quantidade <= 30)
            {
                return Ok(new { total = quantidade, pedidos = pedido, cor = "#43A047" });
            }
            else if (quantidade > 30 && quantidade <= 50)
            {
                return Ok(new { total = quantidade, pedidos = pedido, cor = "#FFB300" });
            }
            else
            {
                return Ok(new { total = quantidade, pedidos = pedido, cor = "#D32F2F" });
            }
        }

        [HttpGet]
        [Route("relatorio-aguardando-despacho-nucci")]

        public IActionResult RelatorioAguardandoDespacho()
        {
            var pedido = _painelGerencialRepository.RelatorioAguardandoDespachoNucci().Result;
            var quantidade = pedido.Count();

            if (quantidade <= 30)
            {
                return Ok(new { total = quantidade, pedidos = pedido, cor = "#43A047" });
            }
            else if (quantidade > 30 && quantidade <= 50)
            {
                return Ok(new { total = quantidade, pedidos = pedido, cor = "#FFB300" });
            }
            else
            {
                return Ok(new { total = quantidade, pedidos = pedido, cor = "#D32F2F" });
            }
        }
    }
}