using Dapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PainelGerencial.Domain;
using PainelGerencial.Repository;
using PainelGerencial.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Ubiety.Dns.Core;


namespace PainelGerencial.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class AuditoriaProdutosController : ControllerBase
    {
        private readonly IAuditoriaProdutosRepository _auditoriaProdutosRepository;
        private readonly IAuditoriaProdutosServicos _auditoriaProdutosServicos;
        private const string _arquivoProdutoDivergente = @"\\192.168.0.179\inetpub\wwwroot\PainelGerencialAPI\download\csv\produtos-divergentes.csv";
        private const string _arquivoRelatorioAuditoria = @"\\192.168.0.179\inetpub\wwwroot\PainelGerencialAPI\download\csv\relatorio-auditoria.csv";
        private const string _arquivo = @"\\192.168.0.179\inetpub\wwwroot\PainelGerencialAPI\download\csv\";

        public AuditoriaProdutosController(
            IAuditoriaProdutosRepository auditoriaProdutosRepository, 
            IAuditoriaProdutosServicos auditoriaProdutosServicos)
        {
            _auditoriaProdutosRepository = auditoriaProdutosRepository;
            _auditoriaProdutosServicos = auditoriaProdutosServicos;
        }

        [HttpGet]
        [Route("relatorio-medidas-divergentes-csv-api")]
        public IActionResult relatorio_medidas_divergentes_csv_api()
        {
            IActionResult relatorio = this.atualizar_dados_medidas();

            relatorio = (IActionResult)this.relatorio_medidas_divergentes_csv();

            return relatorio;
        }


        [NonAction]
        public void verifica_divergencias_medidas()
        {
            var dados = _auditoriaProdutosRepository.DadosProdutosPai();

            /* Limpa tabela */
            _auditoriaProdutosRepository.LimpaProdutosDivergentes();

            foreach (var pai in dados)
            {
                List<ProdutoFilho_Model> dados_kit = (List<ProdutoFilho_Model>)_auditoriaProdutosRepository.DadosProdutoFilho(pai.id);

                if (dados_kit.Any())
                {
                    foreach(var kit in dados_kit)
                    {
                        List<Divergencia_Produto_Model> dados_divergentes = new List<Divergencia_Produto_Model>();

                        if ((float?)kit.comprimento != (float?)pai.comprimento)
                        {
                            dados_divergentes.Add(new Divergencia_Produto_Model
                            {
                                tipo = "Comprimento",
                                valor_pai = (float?)pai.comprimento,
                                valor_kit = (float?)kit.comprimento,
                                diferenca = ((float?)(pai.comprimento - kit.comprimento))
                            });
                        }

                        if ((float?)kit.largura != (float?)pai.largura)
                        {
                            dados_divergentes.Add(new Divergencia_Produto_Model
                            {
                                tipo      = "Largura",
                                valor_pai = (float?)pai.largura,
                                valor_kit = (float?)kit.largura,
                                diferenca = ((float?)(pai.largura - kit.largura))
                            });
                        }

                        if ((float?)kit.espessura != (float?)pai.espessura)
                        {
                            dados_divergentes.Add(new Divergencia_Produto_Model
                            {
                                tipo      = "Espessura",
                                valor_pai = (float?)pai.espessura,
                                valor_kit = (float?)kit.espessura,
                                diferenca = ((float?)(pai.espessura - kit.espessura))
                            });
                        }

                        if ((float?)kit.peso != (float?)pai.peso)
                        {
                            dados_divergentes.Add(new Divergencia_Produto_Model
                            {
                                tipo      = "Peso",
                                valor_pai = (float?)pai.peso,
                                valor_kit = kit.peso,
                                diferenca = ((float?)(pai.peso - kit.peso))
                            });
                        }

                        if (dados_divergentes.Any())
                        {
                            var divergencias = JsonConvert.SerializeObject(dados_divergentes);

                            var data = new ProdutoDivergente_Model()
                            {
                                id = kit.id,
                                dk = kit.dk,
                                id_pai = pai.id,
                                dk_pai = pai.dk,
                                nome = kit.nome,
                                estoque = (int?)kit.estoque,
                                divergencias = dados_divergentes
                            };

                            _auditoriaProdutosRepository.InsereProdutosDivergentes(data);
                        }
                    }
                }
            }
        }

        
        [HttpGet]
        [Route("atualizar-dados-medidas")]
        /* Medidas divergentes: Produtos Pai x kits */
        public IActionResult atualizar_dados_medidas()
        {
            var sw = new Stopwatch();
            sw.Start();

            List<dynamic> DadosProduto = (List<dynamic>)_auditoriaProdutosRepository.DadosProduto();

            if (DadosProduto.Count > 0)
            {
                /* Limpa as tabelas */
                _auditoriaProdutosRepository.LimparProdutosPaiAsync();
                _auditoriaProdutosRepository.LimparProdutosFilhosAsync();

                _auditoriaProdutosRepository.InsereProdutoPaiEmLote();

                //foreach (var dado in DadosProduto)
                //{
                //string classe_nome = "";

                //if (dado.classe == 1)
                //{
                //    classe_nome = "Normal";
                //}

                //if (dado.classe == 7)
                //{
                //    classe_nome = "Produto Novo";
                //}

                //if (dado.classe == 20)
                //{
                //    classe_nome = "Anúncio Novo";
                //}

                //var produtoPai = new ProdutoPai_Model()
                //{
                //    id = dado.codigo,
                //    dk = dado.dk,
                //    nome = dado.nome,
                //    classe = dado.classe,
                //    classe_nome = dado.classe_nome,
                //    peso = (double)dado.peso,
                //    comprimento = (float?)dado.comprimento,
                //    largura = (float?)dado.largura,
                //    espessura = (float?)dado.espessura
                //};

                //_auditoriaProdutosRepository.InsereProdutoPai(produtoPai);
                //}

                /* Seleciona os produtos (filhos) */
                _auditoriaProdutosRepository.InsereProdutoFilhoEmLote();
                
                //var dados = _auditoriaProdutosRepository.DksPai();

                //foreach (var item in dados)
                //{
                //    List<dynamic> dadosProduto = _auditoriaProdutosRepository.DadosProdutoPai(item.id);

                //    if (dadosProduto.Count > 0)
                //    {
                //        foreach(var dado in dadosProduto)
                //        {
                //            var filho = new ProdutoFilho_Model()
                //            {
                //                id = dado.codigo,
                //                dk = dado.dk,
                //                id_pai = (int?)dado.id_pai,
                //                dk_pai = item.dk,
                //                nome = dado.nome,
                //                estoque = (float?)dado.estoque,
                //                peso = (float)dado.peso,
                //                comprimento = (float?)dado.comprimento,
                //                largura = (float?)dado.largura,
                //                espessura = (float?)dado.espessura
                //            };

                //            _auditoriaProdutosRepository.InsereProdutoFilho(filho);
                //        }
                //    }
                //}

                this.verifica_divergencias_medidas();
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds.ToString());

            return Ok(new
            {
                status = "Atualização de produtos realizado com Sucesso!"
            });
        }

        [NonAction]
        public IActionResult relatorio_medidas_divergentes_csv()
        {
            List<ProdutoDivergente_Model> dados = (List<ProdutoDivergente_Model>)_auditoriaProdutosRepository.ListaProdutosDivergentesAsync();

            string conteudo_csv = "Data de Geração: " + DateTime.Now.ToString("dd/MM/yyyy") + "\n \t";
            conteudo_csv += "DK; Nome Filho; DK Pai; Nome Pai; Classe; Estoque; Comprimento; Largura; Espessura; Peso; \n \t";

            foreach(var item in dados)
            {
                string dk = item.dk;
                string nome = item.nome;
                string dk_pai = item.dk_pai;
                string nome_pai = item.ProdutoPai.nome;
                string classe = item.ProdutoPai.classe_nome;
                int? estoque = item.estoque;

                string comprimento = "";
                string largura = "";
                string espessura = "";
                string peso = "";

                foreach(var divergencia in item.divergencias)
                {
                    if (divergencia.tipo == "Comprimento")
                    {
                        comprimento = "Pai: " + divergencia.valor_pai + " - Filho: " 
                            + divergencia.valor_kit + " - Dif: " + divergencia.diferenca;
                    }

                    if (divergencia.tipo == "Largura")
                    {
                        largura = "Pai: " + divergencia.valor_pai + " - Filho: "
                            + divergencia.valor_kit + " - Dif: " + divergencia.diferenca;
                    }

                    if (divergencia.tipo == "Espessura")
                    {
                        espessura = "Pai: " + divergencia.valor_pai + " - Filho: "
                            + divergencia.valor_kit + " - Dif: " + divergencia.diferenca;
                    }

                    if (divergencia.tipo == "Peso")
                    {
                        peso = "Pai: " + divergencia.valor_pai + " - Filho: "
                            + divergencia.valor_kit + " - Dif: " + divergencia.diferenca;
                    }

                    conteudo_csv += $"{dk}; {nome}; {dk_pai}; {nome_pai}; {classe}; " +
                        $"{estoque}; {comprimento}; {largura}; {espessura}; {peso}; \n \t";
                }

            }

            FileInfo divergente = new FileInfo(_arquivoProdutoDivergente);

            if (divergente.Exists)
            {
                System.IO.File.Delete(_arquivoProdutoDivergente);
            }

            using (StreamWriter sw = new StreamWriter(_arquivoProdutoDivergente, false, Encoding.Unicode))
            {
                sw.WriteLine(conteudo_csv);
            }
            
            
            return Ok(new
            {
                download = _arquivoProdutoDivergente.ToString()
            });
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult fornecedores(string codigo)
        {
            bool produtoExistente = _auditoriaProdutosRepository.ProdutoExistente(codigo);

            if (! produtoExistente)
            {
                return Ok(new
                {
                    status = $"Nenhum produto encontrado com o código {codigo}."
                });
            } 
            else
            {
                /* Informações do Fornecedor do Produto */
                List<dynamic> dadosFornecedores = (List<dynamic>)_auditoriaProdutosRepository.Fornecedores(codigo);

                if (dadosFornecedores.Count == 0)
                {
                    return Ok(new
                    {
                        status = $"Nenhum produto encontrado com o código {codigo}."
                    });
                }
                else
                {
                    return Ok(new
                    {
                        fornecedores = dadosFornecedores
                    });
                }
            }
        }

        [HttpGet]
        [Route("relatorio-medidas-divergentes")]
        public async Task<IActionResult> RelatorioMedidasDivergentes(string dk, string nome)
        {
            dk = dk?.Trim();
            nome = nome?.Trim();

            var relatorio = await _auditoriaProdutosRepository.RelatorioMedidasDivergentes(dk, nome);

            return Ok(new
            {
                quantidade_registros = relatorio.Count,
                relatorio = relatorio
            });
        }


        [HttpGet]
        [Route("relatorio-auditoria")]
        public async Task<IActionResult> RelatorioAuditoria()
        {
            List<dynamic> relatorio = (List<dynamic>)_auditoriaProdutosRepository.RelatorioAuditoria();

            relatorio.Select(x =>
            {
                x.data_auditoria = x.data_auditoria.ToString("dd/MM/yyyy hh:mm:ss");
                x.data_alteracao = x.data_alteracao.ToString("dd/MM/yyyy hh:mm:ss");
                x.data_geracao_relatorio = x.data_geracao_relatorio.ToString("dd/MM/yyyy hh:mm:ss");
                x.data_envio = x.data_envio.ToString("dd/MM/yyyy hh:mm:ss");
                return x;
            }).ToList();

            if (relatorio.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhum resultado encontrado."
                });
            }
            else
            {
                return Ok(new
                {
                    quantidade_registros = relatorio.Count,
                    ralatorio_auditoria = relatorio
                });
            }
        }


        [HttpGet]
        [Route("dados")]
        public IActionResult ProdutoExistente(string codigoExterno)
        {
            bool produtoExistente = _auditoriaProdutosRepository.ProdutoExistente(codigoExterno);

            if (! produtoExistente)
            {
                return Ok(new
                {
                    status = $"Nenhum produto encontrado com o código {codigoExterno}."
                });
            }
            else
            {
                /* Informações do Produto */
                List<dynamic> dados_produto = (List<dynamic>)_auditoriaProdutosRepository.DadosProduto(codigoExterno);
                var dados_abacos = _auditoriaProdutosRepository.DadosAbacos(codigoExterno);
                var kits_produto = _auditoriaProdutosRepository.KitsProduto(codigoExterno);
                var fornecedores_produto = _auditoriaProdutosRepository.FornecedoresProduto(codigoExterno);
                var historico = _auditoriaProdutosRepository.Historico(codigoExterno);

                dados_produto.Select(x =>
                {
                    x.data_auditoria = x.data_auditoria.ToString("dd/MM/yyyy hh:mm:ss");
                    x.data_alteracao = x.data_alteracao.ToString("dd/MM/yyyy hh:mm:ss");
                    x.data_geracao_relatorio = x.data_geracao_relatorio.ToString("dd/MM/yyyy hh:mm:ss");
                    x.data_envio = x.data_envio.ToString("dd/MM/yyyy hh:mm:ss");
                    return x;
                }).ToList();

                foreach(var item in dados_produto)
                {
                    item.kit = (object)_auditoriaProdutosRepository.ProdutoKit(codigoExterno);
                };

                if (dados_produto.Count == 0)
                {
                    return Ok(new
                    {
                        status = $"Nenhuma informação encontrada com o código {codigoExterno}."
                    });
                } 
                else
                {
                    return Ok(new
                    {
                        produto = dados_produto.FirstOrDefault(),
                        abacos = dados_abacos,
                        kits = kits_produto,
                        anuncios = new List<dynamic>(),
                        fornecedores = fornecedores_produto,
                        historico = historico
                    });
                }
            }
        }

        [HttpGet]
        [Route("relatorio-geral")]
        public IActionResult RelatorioGeral(DateTime? dataInicial, DateTime? dataFinal, int pagina = 1, int limit = 100)
        {
            if (dataInicial == null || dataFinal == null)
            {
                return Ok(new
                {
                    status = "Por favor, insira a data incial e data final"
                });
            }

            List<dynamic> dados = (List<dynamic>)_auditoriaProdutosRepository.RelatorioGeral(dataInicial, dataFinal, pagina, limit);

            string conteudo_csv = "Código de Barras; DK; DK Pai; Descrição; Fornecedor; Largura; Comprimento; Espessura; Peso; Auditado; Data da Auditoria; \n \t ";

            foreach(var item in dados)
            {
                conteudo_csv += $"{item.codigo_barras}; {item.dk}; {item.dk_pai}; {item.descricao}; {item.fornecedor}; {item.largura}; {item.comprimento}; {item.espessura}; {item.peso}; {item.auditado}; {item.data_auditoria}; \n \t ";
            }

            FileInfo arquivo = new FileInfo(_arquivoRelatorioAuditoria);

            if (arquivo.Exists)
            {
                System.IO.File.Delete(_arquivoRelatorioAuditoria);
            }

            using (StreamWriter sw = new StreamWriter(_arquivoRelatorioAuditoria, false, Encoding.Unicode))
            {
                sw.WriteLine(conteudo_csv);
            }

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhum resultado encontrado."
                });
            }
            else
            {
                return Ok(new
                {
                    quantidade_registros = dados.FirstOrDefault().QtdRegistros,
                    pagina = pagina,
                    quantidade_pagina = dados.Count,
                    relatorio_auditoria_csv = _arquivoRelatorioAuditoria,
                    ralatorio_auditoria = dados
                });
            }
        }

        [HttpGet]
        [Route("download/{arquivo}")]
        public IActionResult Download(string arquivo)
        {
            string nomeArquivo = _arquivo + arquivo;
            Response.Headers.Add("Content-type", "application/octet-stream");
            Response.Headers.Add("Content-Disposition", "attachment; Filename="
                + nomeArquivo);

            byte[] input = System.IO.File.ReadAllBytes(nomeArquivo);
            return File(input, System.Net.Mime.MediaTypeNames.Application.Octet, nomeArquivo);
        }

        [HttpGet]
        [Route("verifica-peso/{dk}/{peso}")]
        public IActionResult VerificaPeso(string dk, float peso)
        {
            var verificaPeso = _auditoriaProdutosRepository.VerificaPeso(dk, peso);

            return Ok(new { status = verificaPeso });
        }

        [HttpGet]
        [Route("gravar-corrigido/{id}/{codigo}")]
        public async Task<IActionResult> GravarCorrigido(int id, string codigo)
        {

            if (id == null || string.IsNullOrEmpty(codigo))
            {
                return Ok(new { status = "Por favor, insira todos os campos" });
            } 
            else
            {
                var produtoExistente = _auditoriaProdutosRepository.ProdutoExistente(codigo);

                if (!produtoExistente)
                {
                    return Ok(new { status = $"Nenhum produto encontrado com o código {codigo}." });
                }
                else
                {
                    var corrigido = await _auditoriaProdutosServicos.Corrigido(id, codigo);
                    if (!corrigido)
                    {
                        return Ok(new { status = "Não existem dados para serem exibidos" });
                    }

                    return Ok(new { status = "Informações gravadas com Sucesso!" });
                }
            }

        }


    }
}
