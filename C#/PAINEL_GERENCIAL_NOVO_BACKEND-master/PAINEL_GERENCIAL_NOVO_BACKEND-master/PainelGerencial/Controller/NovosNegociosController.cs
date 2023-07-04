using Ionic.Zip;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PainelGerencial.Domain;
using PainelGerencial.Repository;
using PainelGerencial.Utils;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PainelGerencial.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class NovosNegociosController : ControllerBase
    {
        private readonly INovosNegociosRepository _novosNegociosRepository;
        private readonly string _caminho = @"\\192.168.0.179\inetpub\wwwroot\PainelGerencialAPI\download\word\plano-acao-ata\";
        private const string _csv = @"\\192.168.0.179\inetpub\wwwroot\PainelGerencialAPI\download\csv\novos-negocios\";
        private const string _arquivosSetores = @"\\192.168.0.179\inetpub\wwwroot\PainelGerencialAPI\download\novos-negocios-arquivos-setores\";
        private const string _arquivosPadroes = @"\\192.168.0.179\inetpub\wwwroot\PainelGerencialAPI\download\novos-negocios-arquivos-padroes\";
        public NovosNegociosController(INovosNegociosRepository novosNegociosRepository)
        {
            _novosNegociosRepository = novosNegociosRepository;
        }

        [NonAction]
        public IActionResult Index()
        {
            return Ok(null);
        }

        [HttpGet]
        [Route("marcas")]
        public IActionResult Marcas()
        {
            List<dynamic> marcas = (List<dynamic>)_novosNegociosRepository.Marcas();

            return Ok(new
            {
                quantidade_registros = marcas.Count,
                marcas = marcas
            });
        }

        [HttpGet]
        [Route("usuarios-departamento")]
        public IActionResult UsuariosDepartamentoLista()
        {
            List<dynamic> usuarios = (List<dynamic>)_novosNegociosRepository.UsuariosDepartamentoLista();

            return Ok(new
            {
                usuarios = usuarios
            });
        }

        [HttpPost]
        [Route("plano-de-acao-cadastrar")]
        public IActionResult plano_acao_gravar([FromBody] PlanoAcao_Model planoAcao)
        {
            List<string> erros = new List<string>();
            if (planoAcao.data_reuniao == null)
            {
                erros.Add("Por favor insira a data da visita");
            }

            if (planoAcao.marca == null)
            {
                erros.Add("Por favor insira a marca");
            }

            if (erros.Count == 0)
            {
                planoAcao.usuario_nome = planoAcao.usuario_email.Substring(0, planoAcao.usuario_email.IndexOf("@")).Replace('.', ' ');
                planoAcao.usuario = _novosNegociosRepository.usuario_Id(planoAcao.usuario_email).FirstOrDefault().id;
                long? ultimoId = 0;
                if (planoAcao.id == null)
                {
                    List<dynamic> listaIds = new List<dynamic>();
                    listaIds = (List<dynamic>)_novosNegociosRepository.plano_acao_inserir(planoAcao);
                    ultimoId = (long)listaIds.FirstOrDefault().insert_id;
                }
                else
                {
                    ultimoId = _novosNegociosRepository.plano_acao_editar(planoAcao);
                }

                return Ok(new
                {
                    erros = false,
                    mensagem = "Informações cadastradas com Sucesso",
                    id = ultimoId
                });
            }
            else
            {
                return Ok(new
                {
                    erros = true,
                    mensagem = erros,
                    id = ""
                });
            }
        }

        [HttpPost]
        [Route("plano-de-acao-itens-cadastrar")]
        public IActionResult PlanoDeAcaoItensCadastrar([FromBody] PlanoAcaoItem_Model planoAcaoItem)
        {
            List<string> erros = new List<string>();
            if (planoAcaoItem.descricao == null)
            {
                erros.Add("Por favor insira o descrição");
            }

            if (erros.Count == 0)
            {
                string usuario_nome = planoAcaoItem.usuario_email.Substring(0, planoAcaoItem.usuario_email.IndexOf("@"));
                int usuario = _novosNegociosRepository.usuario_Id(planoAcaoItem.usuario_email).FirstOrDefault().id;
                
                planoAcaoItem.usuario_nome = usuario_nome;
                planoAcaoItem.usuario = usuario;

                // Pega a lista de responsável e atribui o id
                // inicio
                foreach(var item in planoAcaoItem.responsavel)
                {
                    item.id = _novosNegociosRepository.UsuarioPorId(item.email).FirstOrDefault().id;
                }
                // fim

                // Se não preencher a data prazo, colocar por padrão 15 dias
                if (planoAcaoItem.data_prazo == null)
                {
                    planoAcaoItem.data_prazo = DateTime.Now.AddDays(15);
                }

                var dados = _novosNegociosRepository.PlanoAcaoItensGravar(planoAcaoItem);

                return Ok(new
                {
                    erros = false,
                    mensagem = "Informações cadastradas com Sucesso",
                    dados = dados
                });
            }
            else
            {
                return BadRequest(new 
                {
                    erros = true,
                    mensagem = erros.ToString(),
                    id = ""
                });
            }
        }

        [HttpGet]
        [Route("plano-de-acao-download")]
        public IActionResult planoAcaoDownload(int id, bool geral = false)
        {
            List<dynamic> _planoAcao = (List<dynamic>)_novosNegociosRepository.DadosPlanoAcao(id);
            List<PlanoAcaoItem_Model> _dados_plano_acao_item = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.PlanosAcaoItens(id);

            if (_planoAcao.Count == 0)
            {
                return Ok(new
                {
                    mensagem = "Não há dados."
                });
            }

            var plano_acao_id = _planoAcao.FirstOrDefault().id;
            var plano_acao_nome = _planoAcao.FirstOrDefault().usuario_nome;
			int? plano_acao_marca = _planoAcao.FirstOrDefault().marca;
            var plano_acao_marca_nome = _novosNegociosRepository.MarcaNome((int)plano_acao_marca).FirstOrDefault().nome;
            var plano_acao_data_reuniao = _planoAcao.FirstOrDefault().data_reuniao;
            var plano_acao_data_cadastro = _planoAcao.FirstOrDefault().data_cadastro;
            var plano_acao_usuario_id = _planoAcao.FirstOrDefault().usuario;
            var plano_acao_usuario_email = _planoAcao.FirstOrDefault().usuario_email;
            var plano_acao_usuario_nome = _planoAcao.FirstOrDefault().usuario_nome;
            

            string conteudo_csv = "";
            conteudo_csv += "Código: "+plano_acao_id+"; \n \t";
			conteudo_csv += "Nome: "+plano_acao_nome+"; \n \t";
			conteudo_csv += "Marca: "+plano_acao_marca_nome+"; \n \t";
			conteudo_csv += "Data da Reunião: "+plano_acao_data_reuniao+"; \n \t";
			conteudo_csv += "Data de Cadastro: "+plano_acao_data_cadastro+"; \n \t";
			conteudo_csv += "Usuário Responsável: "+plano_acao_usuario_nome+" ("+plano_acao_usuario_email+"); \n \t";

            conteudo_csv += "\n \t";

			conteudo_csv += "Descrição; Responsável; Data Prazo; Data Conclusão; Dias para Finalizar; Status; Efetividade; Comentários; \n \t";

            foreach(var item in _dados_plano_acao_item)
            {
                int plano_acao_item = (int)item.id;
                var nome = item.usuario_nome;
                string descricao = item.descricao.Replace(';', ' ');
                var responsavel_arr = _novosNegociosRepository.PlanoAcaoItensResponsavel((int)item.id);
                var data_prazo = item.data_prazo;
                var data_conclusao = item.data_conclusao;
                string status = item.status_nome.ToString();
                int efetividade = item.efetividade;
                string s_efetividade = "";
                var data_cadastro = item.data_cadastro;
                var usuario_nome = item.usuario_nome;
                var usuario_email = item.usuario_email;
                var dias_para_finalizar = item.dias_para_finalizar;

                string responsavel = "";

                foreach(var responsavel_dados in responsavel_arr)
                {
                    responsavel += responsavel_dados.nome + " (" + responsavel_dados.email + ") ";
                }

                if(item.status == 2)
                {
                    dias_para_finalizar = 0;
                }

                if(item.status == 1)
                {
                    status = "Concluído";
                } else
                {
                    status = "Em Andamento";
                }

                if (efetividade == 1){
					s_efetividade = "Concluído no Prazo";
                }else if (efetividade == 2){
                    s_efetividade = "Concluído com Atraso";
                }else
                {
                    s_efetividade = "Em Andamento";
                }

                var observacoes_itens = (List<dynamic>) _novosNegociosRepository.ObservacoesItens(plano_acao_item);
                string comentarios = "";

                foreach(var v in observacoes_itens)
                {
                    comentarios += v.descricao + " \n";
                }

                conteudo_csv += descricao + "; "+responsavel+"; "+data_prazo+"; "+data_conclusao+"; "+dias_para_finalizar+"; "+status+"; "+ s_efetividade + "; "+comentarios+"; \n \t";
            }

            string arquivo = _csv + "plano-acao-" + id.ToString() + ".csv";
            FileInfo plano_acao_csv = new FileInfo(arquivo);

            if (!Directory.Exists(_csv))
            {
                Directory.CreateDirectory(_csv);
            }



            if (plano_acao_csv.Exists)
            {
                //using (StreamWriter sw = plano_acao_csv.CreateText())
                using (StreamWriter sw = new StreamWriter(arquivo, false, Encoding.Unicode))
                {
                    sw.WriteLine(conteudo_csv);
                }
            }
            else
            {
                //using (StreamWriter sw = plano_acao_csv.AppendText())
                using (StreamWriter sw = new StreamWriter(arquivo, false, Encoding.Unicode))
                {
                    sw.WriteLine(conteudo_csv);
                }
            }


            if (geral)
            {
                return Ok(new
                {
                    arquivo_csv = _csv + "plano-acao-" + id.ToString() + ".csv"
                });
            }
            else
            {
                string nomeArquivo = "plano-acao-" + id + ".csv";
                Response.Headers.Add("Content-type", "application/octet-stream");
                Response.Headers.Add("Content-Disposition", "attachment; Filename="
                    + nomeArquivo);

                byte[] input = Encoding.Unicode.GetBytes(conteudo_csv);
                return File(input, System.Net.Mime.MediaTypeNames.Application.Octet, nomeArquivo);
            }

            
        }

        [HttpGet]
        [Route("plano-de-acao-download-word")]
        public IActionResult plano_acao_download_word(long id, bool geral = false)
        {
            List<dynamic> dadosPlanoAcao = (List<dynamic>)_novosNegociosRepository.DadosPlanoAcao(id);
            List<dynamic> dadosPlanoAcaoItens = (List<dynamic>)_novosNegociosRepository.DadosPlanoAcaoItens(id);

            if (dadosPlanoAcao.Count == 0)
            {
                return Ok(new
                {
                    mensagem = "Não há dados."
                });
            }

            string plano_acao_data_reuniao = dadosPlanoAcao.FirstOrDefault().data_reuniao?.ToString("dd/MM/yyyy");
            string plano_acao_hora_inicial = dadosPlanoAcao.FirstOrDefault().hora_inicial?.ToString();
            string plano_acao_hora_final = dadosPlanoAcao.FirstOrDefault().hora_final?.ToString();
            var plano_acao_local = dadosPlanoAcao.FirstOrDefault().local;
            var plano_acao_participantes = dadosPlanoAcao.FirstOrDefault().participantes;
            var plano_acao_pauta = dadosPlanoAcao.FirstOrDefault().pauta;

            int? plano_acao_marca = 0;
            if (dadosPlanoAcao.Count > 0)
            {
                plano_acao_marca = dadosPlanoAcao.FirstOrDefault().marca;
            }
            string plano_acao_marca_nome = (plano_acao_marca != 0) ?
                _novosNegociosRepository.MarcaNome((int)plano_acao_marca).FirstOrDefault().nome :
                dadosPlanoAcao.FirstOrDefault().marca_nome;

            string itens_descricao = "";
            int cont = 1;
            // se houve mais de um plano de ação
            foreach (var item in dadosPlanoAcaoItens)
            {
                var planoAcao = item.plano_acao;
                var descricao = item.descricao;

                itens_descricao += planoAcao + "." + cont + descricao + "<br />";

                var filhos = _novosNegociosRepository.Filhos((int)item.id);
                int cont2 = 1;
                foreach (var filho in filhos)
                {
                    string descricao_filho = filho.descricao;
                    itens_descricao += "&nbsp; &nbsp; &nbsp; " + planoAcao + "." + cont.ToString() + "." + cont2.ToString() + " - " + descricao_filho + "<br />";
                    cont2++;
                }

                cont++;
            }

            string marca_doc = plano_acao_marca_nome?.ToLower().Replace(' ', '-').ToString();

            string conteudo = @"<html>
                                    <meta http-equiv='Content-Type' content='text/html; charset=utf-8'>
                                    <body>
                                        <div style='width: 100%; background-color: #7f0000; color: #ffffff; text-align: center;'> 
                                            <strong> ATA DE REUNIÃO </strong> 
                                        </div>
                                        <div style='width: 100%; background-color: #e5e5e5;'>
				                            <strong>Data da Reunião:</strong> " + plano_acao_data_reuniao + @" &nbsp; <strong>Horário de Início:</strong> " + plano_acao_hora_inicial + @" &nbsp; <strong>Horário de Término:</strong> " + plano_acao_hora_final + @" <br />
				                            <strong>Local:</strong> " + plano_acao_local + @" <br />
				                            <strong>Participantes:</strong> " + plano_acao_participantes + @" <br />
				                            <strong>Pauta:</strong> " + plano_acao_pauta + @" <br />
				                        </div> <br />
				                        " + itens_descricao + @"
				                    </body>
                                </html>";

            try
            {
                if (geral)
                {
                    string arquivo = _caminho + @"\ata-plano-de-acao-" + id.ToString() + "-" + marca_doc + ".doc";
                    FileInfo plano_acao_doc = new FileInfo(arquivo);

                    if (!Directory.Exists(_caminho))
                    {
                        Directory.CreateDirectory(_caminho);
                    }



                    if (plano_acao_doc.Exists)
                    {
                        //using (StreamWriter sw = plano_acao_doc.CreateText())
                        using (StreamWriter sw = new StreamWriter(arquivo, false, Encoding.UTF8))
                        {
                            sw.WriteLine(conteudo);
                        }
                    }
                    else
                    {
                        //using (StreamWriter sw = plano_acao_doc.AppendText())
                        using (StreamWriter sw = new StreamWriter(arquivo, false, Encoding.UTF8))
                        {
                            sw.WriteLine(conteudo);
                        }
                    }
                    
                }
                else
                {
                    string nomeArquivo = "ata-plano-de-acao-" + id + "-" + marca_doc + ".doc";
                    Response.Headers.Add("Content-type", "application/vnd.ms-word");
                    Response.Headers.Add("Content-Disposition", "attachment; Filename="
                        + nomeArquivo);

                    byte[] input = Encoding.UTF8.GetBytes(conteudo);
                    return File(input, System.Net.Mime.MediaTypeNames.Application.Octet, nomeArquivo);
                }
            } catch(Exception ex)
            {
                return BadRequest(ex);
            }


            return Ok();
        }

        [NonAction]
        public async Task<dynamic> plano_acao_geral_download_word(List<dynamic> plano_acao_arr)
        {
            // migrado do php
            string arquivoZip = _caminho + "plano-acao-ata.zip";

            if (plano_acao_arr.ToList<dynamic>().Count > 0)
            {
                // Exclui somente os arquivos que estão dentro da pasta, não a PASTA
                DirectoryInfo diretorio = new DirectoryInfo(_caminho);
                if (diretorio.Exists)
                {
                    foreach (FileInfo arquivo in diretorio.GetFiles())
                    {
                        arquivo.Delete();
                    }
                }
                
                // Excluir o ZIP da pasta
                FileInfo planoZip = new FileInfo(arquivoZip);
                if (planoZip.Exists)
                {
                    planoZip.Delete();
                }


                foreach (var plano in plano_acao_arr)
                {
                    var teste = this.plano_acao_download_word(plano.id, true);
                }


                
                // "ZIPA" a pasta plano-acao-ata para plano-acao-ata.zip
                using (ZipFile zip = new ZipFile(arquivoZip))
                {
                    // percorre todos os arquivos da lista
                    foreach (var item in diretorio.GetFiles())
                    {
                        // se o item é um arquivo
                        if (System.IO.File.Exists(item.FullName))
                        {
                            try
                            {
                                // Adiciona o arquivo na pasta raiz dentro do arquivo zip
                                zip.AddFile(item.FullName, "plano-acao-ata");
                            } 
                            catch(Exception ex)
                            {
                                throw;
                            }
                        }
                        // se o item é uma pasta
                        else if (Directory.Exists(item.FullName))
                        {
                            try
                            {
                                // Adiciona a pasta no arquivo zip com o nome da pasta
                                zip.AddDirectory(item.Name, new DirectoryInfo(item.Name).Name);
                            }
                            catch (Exception ex)
                            {
                                throw;
                            }
                        }
                    }
                    // Salva o arquivo zip para o destino
                    try
                    {
                        zip.Save();
                    }
                    catch(Exception ex)
                    {
                        throw;
                    }
                }
            }


            //       return true;

            return arquivoZip;
        }

        [HttpGet]
        [Route("plano-de-acao-relatorio")]
        public async Task<IActionResult> PlanoAcaoRelatorioAsync(int marca, string? responsavel, DateTime data_reuniao, int status_relatorio)
        {

            List<dynamic> _planosAcao = (List<dynamic>)_novosNegociosRepository.PlanoAcaoRelatorio(marca, responsavel, data_reuniao, status_relatorio);
            foreach (var item in _planosAcao)
            {
                // teste - refatorar
                List<dynamic> _plano_acao_item_progresso = _novosNegociosRepository.plano_acao_item_progresso(item.id);
                if (_plano_acao_item_progresso.Count > 0)
                {
                    item.progresso_quantidade = _plano_acao_item_progresso.FirstOrDefault().qtd_concluidos;
                    item.progresso_porcentagem = _plano_acao_item_progresso.FirstOrDefault().porcentagem_status;
                }
                
                item.codigo = item.id;
                try
                {
                    int.Parse(item.fornecedor);
                    item.fornecedor_nome = _novosNegociosRepository.FornecedorNome(item.fornecedor);
                }
                catch (Exception ex)
                {
                    item.fornecedor_nome = item.fornecedor;
                }
                item.item_check = "0";
                item.solicitar_alteracao = "0";

                List<dynamic> dados_itens = (List<dynamic>)_novosNegociosRepository.DadosPlanoAcaoItens(item.id);

                if (dados_itens.Count > 0)
                {
                    int cont = 1;

                    int plano_acao_item_id = 0;
                    List<Responsavel_Model> responsaveis;

                    foreach (var plano_acao_item in dados_itens)
                    {
                        plano_acao_item.codigo = item.id + "." + cont.ToString();

                        plano_acao_item_id = plano_acao_item.id;

                        responsaveis = _novosNegociosRepository.PlanoAcaoItensResponsavel(plano_acao_item.id);

                        plano_acao_item.responsavel = responsaveis;

                        if (responsaveis.Count == 0)
                        {
                            plano_acao_item.responsavel_nomes = "Sem Responsável";
                        }
                        else
                        {
                            plano_acao_item.responsavel_nomes = responsaveis.FirstOrDefault().nome;
                        }


                        /* Observações */
                        var observacoes = _novosNegociosRepository.QtdeObservacoes(plano_acao_item_id)
                            .Select(c => { c.qtde = long.Parse(c.qtde.ToString()); return c; }).FirstOrDefault();
                        plano_acao_item.qtde_observacoes = observacoes.qtde;

                        plano_acao_item.item_filho_check = "0";

                        // Filhos
                        List<PlanoAcaoItem_Model> dados_itens_filhos = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.DadosItensFilhos(plano_acao_item_id);
                        plano_acao_item.qtde_itens_filhos = dados_itens_filhos.Count.ToString();

                        /* Itens filhos */
                        int cont2 = 1;

                        foreach(var plano_acao_item_filho in dados_itens_filhos)
                        {
                            plano_acao_item_filho.codigo = item.id.ToString() + "." + cont2.ToString();

                            plano_acao_item_id = (int)plano_acao_item_filho.id;

                            responsaveis = (List<Responsavel_Model>)_novosNegociosRepository.PlanoAcaoItensResponsavel((int)plano_acao_item_filho.id);

                            plano_acao_item_filho.responsavel = responsaveis;

                            if (responsaveis.Count == 0)
                            {
                                plano_acao_item_filho.responsavel_nomes = "Sem Responsável";
                            }
                            else
                            {
                                plano_acao_item_filho.responsavel_nomes = responsaveis.FirstOrDefault().nome;
                            }

                            /* Observações */
                            observacoes = _novosNegociosRepository.QtdeObservacoes((long)plano_acao_item_id)
                                .Select(c => { c.qtde = long.Parse(c.qtde.ToString()); return c; }).FirstOrDefault();
                            plano_acao_item_filho.qtde_observacoes = observacoes.qtde;

                            cont2++;
                        }

                        plano_acao_item.itens_filhos = dados_itens_filhos;

                        /* Fim Itens filhos */

                        cont++;
                    }

                    item.itens = dados_itens;
                }
                else
                {
                    item.itens = new List<dynamic>();
                }

                item.hora_inicial = item.hora_inicial?.ToString();
                item.hora_final = item.hora_final?.ToString();

            }

            await this.plano_acao_geral_download_word(_planosAcao);

            var obj = new
            {
                quantidade_registros = _planosAcao.ToList().Count,
                download_ata = _caminho + @"plano-acao-ata.zip",
                relatorio = _planosAcao
            };

            return Ok(obj);
        }

        [HttpGet]
        [Route("plano-de-acao-relatorio-mes")]
        public IActionResult plano_acao_relatorio_mes(int? mes)
        {
            var dados = (List<dynamic>)_novosNegociosRepository.PlanoAcaoRelatorioMes(mes);

            foreach(var item in dados)
            {
                item.hora_inicial = item.hora_inicial.ToString();
                item.hora_final = item.hora_final.ToString();
            }

            
            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }
            else
            {
                return Ok(new
                {
                    quantidade_registros = dados.Count,
                    relatorio = dados
                });
            }
        }



        [HttpGet]
        [Route("plano-de-acao-relatorio-download")]
        public IActionResult DownloadPlanoAcao()
        {
            string arquivoZip = _caminho + "plano-acao-ata.zip";
            //converte arquivos em um array de bytes
            var dataBytes = System.IO.File.ReadAllBytes(arquivoZip);
            //adiciona bytes ao memory stream
            var dataStream = new MemoryStream(dataBytes);

            FileInfo planoZip = new FileInfo(arquivoZip);
            if (planoZip.Exists)
            {
                Response.Headers.Add("Content-type", "application/zip");
                Response.Headers.Add("Content-Disposition", "attachment; Filename="
                    + arquivoZip);

                byte[] input = dataBytes;
                return File(input, System.Net.Mime.MediaTypeNames.Application.Octet, arquivoZip);
            }
            else
            {
                return Ok(new
                {
                    mensagem = "Não há dados."
                });
            }                
        }

        [HttpGet]
        [Route("plano-de-acao-itens-observacao-relatorio")]
        public IActionResult PlanoAcaoItensObservacaoRelatorio(int item)
        {
            List<dynamic> _planoAcaoItensObservacao = (List<dynamic>)_novosNegociosRepository.PlanoAcaoItensObservacao(item);

            return Ok(new
            {
                quantidade_registros = _planoAcaoItensObservacao.Count,
                relatorio = _planoAcaoItensObservacao
            });
        }

        [HttpGet]
        [Route("plano-de-acao-itens-observacao-dados")]
        public IActionResult plano_acao_itens_observacao_dados(long idObservacao)
        {
            List<PlanoAcaoItemObservacao> dados = (List<PlanoAcaoItemObservacao>)_novosNegociosRepository.dados_observacao(idObservacao);

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }

            return Ok(dados);
        }

        [HttpPost]
        [Route("plano-de-acao-itens-observacao-cadastrar")]
        public IActionResult PlanoAcaoItensObservacaoGravar()
        {
            var request = HttpContext.Request;
            var stream = new StreamReader(request.Body);
            var body = stream.ReadToEndAsync();

            var json = body.Result;

            PlanoAcaoItemObservacao jsonConvert = JsonConvert.DeserializeObject<PlanoAcaoItemObservacao>(json);

            List<string> erros = new List<string>();

            if (jsonConvert.descricao == null)
            {
                erros.Add("Por favor insira a descrição");
            }

            if (erros.Count == 0)
            {
                var plano_acao = jsonConvert.plano_acao;
                int? plano_acao_item = (int?)jsonConvert.plano_acao_item;
                var data_prazo = jsonConvert.data_prazo;
                var descricao = jsonConvert.descricao;
                
                var usuario_email = jsonConvert.usuario_email;
                jsonConvert.usuario_nome = jsonConvert.usuario_email
                    .Substring(0, jsonConvert.usuario_email.IndexOf("@"))
                    .Replace('.', ' ');
                jsonConvert.usuario = _novosNegociosRepository
                    .usuario_Id(jsonConvert.usuario_email).FirstOrDefault().id;

                string usuario_nome = jsonConvert.usuario_nome;

                int id = 0;

                bool pendentes_aprovacao = false;

                if (jsonConvert.id == null)
                {
                    /* Verfiica se há alguma solicitação de prazo pendente, e se houver atualiza com a nova solicitação */
                    if (jsonConvert.data_prazo != null)
                    {
                        List<dynamic> solicitacao_nao_aprovada = (List<dynamic>)_novosNegociosRepository.SolicitacaoNaoAprovada(jsonConvert.plano_acao_item);

                        if (solicitacao_nao_aprovada.Count > 0)
                        {
                            pendentes_aprovacao = true;
                            id = solicitacao_nao_aprovada.FirstOrDefault().id;
                            jsonConvert.id = id;
                            jsonConvert.data_alteracao = DateTime.Now;

                            _novosNegociosRepository.PlanoAcaoItensObservacaoAtualizar(jsonConvert);
                        }
                    }

                    if (pendentes_aprovacao == false)
                    {
                        jsonConvert.data_cadastro = DateTime.Now;
                        id = _novosNegociosRepository.PlanoAcaoItensObservacaoGravar(jsonConvert);
                    }

                    /* Avisar os responsáveis sobre um nova data de prazo */
                    if (jsonConvert.data_prazo != null)
                    {
                        string data_prazo_solicitacao = jsonConvert.data_prazo?.ToString("dd/MM/yyyy");

                        var dados_plano_acao = _novosNegociosRepository.DadosPlanoAcao(jsonConvert.plano_acao);
                        var plano_acao_codigo = dados_plano_acao.FirstOrDefault().id;
                        var plano_acao_descricao = dados_plano_acao.FirstOrDefault().descricao;
                        var plano_acao_marca = _novosNegociosRepository.MarcaNome((int)dados_plano_acao.FirstOrDefault().marca).FirstOrDefault().nome;
                        var plano_acao_data_reuniao = dados_plano_acao.FirstOrDefault().data_reuniao.ToString("dd/MM/yyyy");

                        /* Dados Plano de Ação Item */
                        List<PlanoAcaoItem_Model> codigos_plano_acao_item = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.codigos_plano_acao_item(id);

                        List<PlanoAcaoItem_Model> dados_plano_acao_item = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.dados_plano_acao_item(id);

                        var plano_acao_item_codigo = dados_plano_acao_item.FirstOrDefault().id;
                        var plano_acao_item_data_prazo = dados_plano_acao_item.FirstOrDefault().data_prazo?.ToString("dd/MM/yyyy");

                        DateTime data_prazo_atual = (DateTime)dados_plano_acao_item.FirstOrDefault().data_prazo;

                        string dias_corridos = "";
                        string cor_dias_corridos = "";

                        if (jsonConvert.data_prazo != null && data_prazo_atual != null)
                        {
                            int idias_corridos = (int)Convert.ToDateTime(jsonConvert.data_prazo).Subtract(data_prazo_atual).TotalDays;

                            cor_dias_corridos = (idias_corridos < 0) ? "red" : "blue";

                            if (idias_corridos > 0)
                            {
                                dias_corridos = "+" + dias_corridos.ToString();
                            }

                            if (idias_corridos < 0)
                            {
                                dias_corridos = "-" + dias_corridos.ToString();
                            }

                            dias_corridos = @"
                                <p>
									<strong>Diferença de dias (Prazo Atual - Prazo Solicitado):</strong> 
                                    <strong style='color: " + cor_dias_corridos + @"'> " + dias_corridos + @" </strong> (dias corridos) 
								</p>";
                        }

                        var plano_acao_item_codigo_item = _novosNegociosRepository.codigo_item((List<PlanoAcaoItem_Model>)codigos_plano_acao_item, plano_acao_item, (int)plano_acao_item_codigo);

                        var emails_arr = new List<string>();
                        // Teste - alterar
                        emails_arr.Add("bruno.oyamada@connectparts.com.br");
                        emails_arr.Add("lucas.costa@connectparts.com.br");

                        string assunto = "Novos Negócios - Solicitação de alteração de prazo (" + usuario_nome + " - " + usuario_email + ")";

                        string mensagem = @"
                        <div style='font-family: arial;'>
								Foi solicitado uma alteração de data para o Plano de Ação <strong>" + plano_acao_codigo + @"</strong>. <br /> <br />

								<strong>Solicitante:</strong> " + usuario_nome + @" (" + usuario_email + @") <br />
								<strong>Novo Prazo:</strong> <strong style='color: blue;'>" + data_prazo_solicitacao + @"</strong> <br /> <br />

								<strong> PLANO DE AÇÃO </strong>
								<ul>
									<li> <strong>Código:</strong> " + plano_acao_codigo + @" </li>
									<li> <strong>Marca:</strong> " + plano_acao_marca + @" </li>
									<li> <strong>Descrição:</strong> " + plano_acao_descricao + @" </li>
									<li> <strong>Data da Reunião:</strong> " + plano_acao_data_reuniao + @" </li>
								</ul>
                                
                                <strong> PLANO DE AÇÃO ITEM </strong>
								<ul>
									<li> <strong>Código:</strong> " + plano_acao_item_codigo_item + @" </li>
									<li> <strong>Descrição:</strong> " + plano_acao_descricao + @" </li>
									<li> <strong>Data Prazo Atual:</strong> <strong style='color: red;'>" + plano_acao_item_data_prazo + @"</strong> </li>
								</ul>

								" + dias_corridos + @"

								Para aprovar ou reprovar essa solicitação, acesse a lista de Aprovação de Datas de Prazo. <br /> <br />
								<em>E-mail enviado automaticamente, favor não responder.</em>
							</div>
                    ";

                        this.enviar_email(assunto, emails_arr, mensagem);
                    }
                }
                else
                {
                    jsonConvert.data_alteracao = DateTime.Now;
                    _novosNegociosRepository.PlanoAcaoItensObservacaoAtualizar(jsonConvert);
                }

                return Ok(new
                {
                    erros = false,
                    mensagem = "Informações cadastradas com Sucesso",
                    id = id
                });
            } 
            else
            {
                return BadRequest(new
                {
                    erros = true,
                    mensagem = erros,
                    id = ""
                });
            }

        }

        [HttpPost]
        [Route("TesteEmail")]
        public async Task<dynamic> enviar_email(string titulo, List<string> emails_arr, string mensagem)
        {
            var client = new RestClient("http://integra02.connectparts.com.br:8032/publico/Comum/EnviarEmailComum?cache-control=no-cache&content-type=application/json");
            
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Content-Type", "application/json");
            
            var body = new {
                Chave = "ab63c911-3b40-4ec3-8876-41400c8f5904",
                NomeRemetente = titulo,
                Assunto = titulo,
                CorpoEmail = mensagem,
                Destinatarios = emails_arr,
                ContemHtml = true
            };
            //request.AddParameter("application/json", body, ParameterType.RequestBody);
            request.AddBody(body);
            RestResponse response = await client.ExecuteAsync(request);

            return response.Content;
        }

        [HttpGet]
        [Route("plano-de-acao-itens-observacao-relatorio-aprovar")]
        public IActionResult PlanoAcaoItensObservacaoRelatorioAprovar()
        {
            List<dynamic> _relatorioAprovar = (List<dynamic>)_novosNegociosRepository.PlanoAcaoItensObservacaoRelatorioAprovar();

            if (_relatorioAprovar.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }
            else
            {
                return Ok(new
                {
                    quantidade_registros = _relatorioAprovar.Count,
                    relatorio = _relatorioAprovar
                });
            }
        }

        [HttpGet]
        [Route("plano-de-acao-datas")]
        public IActionResult plano_acao_datas()
        {
            var _planoAcaoDatas = _novosNegociosRepository.PlanoAcaoDatas();

            return Ok(new
            {
                data = _planoAcaoDatas
            });
        }

        [HttpGet]
        [Route("plano-de-acao-rotina-aviso")]
        public IActionResult plano_acao_rotina_aviso()
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.PlanoAcaoRotinaAviso();

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Não há itens a serem enviados"
                });
            }

            if (dados.Count > 0)
            {
                string itens = "";

                foreach(var item in dados)
                {
                    string plano_acao_item = item.id.ToString();
                    string plano_acao = item.plano_acao.ToString();
                    string descricao = item.descricao;
                    var responsavel_arr = _novosNegociosRepository.PlanoAcaoItensResponsavel(item.id);
                    string data_prazo = item.data_prazo.ToString("dd/MM/yyyy");

                    List<string> responsavel = new List<string>();

                    foreach(var resp in responsavel_arr)
                    {
                        responsavel.Add(resp.nome.ToString() + " (" + resp.email.ToString() + ")");
                    }

                    string responsaveis = "";
                    foreach(var nome in responsavel)
                    {
                        if (responsaveis != "")
                        {
                            responsaveis += ", ";
                        }
                        responsaveis += nome;
                    }

                    List<PlanoAcaoItem_Model> plano_acao_item_arr = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.PlanosAcaoItens(item.plano_acao);

                    var codigo_item = _novosNegociosRepository.codigo_item(plano_acao_item_arr, item.plano_acao, item.id);

                    itens += @"
						<strong>Código:</strong> "+codigo_item+@" <br />
						<strong>Descrição:</strong> "+descricao+@" <br />
						<strong>Responsável:</strong> "+ responsaveis+@" <br />
						<strong>Data Prazo:</strong> "+data_prazo+@" <br /> <br />
					";

                    List<string> emails_arr = new List<string>();
                    emails_arr.Add("bruno.oyamada@connectparts.com.br");

                    string assunto = "Novos Negócios - Aviso - Lista de Itens (Plano de Ação) que não foram cumpridos no prazo";

                    string mensagem = @"
						<div style='font-family: arial;'>
							Lista de Itens (Plano de Ação) que não foram cumpridos no prazo. <br /> <br />

							"+itens+@"

							<em>E-mail enviado automaticamente, favor não responder.</em>
						</div>
					";

                    this.enviar_email(assunto, emails_arr, mensagem);
                }

            }

            return Ok(new
            {
                status = "E-mail de aviso enviado com Sucesso!"
            });
        }

        [HttpGet]
        [Route("plano-de-acao-item-rotina-aviso")]
        public IActionResult plano_acao_item_rotina_aviso()
        {
            List<dynamic> plano_acao_itens = (List<dynamic>)_novosNegociosRepository.PlanoAcaoItemRotinaAviso();

            if (plano_acao_itens.Count == 0)
            {
                return Ok(new
                {
                    status = "Não há emails para serem enviados."
                });
            }

            if (plano_acao_itens.Count > 0)
            {
                foreach(var plano_acao_item in plano_acao_itens)
                {
                    List<string> emails_arr = new List<string>();
                    string data_prazo = plano_acao_item.data_prazo.ToString();
                    string qtde_dias_fim_prazo = plano_acao_item.qtde_dias_fim_prazo.ToString();

                    var responsavel_arr = _novosNegociosRepository.PlanoAcaoItensResponsavel(plano_acao_item.id);

                    foreach(var resp in responsavel_arr)
                    {
                        emails_arr.Add(resp.email.ToString());
                    }
                    emails_arr.Add("bruno.oyamada@connectparts.com.br");

                    string titulo = "Novos Negócios - Aviso do Plano de Ação";
                    string mensagem = @"
						<div style='font-family: arial;'>
							Foi gerada uma ação no seu nome e você tem até o dia <strong>"+data_prazo+@"</strong> para tratá-la e finalizá-la. <br /> 
							Qualquer dúvida, favor procurar Novos Negócios ou o Processos. 
							<br /> <br /> 
							<strong style='color: red;'>Quantidade de dias para o fim do prazo: "+qtde_dias_fim_prazo+@"</strong>
							<br /> <br />
							<em>E-mail enviado automaticamente, favor não responder.</em>
						</div>
					";

                    this.enviar_email(titulo, emails_arr, mensagem);
                }
            }

            return Ok(new
            {
                status = "E-mail de aviso enviado com Sucesso!"
            });
        }


        [HttpGet]
        [Route("agenda-relatorio-calendario")]
        public IActionResult agenda_relatorio_calendario()
        {
            var _agenda_relatorio_calendario = (List<dynamic>)_novosNegociosRepository.AgendaRelatorioCalendario();

            return Ok(new
            {
                quantidade_registros = _agenda_relatorio_calendario.Count,
                relatorio = _agenda_relatorio_calendario
            });
        }

        [HttpGet]
        [Route("agenda-invites")]
        public IActionResult agenda_invites(DateTime? inicio, DateTime? fim)
        {
            List<dynamic> _agenda_invites = (List<dynamic>)_novosNegociosRepository.AgendaInvites(inicio, fim);

            if (_agenda_invites.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }
            else
            {
                return Ok(new
                {
                    quantidade_registros = _agenda_invites.Count,
                    relatorio = _agenda_invites
                });
            }
        }


        [HttpGet]
        [Route("plano-de-acao-relatorio-usuario")]
        public IActionResult plano_acao_relatorio_usuario(string usuario)
        {
            List<dynamic> _relatorioUsuario = (List<dynamic>) _novosNegociosRepository.PlanoAcaoQtdeUsuarioEfetividade(usuario);
            List<dynamic> _relatorio = (List<dynamic>)_novosNegociosRepository.RetornoPlanoAcaoItens(usuario);


            return Ok(new
            {
                concluidos_no_prazo = _relatorioUsuario.Where(x => x.efetividade == 1).Select(x => x.qtde = x.qtde).FirstOrDefault() ?? 0,
                concluidos_fora_prazo = _relatorioUsuario.Where(x => x.efetividade == 2).Select(x => x.qtde = x.qtde).FirstOrDefault() ?? 0,
                em_andamento = _relatorioUsuario.Where(x => x.efetividade == 0).Select(x => x.qtde = x.qtde).FirstOrDefault() ?? 0,
                quantidade_registros = _relatorio.Count,
                relatorio = _relatorio
            });
        }

        [HttpGet]
        [Route("agenda-relatorio")]
        public IActionResult AgendaRelatorio(DateTime? dataAgenda, string responsavel)
        {
            List<dynamic> _agendaRelatorio = (List<dynamic>)_novosNegociosRepository.AgendaRelatorio(dataAgenda, responsavel);


            return Ok(new
            {
                quantidade_registros = _agendaRelatorio.Count,
                relatorio = _agendaRelatorio
            });
        }

        [HttpGet]
        [Route("agenda-rotina-aviso")]
        public IActionResult agenda_rotina_aviso()
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.AgendaRotinaAviso();

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Não há agendas para esta data."
                });
            }

            List<string> emails_arr = new List<string>();
            /* Aviso de e-mail */
            if (dados.Count > 0)
            {
                string itens = "";
                int qtde_agenda = dados.Count;
                string titulo = "";
                string mensagem = "";

                foreach (var item in dados)
                {
                    string fornecedor = item.fornecedor_descricao;
                    string assunto = item.assunto;
                    string local = item.local;
                    string hora_inicial = item.hora_inicial.ToString();
                    string hora_final = item.hora_final.ToString();
                    string data_reuniao = item.data_reuniao.ToString();

                    List<dynamic> itens_realizados_arr = new List<dynamic>();
                    List<dynamic> itens_nao_realizados_arr = new List<dynamic>();

                    if (item.compras == 1)
                    {
                        itens_realizados_arr.Add("Compras: <span style='color: blue;'>OK</span>");
                    }
                    else
                    {
                        itens_nao_realizados_arr.Add("Compras: <span style='color: red;'>Não Realizado</span>");
                    }
                    if (item.vendas == 1)
                    {
                        itens_realizados_arr.Add("Vendas: <span style='color: blue;'>OK</span>");
                    }
                    else
                    {
                        itens_nao_realizados_arr.Add("Vendas: <span style='color:red;'>Não Realizado</span>");
                    }
                    if (item.exorbitante == 1)
                    {
                        itens_realizados_arr.Add("Novos Negócios: <span style='color: blue;'>OK</span>");
                    }
                    else
                    {
                        itens_nao_realizados_arr.Add("Novos Negócios: <span style='color:red;'>Não Realizado</span>");
                    }
                    if (item.recebimento == 1)
                    {
                        itens_realizados_arr.Add("Recebimento: <span style='color: blue;'>OK</span>");
                    }
                    else
                    {
                        itens_nao_realizados_arr.Add("Recebimento: <span style='color:red;'>Não Realizado</span>");
                    }
                    if (item.juridico == 1)
                    {
                        itens_realizados_arr.Add("Jurídico: <span style='color: blue;'>OK</span>");
                    }
                    else
                    {
                        itens_nao_realizados_arr.Add("Jurídico: <span style='color:red;'>Não Realizado</span>");
                    }
                    if (item.financeiro == 1)
                    {
                        itens_realizados_arr.Add("Financeiro: <span style='color: blue;'>OK</span>");
                    }
                    else
                    {
                        itens_nao_realizados_arr.Add("Financeiro: <span style='color:red;'>Não Realizado</span>");
                    }
                    if (item.trocas == 1)
                    {
                        itens_realizados_arr.Add("Trocas: <span style='color: blue;'>OK</span>");
                    }
                    else
                    {
                        itens_nao_realizados_arr.Add("Trocas: <span style='color:red;'>Não Realizado</span>");
                    }
                    if (item.garantia == 1)
                    {
                        itens_realizados_arr.Add("Garantia: <span style='color: blue;'>OK</span>");
                    }
                    else
                    {
                        itens_nao_realizados_arr.Add("Garantia: <span style='color:red;'>Não Realizado</span>");
                    }
                    if (item.marketing == 1)
                    {
                        itens_realizados_arr.Add("Marketing: <span style='color: blue;'>OK</span>");
                    }
                    else
                    {
                        itens_nao_realizados_arr.Add("Marketing: <span style='color: red;'>Não Realizado</span>");
                    }

                    string itens_realizados = String.Join("<br /> &nbsp; &nbsp; &nbsp; ", itens_realizados_arr);
                    string itens_nao_realizados = String.Join("<br /> &nbsp; &nbsp; &nbsp; ", itens_nao_realizados_arr);
                    

                    itens += @"
						<strong>Data Reunião:</strong> "+data_reuniao+@" das "+hora_inicial+@" às "+hora_final+@" horas <br />
						<strong>Marca:</strong> "+fornecedor+@" <br />
						<strong>Assunto:</strong> "+assunto+@" <br />
						<strong>Local:</strong> "+local+@" <br /> <br />
						<strong>Itens já Realizados:</strong> <br />
						&nbsp; &nbsp; &nbsp; "+itens_realizados+@" <br /> <br />
						<strong>Itens não Realizados:</strong> <br />
						&nbsp; &nbsp; &nbsp; "+itens_nao_realizados+@" <br />
						<br /> 
						<hr />
						<br />
					";

                    List<dynamic> responsavel_arr = JsonConvert.DeserializeObject<List<dynamic>>(item.para);
                    long qtde_dias = item.qtde_dias;

                    if (responsavel_arr.Count > 0 && qtde_dias <= 5)
                    {
                        emails_arr = new List<string>();

                        foreach(var email in responsavel_arr)
                        {
                            emails_arr.Add(email.email.ToString());
                        }
                        // Teste
                        emails_arr.Add("bruno.oyamada@connectparts.com.br");

                        if (emails_arr.Count > 0)
                        {
                            titulo = $"Reunião: {data_reuniao} das {hora_inicial} às {hora_final} horas - {fornecedor} - Novos Negócios";

                            mensagem = @"
								<strong>Data Reunião:</strong> "+data_reuniao+@" das "+hora_inicial+@" às "+hora_final+@" horas <br />
								<strong>Marca:</strong> "+fornecedor+@" <br />
								<strong>Assunto:</strong> "+assunto+@" <br />
								<strong>Local:</strong> "+local+@" <br />
								<strong>Quantidade de dias para a Reunião:</strong> "+qtde_dias+@" dia(s)
								<br /> <br />
								<em>E-mail enviado automaticamente. <br /> SIGECO </em>
							";

                            this.enviar_email(titulo, emails_arr, mensagem);
                        }
                    }
                }

                emails_arr = new List<string>();
                emails_arr.Add("bruno.oyamada@connectparts.com.br");

                titulo = "Agenda com itens a serem realizados (Envio de Documentos) - Novos Negócios";

                mensagem = @"
					<div style='font-family: arial;'>

						<strong style='color: red;'>Agenda com itens a serem realizados (Envio de Documentos)</strong> <br /> <br />

						<strong>Quantidade de Agendas:</strong> "+qtde_agenda+@" <br /> <br />

						"+itens+@"

						<em>E-mail enviado automaticamente. <br /> SIGECO </em>

					</div>
				";

                this.enviar_email(titulo, emails_arr, mensagem);
            }

            return Ok(new
            {
                status = "E-mail de aviso enviado com Sucesso!"
            });
        }

        [HttpGet]
        [Route("agenda-rotina-aviso-dashboard")]
        public IActionResult agenda_rotina_aviso_dashboard()
        {
            dynamic a_dashboard_atrasados = (dynamic)this.dashboard_atrasados();
            dynamic a_dashboard_plano_acao = (dynamic)this.dashboard_plano_acao();
            dynamic a_dashboard_responsaveis = (dynamic)this.dashboard_responsaveis();

            var dashboard_atrasados = a_dashboard_atrasados.Value;
            var dashboard_plano_acao = a_dashboard_plano_acao.Value;
            var dashboard_responsaveis = a_dashboard_responsaveis.Value;


            /* Status Geral das Ações */
            string tabela_plano_acao = @"
				<table style='background-color: #eeeeee; padding: 6px; width: 800px; font-family: arial !important;'>
					<thead>
						<tr>
							<th colspan='3' style='text-transform: uppercase; padding: 8px;'> Status Geral das Ações </th>
						</tr>
						<tr>
							<th style='padding: 6px 10px;'>Total de Ações</th>
							<th style='padding: 6px 10px;'>Quantidade</th>
							<th style='padding: 6px 10px;'>Porcentagem</th>
						</tr>
					</thead>
					<tbody>
						<tr style='background-color: #ffe59a;'>
							<td style='padding-left: 10px;'>Ações em Andamento</td>
							<td style='text-align: center; padding: 6px'>"+dashboard_plano_acao.tabela.valores.em_andamento+@"</td>
                            <td style = 'text-align: center; padding: 6px'> "+dashboard_plano_acao.tabela.porcentagem.em_andamento+@" </td >
                        </tr>
     
                        <tr style = 'background-color: #dd7e6a;'>
                            <td style = 'padding-left: 10px;' > Ações em Atraso</ td >
                            <td style = 'text-align: center; padding: 6px'> "+dashboard_plano_acao.tabela.valores.em_atraso+@"</td>
                            <td style = 'text-align: center; padding: 6px'> "+dashboard_plano_acao.tabela.porcentagem.em_atraso+@"</td>
                        </tr>
                        <tr style = 'background-color: #b6d7a8;'>
                            <td style = 'padding-left: 10px;' > Ações Concluídas no Prazo </td>
                            <td style = 'text-align: center; padding: 6px'> "+dashboard_plano_acao.tabela.valores.concluidas_no_prazo+@" </td>
                            <td style = 'text-align: center; padding: 6px'> "+dashboard_plano_acao.tabela.porcentagem.concluidas_no_prazo+@" </td>
                        </tr>
                        <tr style = 'background-color: #ebd1dc;'>
                            <td style = 'padding-left: 10px;' > Ações Concluídas com Atraso </td>
                            <td style = 'text-align: center; padding: 6px'> "+dashboard_plano_acao.tabela.valores.concluidas_com_atraso+@" </td>
                            <td style = 'text-align: center; padding: 6px;'> "+dashboard_plano_acao.tabela.porcentagem.concluidas_com_atraso+@" </td>
                        </tr>                        
                        <tr style = 'background-color: #76a5af;'>
                            <td style = 'padding-left: 10px;'> Ações Aguardando Data</td>
                            <td style = 'text-align: center; padding: 6px'> "+dashboard_plano_acao.tabela.valores.aguardando_data+@" </td>
                            <td style = 'text-align: center; padding: 6px'> "+dashboard_plano_acao.tabela.porcentagem.aguardando_data+@" </td>
                        </tr>
                    </tbody>
                    <tfoot>
                        <tr>
                            <th style = 'padding: 10px;'> Total </th>
                            <th> "+dashboard_plano_acao.tabela.valores.total+@" </th>
                            <th> "+dashboard_plano_acao.tabela.porcentagem.total+@" </th>
                        </tr>
                    </tfoot>
                </table>
            ";

            /* Ações em Atraso */
            string trs_atrasados = "";

            if (dashboard_atrasados.quantidade_registros == 0)
            {
                trs_atrasados += @"
						<tr>
							<td style='padding-left: 10px; text-align: center;' colspan='2'> Não existem ações em atraso no momento. </td>
						</tr>
					";
            }
            else
            {
                for (int index = 0; index < dashboard_atrasados.relatorio.nome.Count; index++)
                {
                    trs_atrasados += @"
						<tr>
							<td style='padding-left: 10px;'>"+ dashboard_atrasados.relatorio.nome[index] + @"</td>
                            <td style='text-align: center; padding: 6px'>"+ dashboard_atrasados.relatorio.quantidade[index] + @"</td>
                        </tr>
					";
                }
            }

            string tabela_atrasados = @"
				<table style='background-color: #eeeeee; padding: 6px; width: 800px; font-family: arial !important;'>
					<thead>
						<tr>
							<th colspan='6' style='text-transform: uppercase; padding: 8px;'> Ações em Atraso </th>
						</tr>
						<tr>
							<th style='padding: 6px 10px;'>Responsável</th>
							<th style='padding: 6px 10px;'>Quantidade</th>
						</tr>
					</thead>
					<tbody>
						"+trs_atrasados+@"
					</tbody>
				</table>
			";

            /* Ações por Responsáveis */
            string trs_responsavel = "";
            foreach(var item_responsavel in dashboard_responsaveis.relatorio)
            {
                string style_strong = (item_responsavel.setor == "Total Geral") ? "style='font-weight: bold;'" : "";

                trs_responsavel += @"
					<tr "+style_strong+@">
                        <td style='padding-left: 10px;'>" + item_responsavel.setor + @"</td>
                        <td style='text-align: center; padding: 6px'>" + item_responsavel.nome + @"</td>
                        <td style='text-align: center; padding: 6px'>" + item_responsavel.em_andamento + @"</td>
                        <td style='text-align: center; padding: 6px'>" + item_responsavel.concluido + @"</td>
                        <td style='text-align: center; padding: 6px'>" + item_responsavel.atrasado + @"</td>
                        <td style='text-align: center; padding: 6px'>" + item_responsavel.total+ @"</td>
                    </tr>
                ";
            }

            string tabela_responsaveis = @"
				<table style='background-color: #eeeeee; padding: 6px; width: 800px; font-family: arial !important;'>
					<thead>
						<tr>
							<th colspan='6' style='text-transform: uppercase; padding: 8px;'> Ações por Responsáveis </th>
						</tr>
						<tr>
							<th style='padding: 6px 10px;'>Setor</th>
							<th style='padding: 6px 10px;'>Responsável</th>
							<th style='padding: 6px 10px;'>Em Andamento</th>
							<th style='padding: 6px 10px;'>Concluído</th>
							<th style='padding: 6px 10px;'>Atrasado</th>
							<th style='padding: 6px 10px;'>Total</th>
						</tr>
					</thead>
					<tbody>
						"+trs_responsavel+@"
					</tbody>
				</table>
			";

            /* Envia e-mail */
            List<string> emails_arr = new List<string>();
            emails_arr.Add("lucas.costa@connectparts.com.br");
            emails_arr.Add("bruno.oyamada@connectparts.com.br");

            string titulo = "Dashboard - Placar Geral Das Ações - Novos Negócios";

            string data_relatorio = DateTime.Now.ToString("dd/mm/yyyy HH:mm");

            string mensagem = @"
				<div style='font-family: arial !important;'>

					<strong> Dashboard - Placar Geral Das Ações </strong> <br /> <br />

					Data de Geração do Relatório: <strong>"+data_relatorio+@"</strong> <br /> <br />

					"+tabela_plano_acao+@" <br />

					"+tabela_atrasados+@" <br />

					"+tabela_responsaveis+@" <br />

					<em>E-mail enviado automaticamente. <br /> SIGECO </em>

				</div>
			";

            this.enviar_email(titulo, emails_arr, mensagem);

            return Ok(new {
                status = "E-mail de aviso enviado com Sucesso!"
            });
        }



        [HttpGet]
        [Route("plano-de-acao-dados")]
        public IActionResult plano_acao_dados(int id)
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.DadosPlanoAcao(id);
            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            } else
            {
                return Ok(dados);
            }
        }

        [HttpGet]
        [Route("plano-de-acao-enviar-email")]
        public IActionResult plano_acao_avisar_email(int idPlano, bool item)
        {
            List<PlanoAcaoItem_Model> plano_acao_itens;
            if (idPlano > 0)
            {
                if(item == true)
                {
                    plano_acao_itens = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.PlanoAcaoItemPorId(idPlano);
                }
                else
                {
                    plano_acao_itens = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.PlanoAcaoItemPorPlanoAcao(idPlano);
                }

                foreach(var plano_acao_item in plano_acao_itens)
                {
                    List<string> emails_arr = new List<string>();

                    string data_prazo = plano_acao_item.data_prazo.ToString();
                    var responsavel_arr = _novosNegociosRepository.PlanoAcaoItensResponsavel((int)plano_acao_item.id);

                    foreach(var responsavel in responsavel_arr)
                    {
                        emails_arr.Add(responsavel.email.ToString());
                    }

                    string titulo = "Novos Negócios - Aviso do Plano de Ação";

                    string mensagem = @"
                        <div style='font-family: arial;'>
							Foi gerada uma ação no seu nome e você tem até o dia <strong>"+data_prazo+@"</strong> para tratá-la e finalizá-la. <br /> 
							Qualquer dúvida, favor procurar Novos Negócios ou o Processos. <br /> <br />
							<em>E-mail enviado automaticamente, favor não responder.</em>
						</div>";

                    this.enviar_email(titulo, emails_arr, mensagem);
                }

                return Ok(new
                {
                    status = "E-mail de aviso enviado com Sucesso!"
                });
            }
            else
            {
                return BadRequest(new 
                {
                    status = "Erro ao enviar o email!"
                });
            }

            
        }

        [HttpDelete]
        [Route("plano-de-acao-itens-observacao-deletar")]
        public IActionResult plano_acao_itens_observacao_deletar(int idObservacao)
        {
            List<dynamic> data_prazo_em_aprovacao = (List<dynamic>)_novosNegociosRepository.data_prazo_em_aprovacao(idObservacao);

            if (data_prazo_em_aprovacao.Count > 0)
            {
                return BadRequest(new 
                {
                    status = "Observação pendente de aprovação de Data Prazo!"
                });
            }
            else
            {
                int retorno = _novosNegociosRepository.ExcluiPlanoAcaoItemObservacao(idObservacao);

                return Ok(new
                {
                    status = "Observação do Plano de Ação excluido com Sucesso."
                });
            }
        }

        [HttpDelete]
        [Route("plano-de-acao-itens-deletar")]
        public IActionResult plano_acao_itens_deletar(long idItem)
        {
            List<PlanoAcaoItem_Model> dados = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.PlanoAcaoItemPorId(idItem);

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhum registro para excluir.",
                    titulo = "Registro não encontrado.",
                    mensagem = "Item de Plano de Ação não encontrado."                    
                });
            }
            int plano_acao_item = (int)dados.FirstOrDefault().id;
            var plano_acao = dados.FirstOrDefault().plano_acao;
            var descricao = dados.FirstOrDefault().descricao;

            List<PlanoAcaoItem_Model> codigos_plano_acao_item = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.CodigosPlanoAcaoItem(idItem);

            string plano_acao_item_codigo_item = _novosNegociosRepository.codigo_item(codigos_plano_acao_item, plano_acao, (int)plano_acao_item);

            List<string> listaEmail = new List<string>();
            listaEmail.Add("bruno.oyamada@connectparts.com.br");
            listaEmail.Add("lucas.costa@connectparts.com.br");

            var data_exclusao = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            string assunto = $"Novos Negócios - Aviso de exclusão do Plano de Ação Item {plano_acao_item_codigo_item}";

            string mensagem = @"
					<div style='font-family: arial;'>
						Informamos que o Plano de Ação Item <strong>"+plano_acao_item_codigo_item+@"</strong> foi excluído. <br /> <br />

						<p style='color: red;'>
							Data de Exclusão: <strong> "+data_exclusao+@" </strong>
						</p>

						<strong> PLANO DE AÇÃO ITEM </strong>
						<ul>
							<li> <strong>Código:</strong> "+plano_acao_item_codigo_item+@" </li>
							<li> <strong>Descrição:</strong> "+descricao+@" </li>
						</ul>

						<br />

						<em>E-mail enviado automaticamente, favor não responder.</em>
					</div>
				";

            this.enviar_email(assunto, listaEmail, mensagem);

            _novosNegociosRepository.ExcluirPlanoAcaoItemResponsavel((int)idItem);

            _novosNegociosRepository.ExcluirPlanoAcaoItem(idItem);

            return Ok(new
            {
                status = "Interações do Plano de Ação excluido com Sucesso.",
                titulo = assunto,
                mensagem = mensagem                
            });
        }

        [HttpDelete]
        [Route("plano-de-acao-deletar")]
        public IActionResult plano_acao_deletar(long idPlano)
        {
            List<PlanoAcaoItem_Model> planoAcaoItens = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.PlanoAcaoItemPorPlanoAcao(idPlano);
            foreach(var item in planoAcaoItens)
            {
                _novosNegociosRepository.ExcluirPlanoAcaoItemResponsavel((int)item.id);
            }

            int itens = _novosNegociosRepository.ExcluirItensPorPlanoAcao(idPlano);

            int planosAcao = _novosNegociosRepository.ExcluirPlanoAcao(idPlano);

            return Ok(new
            {
                status = "Plano de Ação excluido com Sucesso."
            });
        }

        [HttpPost]
        [Route("plano-de-acao-itens-observacao-aprovar")]
        public IActionResult plano_acao_itens_observacao_aprovar()
        {
            // Dados do body
            var request = HttpContext.Request;
            var stream = new StreamReader(request.Body);
            var body = stream.ReadToEndAsync();

            var json = body.Result;

            var observacao = JsonConvert.DeserializeObject<dynamic>(json);
            // fim do body

            if (observacao == null)
            {
                return BadRequest(new 
                {
                    status = "Formulário não preenchido."
                });
            }

            if (observacao.usuario_email == null)
            {
                return Ok(new
                {
                    status = "Falta o email do usuário."
                });
            }

            long idObservacao = long.Parse(observacao.id.ToString());
            string _usuario_email = observacao.usuario_email;
            string usuario_nome = _usuario_email.Substring(0, _usuario_email.IndexOf("@"));
            usuario_nome = usuario_nome.Replace('.', ' ');
            List<dynamic> usuario_id = (List<dynamic>)_novosNegociosRepository.usuario_Id(_usuario_email);
            int usuario = usuario_id.FirstOrDefault().id;
            
            List<PlanoAcaoItemObservacao> dados_observacao = (List<PlanoAcaoItemObservacao>)_novosNegociosRepository
                .dados_observacao((long)idObservacao);
            string data_prazo_email = dados_observacao.FirstOrDefault().data_prazo?.ToString("dd/MM/yyyy");
            DateTime data_prazo = (DateTime)dados_observacao.FirstOrDefault().data_prazo;
            var plano_acao_item = dados_observacao.FirstOrDefault().plano_acao_item;

            string titulo = "Solicitação de aprovação de Data Prazo - Novos Negócios";

            List<string> listaEmails = new List<string>();
            listaEmails.Add("bruno.oyamada@connectparts.com.br");
            //listaEmails.Add("lucas.costa@connectparts.com.br");

            string status_aprovado = (observacao.aprovado == 1) ? "<strong style='color: blue;'> APROVOU </strong>" : "<strong style='color: red;'> REPROVOU </strong>";

            List<PlanoAcaoItem_Model> dados_plano_acao_item = (List<PlanoAcaoItem_Model>)_novosNegociosRepository
                .PlanoAcaoItemPorId((long)idObservacao);

            string plano_acao_codigo = "";
            string plano_acao_descricao = "";
            string plano_acao_responsavel = "";
            List<string> emails_arr = new List<string>();

            if (dados_plano_acao_item.Count > 0)
            {
                var plano_acao_id = dados_plano_acao_item.FirstOrDefault().id;
                int? plano_acao = (int?)dados_plano_acao_item.FirstOrDefault().plano_acao;
                long? plano_acao_item_pai = dados_plano_acao_item.FirstOrDefault().plano_acao_item_pai;
                plano_acao_descricao = dados_plano_acao_item.FirstOrDefault().descricao;
                dynamic plano_acao_responsavel_arr = _novosNegociosRepository.PlanoAcaoItensResponsavel((int)dados_plano_acao_item.FirstOrDefault().id);

                List<PlanoAcaoItem_Model> codigos_plano_acao_item = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.PlanoAcaoItemPorPlanoAcao((long)plano_acao);

                plano_acao_codigo = _novosNegociosRepository.codigo_item(codigos_plano_acao_item, (int)plano_acao, (int)plano_acao_id);

                plano_acao_responsavel = "";

                foreach(var responsavel in plano_acao_responsavel_arr)
                {
                    var nome = responsavel.nome;
                    //var email = responsavel.email;

                    emails_arr.Add(responsavel.email.ToString());
                    if (plano_acao_responsavel != "")
                    {
                        plano_acao_responsavel += ",";
                    }
                    plano_acao_responsavel += nome + " (" + responsavel.email.ToString() + ")";
                }
            }

            string mensagem = @"
                <div style='font-family: arial;'>
					O usuário <strong>" + usuario_nome + @"</strong> (" + _usuario_email + @"), " + status_aprovado + @" 
					a solicitação da nova data prazo <strong>" + data_prazo_email + @"</strong> para o item: <br /> <br />
					<strong>Código</strong>: " + plano_acao_codigo + @" <br />
					<strong>Descrição</strong>: " + plano_acao_descricao + @" <br />
					<strong>Responsável</strong>: " + plano_acao_responsavel + @" <br /> <br />
					<em>E-mail enviado automaticamente, favor não responder.</em>
				</div>
            ";

            this.enviar_email(titulo, emails_arr, mensagem);

            _novosNegociosRepository.AprovarPlanoAcaoItemObservacao(
                (int)observacao.aprovado, 
                usuario, 
                usuario_nome, 
                _usuario_email,
                idObservacao,
                data_prazo
            );

            return Ok(new
            {
                status = "Alteração realizada com Sucesso."
            });
        }

        [HttpGet]
        [Route("plano-de-acao-item-concluido")]
        public IActionResult plano_acao_item_concluido(long idItem)
        {
            var dados = _novosNegociosRepository.PlanoAcaoItemPorId(idItem);
            DateTime? data_prazo = dados.FirstOrDefault()?.data_prazo;

            int efetividade = 0;
            if (DateTime.Now <= data_prazo)
            {
                efetividade = 1;
            }
            else
            {
                efetividade = 2;
            }

            _novosNegociosRepository.ConcluiPlanoAcaoItem(idItem, efetividade);

            return Ok(new
            {
                status = "Plano de Ação Item alterado com Sucesso!"
            });
        }

        [HttpGet]
        [Route("plano-de-acao-itens-relatorio")]
        public IActionResult plano_acao_itens_relatorio(long idPlanoAcao)
        {
            List<PlanoAcaoItem_Model> dados = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.PlanosAcaoItens(idPlanoAcao);

            foreach (var item in dados)
            {
                long? plano_acao_item_pai = item.plano_acao_item_pai;

                if (plano_acao_item_pai > 0)
                {
                    continue;
                }

                List<Responsavel_Model> responsavel_arr = (List<Responsavel_Model>)_novosNegociosRepository.PlanoAcaoItensResponsavel((int)item.id);

                foreach(var responsavel_dados in responsavel_arr)
                {
                    string responsavel_nome = responsavel_dados.email.ToString()
                        .Substring(0, responsavel_dados.email.ToString().IndexOf("@"))
                        .Replace('.', ' ');
                }

                item.responsavel = responsavel_arr;

                /* Itens Filhos */
                int plano_acao_item_id = (int)item.id;

                List<PlanoAcaoItem_Model> dados_filhos = (List<PlanoAcaoItem_Model>)_novosNegociosRepository
                    .DadosItensFilhos(plano_acao_item_id);

                if (dados_filhos.Count > 0)
                {
                    foreach(var filho in dados_filhos)
                    {
                        responsavel_arr = (List<Responsavel_Model>)_novosNegociosRepository.PlanoAcaoItensResponsavel((int)filho.id);

                        foreach(var responsavel_dados in responsavel_arr)
                        {
                            string responsavel_nome = responsavel_dados.email.ToString()
                                .Substring(0, responsavel_dados.email.IndexOf("@"))
                                .Replace('.', ' ');
                        }

                        filho.responsavel = responsavel_arr;
                    }

                    item.itens_filhos = dados_filhos;
                }
                else
                {
                    item.itens_filhos = new List<PlanoAcaoItem_Model>();
                }
            }

            return Ok(new
            {
                quantidade_registros = dados.Count,
                relatorio = dados
            });
        }

        [HttpGet]
        [Route("email-relatorio")]
        public IActionResult email_relatorio()
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.EmailRelatorio();

            return Ok(new
            {
                quantidade_registros = dados.Count,
                relatorio = dados
            });
        }

        [HttpGet]
        [Route("agenda-datas")]
        public IActionResult agenda_datas()
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.AgendaDatas();

            return Ok(new
            {
                datas = dados
            });
        }

        [HttpPost]
        [Route("agenda-cadastrar")]
        public IActionResult agenda_gravar()
        {
            // Dados do body
            var request = HttpContext.Request;
            var stream = new StreamReader(request.Body);
            var body = stream.ReadToEndAsync();

            var json = body.Result;

            var agenda = JsonConvert.DeserializeObject<Agenda_Model>(json);
            // fim do body

            List<string> erros = new List<string>();

            if (agenda.data_reuniao == null)
            {
                erros.Add("Por favor insira a data de reunião");
            }

            if(erros.Count == 0)
            {
                string usuario_email = agenda.usuario_email;
                string usuario_nome = agenda.usuario_email.ToString()
                    .Substring(0, agenda.usuario_email.ToString().IndexOf("@"))
                    .Replace('.', ' ');
                List<dynamic> objUsuario = (List<dynamic>)_novosNegociosRepository.usuario_Id(agenda.usuario_email.ToString());
                int usuario = objUsuario.FirstOrDefault().id;

                agenda.usuario_nome = usuario_nome;
                agenda.usuario = usuario;

                if (agenda.fornecedor_novo == 1)
                {
                    agenda.fornecedor_descricao = agenda.fornecedor.ToString();
                }
                else if (agenda.fornecedor != null)
                {
                    agenda.fornecedor_descricao = _novosNegociosRepository.MarcaNome((int)agenda.fornecedor).FirstOrDefault().nome;
                }


                long id = 0;
                if (agenda.id == null)
                {
                    agenda.invite_enviado = 1;
                    agenda.data_cadastro = DateTime.Now;

                    id = (long)_novosNegociosRepository.InsereAgenda(agenda).FirstOrDefault().insert_id;
                }
                else
                {
                    agenda.data_alteracao = DateTime.Now;

                    _novosNegociosRepository.AlteraAgenda(agenda);
                    id = (long)(agenda.id != null ? agenda.id : 0);
                }

                if (id > 0)
                {
                    this.agenda_avisar_email(id);
                }

                return Ok(new
                {
                    erros = false,
                    mensagem = "Informações cadastradas com Sucesso",
                    id = id
                });
            }
            else
            {
                return Ok(new
                {
                    erros = true,
                    mensagem = erros,
                    id = ""
                });
            }
        }

        [NonAction]
        public void agenda_avisar_email(long id)
        {
            if (id > 0)
            {
                List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.AgendaPorId(id);
                
                if (dados.Count > 0)
                {
                    var emails_para_json = dados.FirstOrDefault().para;
                    var emails_avisar_json = dados.FirstOrDefault().avisar;

                    if (emails_para_json.ToString().Length > 0)
                    {
                        var fornecedor = dados.FirstOrDefault().fornecedor_descricao;
                        var local = dados.FirstOrDefault().local;
                        var assunto = dados.FirstOrDefault().assunto;
                        var data_reuniao = dados.FirstOrDefault().data_reuniao;
                        var hora_inicial = dados.FirstOrDefault().hora_inicial;
                        var hora_final = dados.FirstOrDefault().hora_final;

                        string saudacao = "";
                        if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 12)
                        {
                            saudacao = "Bom dia";
                        } 
                        else if (DateTime.Now.Hour >= 12 && DateTime.Now.Hour <= 18)
                        {
                            saudacao = "Boa tarde";
                        } 
                        else
                        {
                            saudacao = "Boa noite";
                        }

                        string titulo = $"Reunião com o fornecedor {fornecedor} no dia {data_reuniao} das {hora_inicial} às {hora_final} horas";
                        var data_anterior_reuniao = DateTime.Parse(data_reuniao).AddDays(-1);

                        /* E-mail Para */
                        string mensagem_para = @"
							<div style='font-family: arial;'>
								<strong>Data Reunião:</strong> "+data_reuniao+@" das "+hora_inicial+@" às "+hora_final+@" horas <br />
								<strong>Marca:</strong> "+fornecedor+@" <br />
								<strong>Assunto:</strong> "+assunto+@" <br />
								<strong>Local:</strong> "+local+@" 
								<br /> <br />
								"+saudacao+@",
								<br /> <br />
								Favor fazer levantamento de informações e inseri-las na sua devida pasta para a reunião com o fornecedor "+fornecedor+@" até 12:00 horas do dia "+data_anterior_reuniao+@". <br />
								Segue informações a serem levantadas por cada departamento.
								<br /> <br />

								<strong>ANALISTA DE COMPRAS</strong>
								<table style='background-color: #ccffcc; font-family: arial;' width='100%' cellpadding='3'>
									<tr>
										<td>
										 	Dias de estoque. <br />
											Levantamento de Sugestão de Compras (45 dias + LEAD). <br />
											Levantamento referente ao valor pago HOJE. <br />
											Realizar levantamento dos itens de pendências do fornecedor. <br />
											Avaliar se necessita de ajudar com relação aos follow up do fornecedor. <br />
											Negociações pendentes (pontos importantes). <br />
											Giro dos produtos. <br />
											Custo do estoque. <br />
											Bonificação Abertas / % de descontos
											<br /> <br />
											<a href='http://sgc.dakotaparts.com.br/comercial/emailConfirmacao/"+id+@"/compras' target='_blank' style='text-decoration: none;'>Marcar como Recebido!</a>
											<br /> <br />
										</td>
									</tr>
								</table>
								<br />

								<strong>ANALISTA DE VENDAS</strong>
								<table style='background-color: #ffcccc; font-family: arial;' width='100%' cellpadding='3'>
									<tr>
										<td>
										 	EXECUÇÃO DE LEVANTAMENTO <br />
											Preço, Tícket Médio, Margem, Estoque,Giro, Exorbitante (foto, descrição, preço, titulo, categoria), ML e LV <br />
											Sem Giro e Exorbitante ( Listagem dos DKs e descrição de produtos com a quantidade) <br />
											Fazer ação avaliando se o fornecedor vende ou não. <br />
											Queima do produto se exorbitante ou sem giro. (pontos importantes) <br />
											Devolução de produto após a queima. (pontos importantes) <br />
											Analisar todos os anuncios do fornecedor (LV e ML) 
											<br /> <br />
											<a href='http://sgc.dakotaparts.com.br/comercial/emailConfirmacao/"+id+@"/vendas' target='_blank' style='text-decoration: none;'>Marcar como Recebido!</a>
											<br /> <br />
										</td>
									</tr>
								</table>
								<br />

								<strong>GARANTIA</strong>
								<table style='background-color: #ccffff; font-family: arial;' width='100%' cellpadding='3'>
									<tr>
										<td>
										 	Problemas com Garantia física, pendências do fornecedor e notas em processo de negociação ou paradas no setor.
										 	<br /> <br />
											<a href='http://sgc.dakotaparts.com.br/comercial/emailConfirmacao/"+id+@"/garantia' target='_blank' style='text-decoration: none;'>Marcar como Recebido!</a>
											<br /> <br />
										</td>
									</tr>
								</table>
								<br />

								<strong>JURÍDICO</strong>
								<table style='background-color: #eeccff; font-family: arial;' width='100%' cellpadding='3'>
									<tr>
										<td>
										 	Levantamento sobre problemas jurídicos. <br />
											Levantamento sobre problemas com Ação e Procon.
											<br /> <br />
											<a href='http://sgc.dakotaparts.com.br/comercial/emailConfirmacao/"+id+@"/juridico' target='_blank' style='text-decoration: none;'>Marcar como Recebido!</a>
											<br /> <br />
										</td>
									</tr>
								</table>
								<br />

								<strong>MARKETING</strong>
								<table style='background-color: #b3ffe0; font-family: arial;' width='100%' cellpadding='3'>
									<tr>
										<td>
										 	Buscar na planilha de Briefing compartilhada pelo marketing todos os banners e e-mail markting referente ao fornecedor. <br />
											Resultados obtidos através de VPC. <br />
											Idéias de campanhas futuras. <br />
											Capanhas realizadas nos ultimos 60 dias do fornecedores. <br />
											Quais produtos estão em estúdio para fazer anúncio e motivo do status. <br />
											<br /> <br />
											<a href='http://sgc.dakotaparts.com.br/comercial/emailConfirmacao/"+id+@"/marketing' target='_blank' style='text-decoration: none;'>Marcar como Recebido!</a>
											<br /> <br />
										</td>
									</tr>
								</table>
								<br />

								<strong>TROCAS</strong>
								<table style='background-color: #b3e6ff; font-family: arial;' width='100%' cellpadding='3'>
									<tr>
										<td>
										 	Produtos com trocas efetuadas. Qual o motivo das trocas e modelo do produto? <br />
											Relatórios dos últimos 60 dias para todos os fornecedores. <br />
											As trocas estão sendo realizadas conforme combinado com o Fornecedor?
											<br /> <br />
											<a href='http://sgc.dakotaparts.com.br/comercial/emailConfirmacao/"+id+@"/trocas' target='_blank' style='text-decoration: none;'>Marcar como Recebido!</a>
											<br /> <br />
										</td>
									</tr>
								</table>
								<br />

								<strong>RECEBIMENTOS</strong>
								<table style='background-color: #ccccff; font-family: arial;' width='100%' cellpadding='3'>
									<tr>
										<td>
										 	Listar as divergências ocorridas nos últimos meses relacionado a este Fornecedor.
										 	<br /> <br />
											<a href='http://sgc.dakotaparts.com.br/comercial/emailConfirmacao/"+id+@"/recebimento' target='_blank' style='text-decoration: none;'>Marcar como Recebido!</a>
											<br /> <br />
										</td>
									</tr>
								</table>
								<br />

								<strong>FINANCEIRO</strong>
								<table style='background-color: #ffddcc; font-family: arial;' width='100%' cellpadding='3'>
									<tr>
										<td>
										 	Análise NCM.
											Problemas com Garantia Financeira, pendências com relação as Notas de Garantia. Apresentar a data, número da nota, valores em aberto e status do processo.                                                             
										 	<br /> <br />
											<a href='http://sgc.dakotaparts.com.br/comercial/emailConfirmacao/"+id+@"/financeiro' target='_blank' style='text-decoration: none;'>Marcar como Recebido!</a>
											<br /> <br />
										</td>
									</tr>
								</table>
								<br />

								<strong>NOVOS NEGÓCIOS</strong>
								<table style='background-color: #ffffcc; font-family: arial;' width='100%' cellpadding='3'>
									<tr>
										<td>
										 	<br /> <br />
											<a href='http://sgc.dakotaparts.com.br/comercial/emailConfirmacao/"+id+@"/exorbitante' target='_blank' style='text-decoration: none;'>Marcar como Recebido!</a>
											<br /> <br />
										</td>
									</tr>
								</table>
								<br />

								<em>E-mail enviado automaticamente. <br /> SIGECO </em>

							</div>
						";

                        var emails_para_arr = JsonConvert.DeserializeObject<dynamic>(emails_para_json);
                        List<string> emails_para = new List<string>();

                        foreach(var item in emails_para_arr)
                        {
                            emails_para.Add(item.email.ToString());
                        }

                        this.enviar_email(titulo, emails_para, mensagem_para);

                        /* E-mail Avisar */

                        if (emails_avisar_json.ToString().Length > 0 && emails_avisar_json.ToString() != "[]")
                        {
                            List<dynamic> emails_avisar_arr = JsonConvert.DeserializeObject<dynamic>(emails_avisar_json);
                            List<string> emails_avisar = new List<string>();

                            if (emails_avisar_arr.Count > 0)
                            {
                                string mensagem_avisar = @"
									<div style='font-family: arial;'>
										"+saudacao+@", <br />
										Haverá reunião no dia <strong>"+data_reuniao+@"</strong> às <strong>"+hora_inicial+@"</strong> até <strong>"+hora_final+@"</strong> horas com o fornecedor <strong>"+fornecedor+@"</strong>.
										<br /> <br />
										<strong>Data Reunião:</strong> "+data_reuniao+@" das "+hora_inicial+@" às "+hora_final+@" horas <br />
										<strong>Marca:</strong> "+fornecedor+@" <br />
										<strong>Assunto:</strong> "+assunto+@" <br />
										<strong>Local:</strong> "+local+@" 
										<br /> <br />
										<em>E-mail enviado automaticamente. Favor não responder. <br /> SIGECO </em>
									</div>
								";

                                foreach(var dado in emails_avisar_arr)
                                {
                                    emails_avisar.Add(dado.email.ToString());
                                }

                                this.enviar_email(titulo, emails_avisar, mensagem_avisar);
                            }
                        }
                    }
                }
            }
        }

        [HttpDelete]
        [Route("agenda-deletar")]
        public IActionResult agenda_deletar(long idAgenda)
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.AgendaPorId(idAgenda);

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Agenda não localizada."
                });
            }

            string fornecedor = dados.FirstOrDefault().fornecedor_descricao;
            string assunto = dados.FirstOrDefault().assunto;
            string local = dados.FirstOrDefault().local;
            string hora_inicial = dados.FirstOrDefault().hora_inicial?.ToString();
            string hora_final = dados.FirstOrDefault().hora_final?.ToString();
            DateTime dtDataReuniao;
            string data_reuniao = "";
            if (dados.FirstOrDefault().data_reuniao != null)
            {
                try
                {
                    dtDataReuniao = DateTime.Parse(dados.FirstOrDefault().data_reuniao);
                    data_reuniao = dtDataReuniao.ToString("dd/MM/yyyy");
                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }                
            }
            
            _novosNegociosRepository.ExcluirAgendaInteracoes(idAgenda);
            _novosNegociosRepository.ExcluirAgenda(idAgenda);

            /* Aviso de e-mail */
            string titulo = "Aviso de exclusão de Reunião (Agenda) - Novos Negócios";

            string mensagem = @"
				<div style='font-family: arial;'>

					<strong style='color: red;'>Informamos que a Reunião abaixo foi excluída!</strong> <br /> <br />

					<strong>Data Reunião:</strong> "+data_reuniao+@" das "+hora_inicial+@" às "+hora_final+@" horas <br />
					<strong>Marca:</strong> "+fornecedor+@" <br />
					<strong>Assunto:</strong> "+assunto+@" <br />
					<strong>Local:</strong> "+local+@" 
					<br /> <br />
					<em>E-mail enviado automaticamente. <br /> SIGECO </em>
				</div>
			";

            List<string> emails_arr = new List<string>();


            if (dados.FirstOrDefault().para != null)
            {
                var para_arr = JsonConvert.DeserializeObject<dynamic>(dados.FirstOrDefault().para.ToString());
                
                foreach (var email in para_arr)
                {
                    emails_arr.Add(email.email.ToString());
                }
            }

            if (dados.FirstOrDefault().avisar != null)
            {
                var avisar_arr = JsonConvert.DeserializeObject<dynamic>(dados.FirstOrDefault().avisar);

                foreach (var email in avisar_arr)
                {
                    if (emails_arr.Contains(email.email.ToString()))
                    {
                        continue;
                    }

                    emails_arr.Add(email.email.ToString());
                }
            }


            this.enviar_email(titulo, emails_arr, mensagem);

            return Ok(new
            {
                status = "Agenda excluida com Sucesso."
            });
        }

        [HttpPost]
        [Route("agenda-avaliacao")]
        public IActionResult agenda_avaliacao()
        {
            // Dados do body
            var request = HttpContext.Request;
            var stream = new StreamReader(request.Body);
            var body = stream.ReadToEndAsync();

            var json = body.Result;

            Agenda_Avaliacao_Model dados = JsonConvert.DeserializeObject<Agenda_Avaliacao_Model>(json);
            // fim do body

            List<string> erros = new List<string>();
            if (dados.agenda == null)
            {
                erros.Add("Por favor insira a agenda");
            }

            if (dados.setor == null)
            {
                erros.Add("Por favor insira o setor");
            }

            if (dados.usuario_email == null)
            {
                erros.Add("Por favor insira o usuário");
            }

            if (erros.Count == 0)
            {
                var usuario_email = dados.usuario_email;
                var usuario_nome = dados.usuario_email.ToString()
                    .Substring(0, dados.usuario_email.ToString().IndexOf("@"))
                    .Replace('.', ' ');
                var usuario = _novosNegociosRepository.usuario_Id(dados.usuario_email.ToString());

                List<dynamic> avaliacao_existente = (List<dynamic>)_novosNegociosRepository
                    .avaliacao_existente((long)dados.agenda, (int)dados.setor);

                long id = 0;
                if (avaliacao_existente.Count > 0)
                {
                    id = avaliacao_existente.FirstOrDefault().id;
                }

                if (id == 0)
                {
                    dados.data_cadastro = DateTime.Now;

                    id = (long)_novosNegociosRepository.InsereAgendaAvaliacao(dados).FirstOrDefault().insert_id;
                }
                else
                {
                    dados.data_alteracao = DateTime.Now;
                    _novosNegociosRepository.AlteraAgendaAvaliacao(dados);
                }

                return Ok(new
                {
                    erros = false,
                    mensagem = "Informações cadastradas com Sucesso",
                    id = id
                });
            }
            else
            {
                return BadRequest(new
                {
                    erros = true,
                    mensagem = erros,
                    id = 0
                });
            }
        }

        [HttpGet]
        [Route("agenda-avaliacao-item")]
        public IActionResult agenda_avaliacao_item(long idItem)
        {
            List<dynamic> avaliacao_agenda = (List<dynamic>)_novosNegociosRepository.AgendaAvaliacao(idItem);

            foreach(var item in avaliacao_agenda)
            {
                List<dynamic> nomeSetor = _novosNegociosRepository.setor_nome(item.setor);
                item.nome_setor = nomeSetor.FirstOrDefault().nome;
            }

            return Ok(avaliacao_agenda);
        }

        [HttpGet]
        [Route("agenda-avaliacao-relatorio")]
        public IActionResult agenda_avaliacao_relatorio()
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.AgendaAvaliacoes();

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }

            foreach(var item in dados)
            {
                if(item.fornecedor > 0)
                {
                    List<dynamic> marca = _novosNegociosRepository.MarcaNome(item.fornecedor);
                    item.fornecedor_descricao = marca.FirstOrDefault().nome;
                }

                var listaPara = JsonConvert.DeserializeObject<dynamic>(item.para.ToString());
                item.para = new List<dynamic>();
                foreach(var para in listaPara)
                {
                    item.para.Add(new
                    {
                        email = para.email.ToString(),
                        nome = para.nome.ToString(),
                        ticked = para.ticked.Value
                    });
                }
                var listaAvisar = JsonConvert.DeserializeObject<dynamic>(item.avisar.ToString());
                item.avisar = new List<dynamic>();
                foreach(var avisar in listaAvisar)
                {
                    item.avisar.Add(new
                    {
                        email = avisar.email.ToString(),
                        nome = avisar.nome.ToString(),
                        ticked = avisar.ticked.Value
                    });
                }

                var avaliacao_agenda = _novosNegociosRepository.AvaliacaoAgenda((int)item.id);

                item.avaliacao = new List<dynamic>();
                foreach (var avaliacao in avaliacao_agenda)
                {
                    item.avaliacao.Add(avaliacao);
                }
            }

            return Ok(new
            {
                quantidade_registros = dados.Count,
                relatorio = dados
            });
        }

        [HttpPost]
        [Route("agenda-status-setor")]
        public async Task<IActionResult> agenda_status_setor(long? agenda, int? setor, string? usuario_email, string? comentario, IFormFile? arquivo)
        {
            HttpClient client = null;
            StreamContent fileStream = null;
            try
            {

                List<string> erros = new List<string>();
                if (agenda == null)
                {
                    erros.Add("Por favor insira a agenda");
                }

                if (setor == null)
                {
                    erros.Add("Por favor insira o setor");
                }

                if (usuario_email == null)
                {
                    erros.Add("Por favor insira o usuário");
                }

                string usuario_nome = "";
                int usuario = 0;
                if (usuario_email != null)
                {
                    usuario_nome = usuario_email.ToString()
                    .Substring(0, usuario_email.ToString().IndexOf("@"))
                    .Replace('.', ' ');

                    usuario = _novosNegociosRepository.usuario_Id(usuario_email.ToString()).FirstOrDefault().id;
                }


                if (agenda > 0 && setor > 0)
                {
                    string _arquivo = arquivo != null ? _arquivosSetores + arquivo.FileName : "";
                    long id = _novosNegociosRepository.AgendaStatusSetor((long)agenda, (int)setor, comentario, usuario, usuario_email, usuario_nome, _arquivo);

                    if (id > 0 && arquivo != null)
                    {
                        
                        if (!Directory.Exists(_arquivosSetores))
                        {
                            Directory.CreateDirectory(_arquivosSetores);
                        }


                        if (arquivo.Length > 0)
                        {
                            using (var stream = System.IO.File.Create(_arquivosSetores + arquivo.FileName))
                            {
                                await arquivo.CopyToAsync(stream);
                            }
                        }

                    }
                }

                if (erros.Count == 0)
                {
                    return Ok(new
                    {
                        status = "Agenda alterada com Sucesso."
                    });
                }
                else
                {
                    string strErros = "";
                    foreach(var item in erros)
                    {
                        strErros += item.ToString();
                        if (strErros.Length > 0)
                        {
                            strErros += "<br />";
                        }
                    }

                    return Ok(new
                    {
                        erros = true,
                        mensagem = strErros
                    }); ;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
            finally
            {
                client?.Dispose();
                fileStream?.Dispose();
            }

            
        }

        [HttpGet]
        [Route("agenda-invites-interacoes")]
        public IActionResult agenda_interacoes(long idAgenda)
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.AgendaInteracoes(idAgenda);

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }
            else
            {
                return Ok(new
                {
                    quantidade_registros = dados.Count,
                    relatorio = dados
                });
            }
        }

        [HttpDelete]
        [Route("agenda-arquivos-deletar")]
        public IActionResult agenda_arquivos_deletar(long idAgenda)
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.AgendaInteracoesPorId(idAgenda);

            if (System.IO.File.Exists(dados.FirstOrDefault().arquivo))
            {
                System.IO.File.Delete(dados.FirstOrDefault().arquivo);
            }

            _novosNegociosRepository.AtualizaNomeArquivo(idAgenda, "");

            return Ok(new
            {
                status = "Arquivo Padrão excluido com Sucesso."
            });
        }

        [HttpGet]
        [Route("agenda-arquivos-download")]
        public IActionResult AgendaArquivoDownload(long idAgenda)
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.AgendaInteracoesPorId(idAgenda);

            if (dados.Count == 0)
            {
                return Ok(new {
                    status = "Arquivo inexistente."
                });
            }

            if (!System.IO.File.Exists(dados.FirstOrDefault().arquivo))
            {
                return Ok(new
                {
                    status = "Arquivo inexistente."
                });
            }

            FileStream fileStream;
            try
            {
                fileStream = System.IO.File.OpenRead(dados.FirstOrDefault().arquivo);
            }
            catch(DirectoryNotFoundException ex)
            {
                return new EmptyResult();
            }

            return File(fileStream, "application/*", dados.FirstOrDefault().arquivo);
        }

        [HttpGet]
        [Route("agenda-dados")]
        public IActionResult agenda_dados(long idAgenda)
        {
            List<dynamic> agenda = (List<dynamic>)_novosNegociosRepository.AgendaPorId(idAgenda);

            if (agenda.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }

            foreach(var item in agenda)
            {
                if(item.fornecedor != null)
                {
                    item.fornecedor_descricao = _novosNegociosRepository.MarcaNome(item.fornecedor);
                }

                var listaPara = JsonConvert.DeserializeObject<dynamic>(item.para.ToString());
                item.para = new List<dynamic>();
                foreach (var para in listaPara)
                {
                    item.para.Add(new
                    {
                        email = para.email.ToString(),
                        nome = para.nome.ToString(),
                        ticked = para.ticked.Value
                    });
                }
                var listaAvisar = JsonConvert.DeserializeObject<dynamic>(item.avisar.ToString());
                item.avisar = new List<dynamic>();
                foreach (var avisar in listaAvisar)
                {
                    item.avisar.Add(new
                    {
                        email = avisar.email.ToString(),
                        nome = avisar.nome.ToString(),
                        ticked = avisar.ticked.Value
                    });
                }
            }

            return Ok(agenda.FirstOrDefault());
        }

        [HttpGet]
        [Route("fornecedores")]
        public IActionResult Fornecedores()
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.Fornecedores();

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma fornecedor encontrado."
                });
            }
            else
            {
                return Ok(new
                {
                    quantidade_registros = dados.Count,
                    fornecedores = dados
                });
            }
        }

        [HttpGet]
        [Route("usuarios-departamento-relatorio")]
        public IActionResult usuarios_departamento_relatorio()
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.UsuariosDepartamentoRelatorio();

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }
            else
            {
                return Ok(new
                {
                    quantidade_registros = dados.Count,
                    relatorio = dados
                });
            }
        }

        [HttpPost]
        [Route("usuarios-departamento-cadastrar")]
        public IActionResult usuarios_departamento_gravar([FromBody] Usuarios_Model usuario)
        {
            List<string> erros = new List<string>();
            if (usuario.nome == null)
            {
                erros.Add("Por favor insira a agenda");
            }

            if (usuario.email == null)
            {
                erros.Add("Por favor insira o e-mail");
            }

            if(usuario.setor == null)
            {
                erros.Add("Por favor insira o setor");
            }

            //if (_novosNegociosRepository.VerificaUsuarioExistente(dados.email.ToString()))
            //{
            //    erros.Add("E-mail já cadastrado em nossa base de dados");
            //}

            if(erros.Count == 0)
            {
                usuario.setor_nome = _novosNegociosRepository.setor_nome(usuario.setor).FirstOrDefault().nome;
                
                usuario.usuario_nome = usuario.nome;
                usuario.usuario = _novosNegociosRepository.usuario_Id(usuario.email).FirstOrDefault()?.id;

                int id = 0;
                if (usuario.id == null || usuario.id == 0)
                {
                    usuario.data_cadastro = DateTime.Now;
                    id = (int)_novosNegociosRepository.UsuariosDepartamentoInserir(usuario);
                }
                else
                {
                    usuario.data_alteracao = DateTime.Now;
                    usuario.id = int.Parse(usuario.id.ToString());
                    id = _novosNegociosRepository.UsuariosDepartamentoAtualizar(usuario);
                }

                return Ok(new
                {
                    erros = false,
                    mensagem = "Informações cadastradas com Sucesso",
                    id = id
                });
            }
            else
            {
                return Ok(new
                {
                    erros = true,
                    mensagem = erros
                });
            }

        }

        [HttpDelete]
        [Route("usuarios-departamento-deletar")]
        public IActionResult usuarios_departamento_deletar(long idUsuario)
        {
            int excluidos = _novosNegociosRepository.UsuariosDepartamentoDeletar(idUsuario);

            if(excluidos == 0)
            {
                return Ok(new
                {
                    status = "Nenhum usuário para excluir."
                });
            }
            else
            {
                return Ok(new
                {
                    status = "Usuário excluido com Sucesso."
                });
            }
        }

        [HttpGet]
        [Route("email-dados")]
        public IActionResult email_dados(long idEmail)
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.EmailDados(idEmail);

            if (dados.Count == 0) {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }
            else
            {
                return Ok(dados.FirstOrDefault());
            }
        }

        [HttpDelete]
        [Route("email-deletar")]
        public IActionResult email_deletar(long idEmail)
        {
            int deletados = _novosNegociosRepository.EmailDeletar(idEmail);
            if (deletados == 0)
            {
                return Ok(new
                {
                    status = "Email não encontrado."
                });
            } else
            {
                return Ok(new
                {
                    status = "Email excluido com Sucesso."
                });
            }
        }

        [HttpPost]
        [Route("email-cadastrar")]
        public IActionResult email_gravar([FromBody] Emails_Model dados)
        {
            //dynamic dados = JsonConvert.DeserializeObject<dynamic>(json.ToString());
            // fim do body

            List<string> erros = new List<string>();
            if (dados.nome == null)
            {
                erros.Add("Por favor insira o nome");
            }

            if (dados.email == null)
            {
                erros.Add("Por favor insira o e-mail");
            }

            if (erros.Count == 0)
            {
                long id = 0;
                if (dados.id == null)
                {
                    dados.data_cadastro = DateTime.Now;
                    id = _novosNegociosRepository.EmailsInserir(dados);
                }
                else
                {
                    dados.data_alteracao = DateTime.Now;
                    _novosNegociosRepository.EmailsAtualizar(dados);
                    id = dados.id;
                }

                return Ok(new
                {
                    erros = false,
                    mensagem = "Informações cadastradas com Sucesso",
                    id = id
                });
            }
            else
            {
                return Ok(new
                {
                    erros = true,
                    mensagem = erros,
                    id = ""
                });
            }
        }

        [HttpGet]
        [Route("arquivos-padroes-relatorio")]
        public IActionResult arquivos_padroes_relatorio()
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.arquivos_padroes();

            foreach(var item in dados)
            {
                if (item.setor != null)
                {
                    List<dynamic> setores = _novosNegociosRepository.setor_nome(item.setor);
                    item.nome_setor = setores.FirstOrDefault().nome;
                }                

            }

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }
            else
            {
                return Ok(new
                {
                    quantidade_registros = dados.Count,
                    relatorio = dados
                });
            }
        }

        [HttpGet]
        [Route("novos-negocios-arquivos-padroes")]
        public IActionResult download(string arquivo)
        {
            if (! System.IO.File.Exists(_arquivosPadroes + arquivo))
            {
                return Ok(new
                {
                    status = "Nenhum arquivo encontrado."
                });
            }

            FileStream fileStream;
            try
            {
                fileStream = System.IO.File.OpenRead(_arquivosPadroes + arquivo);
            }
            catch (DirectoryNotFoundException ex)
            {
                return new EmptyResult();
            }

            return File(fileStream, "application/*", arquivo);
        }

        [HttpPost]
        [Route("arquivos-padroes-cadastrar")]
        public async Task<IActionResult> arquivos_padroes_gravar(int? idArquivo, string nome, int setor, string comentario, string usuario, IFormFile? arquivo)
        {
            List<string> erros = new List<string>();
            if (nome == null)
            {
                erros.Add("Por favor insira o nome");
            }

            if (setor == null)
            {
                erros.Add("Por favor insira o setor");
            }

            if (usuario == null)
            {
                erros.Add("Por favor insira o usuário");
            }

            if(arquivo == null)
            {
                erros.Add("Por favor insira o arquivo");
            }

            if (erros.Count == 0)
            {
                Arquivos_Padroes_Model arquivos = new Arquivos_Padroes_Model();
                arquivos.usuario_email = usuario;
                arquivos.usuario_nome = arquivos.usuario_email
                    .Substring(0, arquivos.usuario_email.IndexOf("@"))
                    .Replace('.', ' ');
                arquivos.usuario = _novosNegociosRepository
                    .usuario_Id(arquivos.usuario_email).FirstOrDefault().id;

                arquivos.nome = nome.ToString();
                arquivos.setor = setor;
                arquivos.comentario = comentario.ToString();

                // Grava arquivo na pasta
                if (arquivo.Length > 0)
                {
                    using (var stream = System.IO.File.Create(_arquivosPadroes + arquivo.FileName))
                    {
                        await arquivo.CopyToAsync(stream);
                    }
                }
                arquivos.arquivo = arquivo.FileName;

                int id = 0;
                if (idArquivo == null)
                {
                    arquivos.data_cadastro = DateTime.Now;
                    id = _novosNegociosRepository.ArquivosPadroesInserir(arquivos);
                }
                else
                {
                    arquivos.data_alteracao = DateTime.Now;
                    arquivos.id = (int)idArquivo;
                    
                    id = arquivos.id;
                    _novosNegociosRepository.ArquivosPadroesAtualizar(arquivos);
                }                

                return Ok(new
                {
                    erros = false,
                    mensagem = "Informações cadastradas com Sucesso",
                    id = id
                });
            }
            else {
                return Ok(new
                {
                    erros = true,
                    mensagem = erros
                });
            }
        }

        [HttpDelete]
        [Route("arquivos-padroes-deletar")]
        public IActionResult arquivos_padroes_deletar(int idArquivo)
        {
            Arquivos_Padroes_Model dados = (Arquivos_Padroes_Model)_novosNegociosRepository.ArquivosPadroesPorId(idArquivo);
            if(System.IO.File.Exists(_arquivosPadroes + dados.arquivo))
            {
                System.IO.File.Delete(_arquivosPadroes + dados.arquivo);
            }

            int cont = _novosNegociosRepository.ArquivosPadroesDeletar(idArquivo);

            if(cont > 0)
            {
                return Ok(new
                {
                    status = "Arquivo Padrão excluido com Sucesso."
                });
            } 
            else
            {
                return Ok(new
                {
                    status = "Não há registros para serem excluídos."
                });
            }
        }

        [HttpGet]
        [Route("arquivos-padroes-dados")]
        public IActionResult arquivos_padroes_dados(int idArquivo)
        {
            Arquivos_Padroes_Model dados = _novosNegociosRepository.ArquivosPadroesPorId(idArquivo);

            if (dados == null)
            {
                return Ok(new
                {
                    status = "Dados não encontrados."
                });
            }

            dados.nome_setor = _novosNegociosRepository.setor_nome(dados.setor).FirstOrDefault().nome;

            return Ok(dados);
        }

        [HttpGet]
        [Route("plano-de-acao-enviar-email-item")]
        public IActionResult plano_acao_item_avisar_email(int idItem, bool item = false)
        {
            if (idItem == 0)
            {
                return Ok(new
                {
                    status = "Item não encontrado!"
                });
            }

            if (idItem > 0)
            {
                List<PlanoAcaoItem_Model> plano_acao_itens;
                if (item == true)
                {
                    plano_acao_itens = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.PlanoAcaoItemPorId(idItem);
                }
                else
                {
                    plano_acao_itens = (List<PlanoAcaoItem_Model>)_novosNegociosRepository.PlanoAcaoItemPorPlanoAcao(idItem);
                }

                if (plano_acao_itens.Count == 0)
                {
                    return Ok(new
                    {
                        status = "Não há itens para serem enviados."
                    });
                }

                foreach (var plano_acao_item in plano_acao_itens)
                {
                    List<string> emails_arr = new List<string>();
                    string data_prazo = plano_acao_item.data_prazo.ToString();

                    List<Responsavel_Model> responsaveis = (List<Responsavel_Model>)_novosNegociosRepository.PlanoAcaoItensResponsavel((int)plano_acao_item.id);

                    for (int i = 0; i < responsaveis.Count; i++)
                    {
                        emails_arr.Add(responsaveis[i].email.ToString());
                    }
                    emails_arr.Add("bruno.oyamada@connectparts.com.br");


                    string titulo = "Novos Negócios - Aviso do Plano de Ação";

                    string mensagem = @"
						<div style='font-family: arial;'>
							Foi gerada uma ação no seu nome e você tem até o dia <strong>"+data_prazo+@"</strong> para tratá-la e finalizá-la. <br /> 
							Qualquer dúvida, favor procurar Novos Negócios ou o Processos. <br /> <br />
							<em>E-mail enviado automaticamente, favor não responder.</em>
						</div>
					";

                    this.enviar_email(titulo, emails_arr, mensagem);
                }
            }

            return Ok(new
            {
                status = "E-mail de aviso enviado com Sucesso!"
            });
        }

        [HttpGet]
        [Route("plano-de-acao-itens-dados")]
        public IActionResult plano_acao_itens_dados(long idItem)
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.dados_item_filho(idItem);

            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }
            else
            {
                return Ok(dados.FirstOrDefault());
            }
        }

        [HttpGet]
        [Route("dashboard-atrasados")]
        public IActionResult dashboard_atrasados()
        {
            List<dynamic> dados = (List<dynamic>)_novosNegociosRepository.dashboard_atrasados();

            List<string> emails_arr = new List<string>();
            List<string> responsavel_nome = new List<string>();
            IDictionary<string, int> quantidades = new Dictionary<string, int>();
            
            foreach (var item in dados)
            {
                List<Responsavel_Model> responsavel_arr = _novosNegociosRepository.PlanoAcaoItensResponsavel(item.id);

                if (responsavel_arr.Count == 0)
                {
                    if (! responsavel_nome.Contains("Sem responsável"))
                    {
                        responsavel_nome.Add("Sem responsável");
                        quantidades["Sem responsável"] = 1;
                    }
                    else
                    {
                        quantidades["Sem responsável"]++;
                    }
                }
                
                foreach (var responsavel in responsavel_arr)
                {
                    if(!responsavel_nome.Contains(responsavel.nome.ToString()))
                    {
                        responsavel_nome.Add(responsavel.nome.ToString());
                        quantidades[responsavel.nome.ToString()] = 1;
                    }
                    else
                    {
                        quantidades[responsavel.nome.ToString()]++;
                    }
                }
            }

            // Lista as quantidades
            List<int> listaQuantidades = new List<int>();
            foreach(var item in responsavel_nome)
            {
                listaQuantidades.Add(quantidades[item]);
            }

            
            if (dados.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }
            else
            {
                return Ok(new
                {
                    quantidade_registros = responsavel_nome.Count,
                    relatorio = new
                    {
                        nome = responsavel_nome, //responsavel_nome,
                        quantidade = listaQuantidades
                    }
                });
            }
            
        }

        [HttpGet]
        [Route("dashboard-plano-de-acoes")]
        public IActionResult dashboard_plano_acao()
        {
            long total_plano_acao = _novosNegociosRepository.QtdPlanoAcaoItem().FirstOrDefault().qtde;
            long em_andamento = _novosNegociosRepository.QtdPlanoAcaoItemEmAndamento().FirstOrDefault().qtde;
            long em_atraso = _novosNegociosRepository.QtdPlanoAcaoItemEmAtraso().FirstOrDefault().qtde;
            long concluidas_no_prazo = _novosNegociosRepository.QtdPlanoAcaoItemConcluidosNoPrazo().FirstOrDefault().qtde;
            long concluidas_com_atraso = _novosNegociosRepository.QtdPlanoAcaoItemConcluidosComAtraso().FirstOrDefault().qtde;
            long aguardando_data = _novosNegociosRepository.QtdPlanoAcaoItemAguardandoData().FirstOrDefault().qtde;

            double porc_em_andamento = Math.Round((double)(em_andamento * 100) / total_plano_acao);
            double porc_em_atraso = Math.Round((double)(em_atraso * 100) / total_plano_acao, 1);
            double porc_concluidas_no_prazo = Math.Round((double)(concluidas_no_prazo * 100) / total_plano_acao, 1);
            double porc_concluidas_com_atraso = Math.Round((double)(concluidas_com_atraso * 100) / total_plano_acao, 1);
            double porc_aguardando_data = Math.Round((double)(aguardando_data * 100) / total_plano_acao, 1);

            dynamic valores = new
            {
                total = total_plano_acao,
                em_andamento = em_andamento,
                em_atraso = em_atraso,
                concluidas_no_prazo = concluidas_no_prazo,
                concluidas_com_atraso = concluidas_com_atraso,
                aguardando_data = aguardando_data
            };

            dynamic porcentagem = new
            {
                total = "100.0%",
                em_andamento = porc_em_andamento.ToString() + "%",
                em_atraso = porc_em_atraso.ToString() + "%",
                concluidas_no_prazo = porc_concluidas_no_prazo.ToString() + "%",
                concluidas_com_atraso = porc_concluidas_com_atraso.ToString() + "%",
                aguardando_data = porc_aguardando_data.ToString() + "%"
            };

            dynamic grafico = new
            {
                total = "100.0%",
                em_andamento = porc_em_andamento,
                em_atraso = porc_em_atraso,
                concluidas_no_prazo = porc_concluidas_no_prazo,
                concluidas_com_atraso = porc_concluidas_com_atraso,
                aguardando_data = porc_aguardando_data
            };

            return Ok(new
            {
                tabela = new
                {
                    valores = valores,
                    porcentagem = porcentagem
                },
                grafico = grafico
            });
        }

        [HttpGet]
        [Route("dashboard-responsaveis")]
        public IActionResult dashboard_responsaveis()
        {
            List<dynamic> plano_acao_item = (List<dynamic>)_novosNegociosRepository.PlanoAcaoItem();

            if (plano_acao_item.Count == 0)
            {
                return Ok(new
                {
                    status = "Nenhuma informação encontrada."
                });
            }

            long total = 0;
            long total_geral = 0;

            long total_em_atraso = 0;
            long total_concluido = 0;
            long total_atrasado = 0;
            List<string> usuario_comp = new List<string>();

            List<dynamic> retorno = new List<dynamic>();

            foreach (var item in plano_acao_item)
            {
                List<Responsavel_Model> responsavel_arr = _novosNegociosRepository.PlanoAcaoItensResponsavel(item.id);

                string responsavel_email = "";
                string responsavel_nome = "";
                if (responsavel_arr.Count > 0)
                {
                    responsavel_email = responsavel_arr.FirstOrDefault().email;
                    responsavel_nome = responsavel_arr.FirstOrDefault().nome;
                }

                if (usuario_comp.Contains(responsavel_email))
                {
                    continue;
                }

                usuario_comp.Add(responsavel_email);

                long em_andamento_responsavel = _novosNegociosRepository.QtdEmAndamento(responsavel_email).FirstOrDefault().qtde;

                long concluido_responsavel = _novosNegociosRepository.QtdConcluido(responsavel_email).FirstOrDefault().qtde;

                long atrasado_responsavel = _novosNegociosRepository.QtdAtrasados(responsavel_email).FirstOrDefault().qtde;

                total = (long)(em_andamento_responsavel + concluido_responsavel + atrasado_responsavel);

                total_em_atraso += em_andamento_responsavel;
                total_concluido += concluido_responsavel;
                total_atrasado += atrasado_responsavel;

                total_geral += total;

                string setor = "";
                if (item.setor == 0)
                {
                    setor = "Sem Setor";
                }
                else
                {
                    List<dynamic> listaSetores = _novosNegociosRepository.setor_nome(item.setor);
                    setor = listaSetores.FirstOrDefault().nome;
                }

                retorno.Add(new
                {
                    setor = setor,
                    nome = responsavel_nome.Length == 0 ? "Sem Responsável" : responsavel_nome,
                    em_andamento = em_andamento_responsavel,
                    concluido = concluido_responsavel,
                    atrasado = atrasado_responsavel,
                    total = total.ToString()
                });
            }

            retorno.Add(new
            {
                setor = "Total Geral",
                nome = " - ",
                em_andamento = total_em_atraso.ToString(),
                concluido = total_concluido.ToString(),
                atrasado = total_atrasado.ToString(),
                total = total_geral.ToString()
            });

            return Ok(new
            {
                quantidade_registros = retorno.Count,
                relatorio = retorno
            });
        }

        [HttpPost]
        [Route("converte-responsavel")]
        public IActionResult ConverteResponsavel()
        {
            var planoAcaoItem = _novosNegociosRepository.PlanoAcaoItem();
            foreach(var item in planoAcaoItem)
            {
                var listaResponsavel = JsonConvert.DeserializeObject<dynamic>(item.responsavel);
                foreach(var responsavel in listaResponsavel)
                {
                    string email = responsavel.email.ToString();
                    string nome = responsavel.nome.ToString();
                    int? ticked = null;
                    if (responsavel.ticked != null)
                    {
                        if (bool.Parse(responsavel.ticked?.ToString()))
                        {
                            ticked = 1;
                        }
                        else
                        {
                            ticked = 0;
                        }
                    }

                    int? idResponsavel = _novosNegociosRepository.ResponsavelId(email, nome, ticked);

                    if (idResponsavel == null)
                    {
                        idResponsavel = _novosNegociosRepository.ResponsavelInserir(email, nome, ticked);
                    }

                    _novosNegociosRepository.PlanoAcaoItemResponsavelInserir(item.id, (int)idResponsavel);
                }
            }

            return Ok(new
            {
                status = "Inserido com sucesso."
            });
        }


    }
}
