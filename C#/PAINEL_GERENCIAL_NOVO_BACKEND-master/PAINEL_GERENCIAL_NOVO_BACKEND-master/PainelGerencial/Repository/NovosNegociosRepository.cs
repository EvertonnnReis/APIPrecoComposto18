using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PainelGerencial.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public class NovosNegociosRepository : INovosNegociosRepository
    {
        private readonly string _connectionString;
        //private readonly string _connectionStringMysqlNovosNegocios;
        private readonly string _connectionStringMysqlConnectParts;
        private readonly string _connectionStringSigeco;

        public NovosNegociosRepository(IConfiguration configuration)
        {
            //_connectionStringMysqlNovosNegocios = configuration.GetConnectionString("novosnegocios");
            _connectionString = configuration.GetConnectionString("PainelGerencialServer");
            _connectionStringMysqlConnectParts = configuration.GetConnectionString("connectparts");
            _connectionStringSigeco = configuration.GetConnectionString("Sigeco");
        }

        public IEnumerable<dynamic> Marcas()
        {
            string sql = @"SELECT MARP_COD AS id, MARP_NOM AS nome FROM TCOM_MARPRO (NOLOCK) ORDER BY MARP_NOM ASC";
            
            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }


        public IEnumerable<dynamic> UsuariosDepartamentoLista()
        {
            string sql = @"SELECT email, nome FROM usuarios (NOLOCK) WHERE ativo = 1 ORDER BY nome ASC";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql, commandType: CommandType.Text);
            }
        }

        public IEnumerable<dynamic> UsuariosDepartamentoRelatorio()
        {
            string sql = @"SELECT usuarios.id
                                 ,usuarios.nome
                                 ,usuarios.email
                                 ,usuarios.setor
                                 ,setor.nome AS setor_nome
                                 ,usuarios.data_cadastro
                                 ,usuarios.data_alteracao
                                 ,usuarios.ativo
                                 ,usuarios.usuario
                                 ,usuarios.usuario_nome
                                 ,usuarios.usuario_email 
                             FROM usuarios (NOLOCK)
                             LEFT OUTER JOIN setor
                               ON setor.id = usuarios.setor
                            ORDER BY nome ASC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql, commandType: CommandType.Text);
            }
        }


        public IEnumerable<dynamic> PlanoAcaoRelatorio(int marca, string responsavel, DateTime data_reuniao, int efetividade)
        {
            string sql = @"SELECT 
                                  plano_acao.id, fornecedor, marca, ISNULL(MARP_NOM, marca_nome) AS marca_nome, 
                                  plano_acao.data_reuniao AS data_reuniao,
                                  descricao, 
                                  plano_acao.data_cadastro AS data_cadastro, 
                                  data_alteracao AS data_alteracao, 
                                  usuario, usuario_email, usuario_nome, 
                                  hora_inicial AS hora_inicial, 
                                  hora_final AS hora_final, participantes, pauta, local,
                                  (SELECT COUNT(plano_acao_item.id) FROM plano_acao_item (NOLOCK)
                                    WHERE plano_acao_item.plano_acao = plano_acao.id) AS qtde_interacoes
                             FROM plano_acao (NOLOCK)
                             LEFT OUTER JOIN ABACOS.dbo.TCOM_MARPRO (NOLOCK)
                               ON TCOM_MARPRO.MARP_COD = plano_acao.marca
                            WHERE 1 = CASE 
                                        WHEN @marca IS NULL THEN 1
                                        WHEN marca = @marca THEN 1
                                      ELSE 0
                                      END
                              AND 1 = CASE 
                                        WHEN @responsavel IS NULL THEN 1
                                		WHEN plano_acao.id IN (SELECT DISTINCT plano_acao_item.plano_acao 
                                                                 FROM plano_acao_item (NOLOCK) 
																INNER JOIN plano_acao_item_responsavel (NOLOCK)
																   ON plano_acao_item_responsavel.plano_acao_item = plano_acao_item.id
                                                                INNER JOIN usuarios (NOLOCK)
																   ON usuarios.id = plano_acao_item_responsavel.responsavel
                                                                WHERE usuarios.email LIKE CONCAT('%', @responsavel, '%')) THEN 1
                                      ELSE 0
                                      END
                              AND 1 = CASE 
                                        WHEN @data_reuniao IS NULL THEN 1
                                		WHEN data_reuniao = @data_reuniao THEN 1
                                      ELSE 0
                                      END
                              AND 1 = CASE 
                                        WHEN @efetividade IS NULL OR @efetividade = 1 THEN 1
                                		WHEN EXISTS (SELECT 1  
                                                       FROM plano_acao_item (NOLOCK)
                                                      WHERE plano_acao_item_pai IS NULL 
                                                        AND efetividade = @efetividade
                                                        AND plano_acao.id = plano_acao) THEN 1
                                      ELSE 0
                                      END 
                            ORDER BY id ASC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@marca", null, DbType.Int16, ParameterDirection.Input);
                if (marca > 0) 
                {
                    parameter.Add("@marca", marca, DbType.Int16, ParameterDirection.Input);
                }
                parameter.Add("@responsavel", responsavel, DbType.String, ParameterDirection.Input);
                parameter.Add("@data_reuniao", null, DbType.Date, ParameterDirection.Input);
                if (data_reuniao.Year > 1)
                {
                    parameter.Add("@data_reuniao", data_reuniao, DbType.Date, ParameterDirection.Input);
                }                
                parameter.Add("@efetividade", null, DbType.Int16, ParameterDirection.Input);
                if (efetividade > 0)
                {
                    parameter.Add("@efetividade", efetividade, DbType.Int16, ParameterDirection.Input);
                }

                return connection.Query<dynamic>(sql, parameter, commandType: CommandType.Text);
            }
        }


        public IEnumerable<dynamic> PlanoAcaoRelatorioMes(int? mes)
        {
            string sql = @"SELECT id
                                 ,fornecedor
                                 ,marca
                                 ,marca_nome
                                 ,descricao
                                 ,data_cadastro AS data_cadastro
                                 ,data_alteracao AS data_alteracao
                                 ,usuario
                                 ,usuario_email
                                 ,usuario_nome
                                 ,hora_inicial
                                 ,hora_final
                                 ,participantes
                                 ,pauta
                                 ,local
                                 ,plano_acao.data_reuniao AS data_reuniao
                                 ,(SELECT COUNT(plano_acao_item.id) 
                                     FROM plano_acao_item (NOLOCK)
                                    WHERE plano_acao_item.plano_acao = plano_acao.id) AS qtde_interacoes 
                             FROM plano_acao (NOLOCK)
                            WHERE ((@mes IS NULL AND MONTH(data_reuniao) = MONTH(GETDATE())) 
                                     OR MONTH(data_reuniao) = MONTH(@mes)
                                  );";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("mes", mes, DbType.Int16, ParameterDirection.Input);
                
                return connection.Query<dynamic>(sql, parameter, commandType: CommandType.Text);
            }
        }


        public IEnumerable<dynamic> PlanoAcaoRotinaAviso()
        {
            string sql = @"SELECT id
                                 ,plano_acao
                                 ,descricao
                                 ,data_prazo
                             FROM plano_acao_item (NOLOCK)
                            WHERE plano_acao IN (
                                  SELECT id FROM plano_acao (NOLOCK) 
                                   WHERE DATEDIFF(DAY, data_reuniao, GETDATE()) = 1
                            );";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql, commandType: CommandType.Text);
            }
        }

        public IEnumerable<dynamic> PlanoAcaoItemRotinaAviso()
        {
            string sql = @"SELECT id
                                 ,data_prazo
                                 ,GETDATE() AS hoje
                                 ,data_conclusao
                                 ,DATEDIFF(DAY, data_prazo, GETDATE()) AS qtde_dias_fim_prazo 
                             FROM plano_acao_item (NOLOCK)
                             WHERE DATEDIFF(DAY, data_prazo, GETDATE()) BETWEEN 0 AND 5  
                               AND data_prazo >= GETDATE() 
                               AND data_conclusao IS NULL;";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql, commandType: CommandType.Text);
            }
        }


        public IEnumerable<dynamic> DadosPlanoAcao(long id)
        {
            string sql = @"SELECT plano_acao.id, fornecedor, marca, marca_nome, 
                                  data_reuniao,
                                  descricao, 
                                  plano_acao.data_cadastro AS data_cadastro, 
                                  data_alteracao AS data_alteracao, 
                                  usuario, usuario_email, usuario_nome, 
                                  hora_inicial, 
                                  hora_final, participantes, pauta, local
                             FROM plano_acao (NOLOCK)
                            WHERE id = @id ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int32, ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parametros);
                foreach(var item in query)
                {
                    item.hora_inicial = item.hora_inicial?.ToString();
                    item.hora_final = item.hora_final?.ToString();
                }

                return query;
            }
        }

        public IEnumerable<dynamic> DadosPlanoAcaoItens(long id)
        {
            string sql = @"SELECT plano_acao_item.id
                                 ,plano_acao_item.plano_acao
                                 ,plano_acao_item.plano_acao_item_pai
                                 ,plano_acao_item.descricao
                                 ,plano_acao_item.setor
                                 ,plano_acao_item.status
                                 ,CASE plano_acao_item.status WHEN 1 THEN 'Concluído' ELSE 'Em Andamento' END AS status_nome
                                 ,plano_acao_item.efetividade
                                 ,CASE WHEN plano_acao_item.efetividade = 1 THEN 'Concluído no Prazo' 
                                       WHEN plano_acao_item.efetividade = 2 THEN 'Concluído com Atraso' 
                                       WHEN plano_acao_item.data_prazo < GETDATE() THEN 'Em Atraso'
                                       ELSE 'Em Andamento' 
                                  END AS efetividade_nome
                                 ,plano_acao_item.usuario
                                 ,plano_acao_item.usuario_email
                                 ,plano_acao_item.usuario_nome
                                 ,plano_acao_item.ficha_cadastro
                                 ,plano_acao_item.nova_regra
                                 ,plano_acao_item.posicao
                                 ,DATEDIFF(DAY, plano_acao_item.data_prazo, GETDATE()) AS dias_para_finalizar
                                 ,plano_acao_item.data_conclusao AS data_conclusao
                                 ,DATEDIFF(DAY, plano_acao_item.data_prazo, plano_acao_item.data_conclusao) AS dias_atraso
                                 ,plano_acao_item.data_prazo AS data_prazo
                                 ,plano_acao_item.data_cadastro AS data_cadastro
                                 ,plano_acao_item.data_alteracao AS data_alteracao
                                 ,setor.nome AS setor_nome
                             FROM plano_acao_item (NOLOCK)
                             LEFT OUTER JOIN setor (NOLOCK)
                               ON setor.id = plano_acao_item.setor
                            WHERE plano_acao = @id
                              AND plano_acao_item_pai IS NULL
                            ORDER BY posicao ASC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> Filhos(int idPai)
        {
            string sql = @"SELECT id, descricao 
                             FROM plano_acao_item (NOLOCK) 
                            WHERE plano_acao_item_pai = @plano_acao_item";
            
            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao_item", idPai, DbType.Int32, ParameterDirection.Input);
                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> MarcaNome(int id)
        {
            string sql = @"SELECT MARP_NOM AS nome 
                             FROM TCOM_MARPRO (NOLOCK)
                            WHERE MARP_COD = @id
                            ORDER BY MARP_NOM ASC";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int32, ParameterDirection.Input);
                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<PlanoAcaoItem_Model> PlanosAcaoItens(long planoAcaoCodigo)
        {
            string sql = @"SELECT id
                                 ,descricao
                                 ,data_prazo AS data_prazo
                                 ,status
                                 ,efetividade
                                 ,data_cadastro AS data_cadastro 
                                 ,data_cadastro
                                 ,usuario_nome
                                 ,usuario_email
                                 ,plano_acao_item_pai
                                 ,data_conclusao AS data_conclusao 
                                 ,DATEDIFF(DAY, data_prazo, GETDATE()) AS dias_para_finalizar
                                 ,plano_acao
                                 ,setor
                                 ,usuario
                                 ,ficha_cadastro
                                 ,nova_regra
                                 ,posicao
                                 ,data_alteracao AS data_alteracao
                             FROM plano_acao_item (NOLOCK)
                            WHERE plano_acao = @plano_acao_codigo; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao_codigo", planoAcaoCodigo, DbType.Int64, ParameterDirection.Input);
                return connection.Query<PlanoAcaoItem_Model>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> itens_pai(long? item_pai_id)
        {
            string sql = @"SELECT id FROM plano_acao_item (NOLOCK)
                            WHERE plano_acao IN (
                                      SELECT plano_acao 
                                        FROM plano_acao_item (NOLOCK)
                                       WHERE id = @item_pai_id) 
                              AND plano_acao_item_pai IS NULL; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("item_pai_id", item_pai_id, DbType.Int32, ParameterDirection.Input);
                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> itens_filhos(long? item_pai_id)
        {
            string sql = "SELECT id FROM plano_acao_item (NOLOCK) WHERE plano_acao_item_pai = @item_pai_id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("item_pai_id", item_pai_id, DbType.Int32, ParameterDirection.Input);
                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public string codigo_item(List<PlanoAcaoItem_Model> codigos_plano_acao_item, long? plano_acao_codigo, int plano_acao_item_codigo)
        {
            int cont = 1;
            int cont3 = 0;
            string plano_acao_item_codigo_item = "";
            foreach (var item in codigos_plano_acao_item)
            {
                if (item.plano_acao_item_pai != null)
                {
                    if ((item.plano_acao_item_pai == 0) && item.id == plano_acao_item_codigo)
                    {
                        plano_acao_item_codigo_item = plano_acao_codigo.ToString() + "." + cont.ToString();
                        break;
                    }

                    if (item.plano_acao_item_pai > 0)
                    {
                        var itens_pai = this.itens_pai(item.plano_acao_item_pai);

                        int cont2 = 1;

                        foreach (var itemPai in itens_pai)
                        {
                            if (itemPai.id == item.plano_acao_item_pai)
                            {
                                plano_acao_item_codigo_item = plano_acao_codigo + "." + cont2.ToString();
                                break;
                            }

                            cont2++;
                        }

                        var itens_filhos = this.itens_filhos(item.plano_acao_item_pai);

                        cont3 = 1;

                        foreach (var j in itens_filhos)
                        {
                            if (j.id == plano_acao_item_codigo)
                            {
                                plano_acao_item_codigo_item = plano_acao_item_codigo_item + "." + cont3.ToString();
                            }

                            cont3++;
                        }

                        break;
                    }
                }

                cont++;
            }

            return plano_acao_item_codigo_item;
        }

        public IEnumerable<dynamic> sql_pendente_filhos(int planoAcaoItemCodigo)
        {
            string sql = @"SELECT COUNT(id) AS qtde 
                             FROM plano_acao_item_observacao (NOLOCK)
                            WHERE data_prazo IS NOT NULL AND aprovado != 1 
                              AND plano_acao_item IN (
                                      SELECT id FROM plano_acao_item (NOLOCK)
                                       WHERE plano_acao_item_pai = @plano_acao_item_codigo)";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao_item_codigo", planoAcaoItemCodigo, DbType.Int32, ParameterDirection.Input);
                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> PlanoAcaoItensObservacaoRelatorioAprovar()
        {
            string sql = @"SELECT plano_acao_item_observacao.id, 
                                  plano_acao_item_observacao.plano_acao, 
                                  plano_acao_item_observacao.plano_acao_item, 
                                  plano_acao_item_observacao.descricao, 
                                  plano_acao_item_observacao.data_cadastro, 
                                  plano_acao_item_observacao.data_alteracao, 
                                  plano_acao_item_observacao.usuario, 
                                  plano_acao_item_observacao.usuario_email, 
                                  plano_acao_item_observacao.usuario_nome, 
                                  plano_acao_item_observacao.aprovado, 
                                  plano_acao_item_observacao.aprovado_usuario, 
                                  plano_acao_item_observacao.aprovado_usuario_nome, 
                                  plano_acao_item_observacao.aprovado_usuario_email, 
                                  plano_acao_item_observacao.data_aprovado, 
                                  plano_acao_item_observacao.data_prazo,
                                  plano_acao_item_observacao.data_cadastro,
                                  plano_acao_item_observacao.data_alteracao,
                                  plano_acao.id AS plano_acao_codigo,
                                  plano_acao.descricao AS plano_acao_descricao,
                                  plano_acao.marca AS plano_acao_marca,
                                  plano_acao_item.id AS plano_acao_item_codigo,
                                  plano_acao_item.descricao AS plano_acao_item_descricao,
                                  plano_acao_item.data_prazo AS plano_acao_item_data_prazo,
                                  (
                                      SELECT CASE 
                                                 WHEN COUNT(id) > 0 THEN 1
                                                 ELSE 0
                                             END
                                        FROM plano_acao_item_observacao (NOLOCK)
                                       WHERE data_prazo IS NOT NULL AND aprovado != 1 
                                         AND plano_acao_item IN (
                                                 SELECT id FROM plano_acao_item (NOLOCK)
                                                  WHERE plano_acao_item_pai = plano_acao_item.id)
                                  ) AS pendente_aprovacao_data_prazo
                                 ,usuarios.id AS usuarios_id, usuarios.email, usuarios.nome, plano_acao_item_responsavel.ticked
                             FROM plano_acao_item_observacao (NOLOCK)
                            INNER JOIN plano_acao (NOLOCK) ON plano_acao.id = plano_acao_item_observacao.plano_acao 
                            INNER JOIN plano_acao_item (NOLOCK) ON plano_acao_item.id = plano_acao_item_observacao.plano_acao_item 
                             LEFT OUTER JOIN plano_acao_item_responsavel (NOLOCK) 
                               ON plano_acao_item_responsavel.plano_acao_item = plano_acao_item_observacao.plano_acao_item
                            INNER JOIN usuarios (NOLOCK)
                               ON usuarios.id = plano_acao_item_responsavel.responsavel
                            WHERE ISNULL(plano_acao_item_observacao.aprovado, 0) = 0
                              AND plano_acao_item_observacao.data_prazo IS NOT NULL;";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                var dados = connection.Query<dynamic>(sql);
                foreach(var item in dados)
                {
                    List<dynamic> marca = this.MarcaNome(item.plano_acao_marca);
                    item.plano_acao_marca = marca.FirstOrDefault()?.nome;

                    int plano_acao_codigo = (int)item.plano_acao_codigo;
                    int plano_acao_item_codigo = (int)item.plano_acao_item_codigo;

                    /* Dados Plano de Ação Item */
                    List<PlanoAcaoItem_Model> codigos_plano_acao_item = (List<PlanoAcaoItem_Model>)this.PlanosAcaoItens(plano_acao_codigo);
                    string plano_acao_item_codigo_item = this.codigo_item(codigos_plano_acao_item, plano_acao_codigo, plano_acao_item_codigo);

                    item.plano_acao_item_codigo = plano_acao_item_codigo_item;

                    /* Filhos pendentes */
                    List<dynamic> sql_pendente_filhos = (List<dynamic>)this.sql_pendente_filhos(plano_acao_item_codigo);
                    long qtde_pendentes = sql_pendente_filhos.FirstOrDefault().qtde;

                    item.plano_acao_item_responsavel = new Responsavel_Model
                    {
                        id = item.usuarios_id,
                        email = item.email,
                        nome = item.nome,
                        ticked = item.ticked == 1,
                    };
                }
                return dados;
            }
        }

        public IEnumerable<dynamic> PlanoAcaoDatas()
        {
            string sql = @"SELECT 
                                  DISTINCT data_reuniao
                             FROM plano_acao (NOLOCK)
                            WHERE data_reuniao >= GETDATE() 
                            ORDER BY data_reuniao ASC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }


        public IEnumerable<dynamic> AgendaRelatorioCalendario()
        {
            string sql = @"SELECT TOP 1000
                                  assunto AS title, 
                                  CONCAT(data_reuniao, 'T', hora_inicial) AS start, 
                                  CONCAT(data_reuniao, 'T', hora_final) AS [end]
                             FROM agenda (NOLOCK)
                            ORDER BY data_reuniao ASC;";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> AvaliacaoAgenda(int agenda)
        {
            string sql = @"SELECT agenda_avaliacao.id
                                 ,agenda_avaliacao.agenda
                                 ,agenda_avaliacao.setor
                                 ,agenda_avaliacao.nota
                                 ,agenda_avaliacao.comentario
                                 ,agenda_avaliacao.usuario
                                 ,agenda_avaliacao.usuario_nome
                                 ,agenda_avaliacao.usuario_email
                                 ,agenda_avaliacao.data_cadastro
                                 ,agenda_avaliacao.data_alteracao
                                 ,setor.nome AS nome_setor
                             FROM agenda_avaliacao (NOLOCK)
                            INNER JOIN setor (NOLOCK)
                               ON setor.id = agenda_avaliacao.setor
                            WHERE agenda = @agenda 
                            ORDER BY setor ASC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("agenda", agenda, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parameter);
            }
        }

        public string validacao_setores_agenda(int setor, long intervalo_dias_reuniao)
        {
            if (setor == 1)
            {
                return "OK";
            } 
            else
            {
                if (intervalo_dias_reuniao > 0)
                {
                    return "NO PRAZO";
                }
                if(intervalo_dias_reuniao == 0)
                {
                    return "FORA DO PRAZO";
                }
                if (intervalo_dias_reuniao < 0 && intervalo_dias_reuniao >= -30)
                {
                    return "FORA DO PRAZO";
                }
                if(intervalo_dias_reuniao < -30)
                {
                    return "CANCELADO";
                }
                return "";
            }
        }

        public IEnumerable<dynamic> AgendaInvites(DateTime? inicio, DateTime? fim)
        {
            string sql = @"SELECT agenda.id
                                 ,agenda.fornecedor
                                 ,agenda.data_reuniao
                                 ,agenda.invite_enviado
                                 ,agenda.data_cadastro
                                 ,agenda.data_alteracao
                                 ,agenda.usuario, agenda.usuario_nome, agenda.usuario_email
                                 ,COALESCE(agenda.compras, 0) AS compras
                                 ,COALESCE(vendas, 0) AS vendas
                                 ,COALESCE(exorbitante, 0) AS exorbitante
                                 ,COALESCE(recebimento, 0) AS recebimento
                                 ,COALESCE(juridico, 0) AS juridico
                                 ,COALESCE(financeiro, 0) AS financeiro
                                 ,COALESCE(trocas, 0) AS trocas
                                 ,COALESCE(garantia, 0) AS garantia
                                 ,COALESCE(marketing, 0) AS marketing
                                 ,agenda.ti, agenda.atendimento, agenda.rh, agenda.ga, agenda.gca
                                 ,agenda.frete, agenda.facilities, agenda.desenvolvimento
                                 ,COALESCE(DATEDIFF(DAY, data_reuniao, GETDATE()), 0) AS intervalo_dias_reuniao
                             FROM agenda (NOLOCK)
                            WHERE 1 = CASE
                                          WHEN @data_inicial IS NULL THEN 1
                                          WHEN data_reuniao >= @data_inicial THEN 1
                                      END
                              AND 1 = CASE
                                          WHEN @data_final IS NULL THEN 1
                                          WHEN data_reuniao <= @data_final THEN 1
                                      END 
                            ORDER BY intervalo_dias_reuniao ASC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("data_inicial", inicio, DbType.DateTime, ParameterDirection.Input);
                param.Add("data_final", fim, DbType.DateTime, ParameterDirection.Input);

                var _agendaInvites = connection.Query<dynamic>(sql, param);

                long total_ok = 0;
                long total_no_prazo = 0;
                long total_fora_prazo = 0;
                long total_cancelado = 0;
                foreach (var item in _agendaInvites)
                {
                    total_ok = 0;
                    total_no_prazo = 0;
                    total_fora_prazo = 0;
                    total_cancelado = 0;

                    if(item.fornecedor != null)
                    {
                        List<dynamic> marca = (List<dynamic>)this.MarcaNome(item.fornecedor);
                        item.fornecedor = marca.FirstOrDefault().nome;
                    }
                    else
                    {
                        item.fornecedor = item.fornecedor_descricao;
                    }
                        
                    item.invite_enviado = item.invite_enviado == 1 ? "Sim" : "Não";
                    
                    item.compras = this.validacao_setores_agenda((int)item.compras, (long)item.intervalo_dias_reuniao);
                    item.vendas = this.validacao_setores_agenda((int)item.vendas, (int)item.intervalo_dias_reuniao);
                    item.exorbitante = this.validacao_setores_agenda((int)item.exorbitante, (int)item.intervalo_dias_reuniao);
                    item.recebimento = this.validacao_setores_agenda((int)item.recebimento, (int)item.intervalo_dias_reuniao);
                    item.juridico = this.validacao_setores_agenda((int)item.juridico, (int)item.intervalo_dias_reuniao);
                    item.financeiro = this.validacao_setores_agenda((int)item.financeiro, (int)item.intervalo_dias_reuniao);
                    item.trocas = this.validacao_setores_agenda((int)item.trocas, (int)item.intervalo_dias_reuniao);
                    item.garantia = this.validacao_setores_agenda((int)item.garantia, (int)item.intervalo_dias_reuniao);
                    item.marketing = this.validacao_setores_agenda((int)item.marketing, (int)item.intervalo_dias_reuniao);

                    switch(item.compras)
                    {
                        case "OK":
                            total_ok++;
                            break;
                        case "NO PRAZO":
                            total_no_prazo++;
                            break;
                        case "FORA DO PRAZO":
                            total_fora_prazo++;
                            break;
                        case "CANCELADO":
                            total_cancelado++;
                            break;
                    }

                    switch(item.vendas)
                    {
                        case "OK":
                            total_ok++;
                            break;
                        case "NO PRAZO":
                            total_no_prazo++;
                            break;
                        case "FORA DO PRAZO":
                            total_fora_prazo++;
                            break;
                        case "CANCELADO":
                            total_cancelado++;
                            break;
                    }

                    switch(item.exorbitante)
                    {
                        case "OK":
                            total_ok++;
                            break;
                        case "NO PRAZO":
                            total_no_prazo++;
                            break;
                        case "FORA DO PRAZO":
                            total_fora_prazo++;
                            break;
                        case "CANCELADO":
                            total_cancelado++;
                            break;
                    }

                    switch(item.recebimento)
                    {
                        case "OK":
                            total_ok++;
                            break;
                        case "NO PRAZO":
                            total_no_prazo++;
                            break;
                        case "FORA DO PRAZO":
                            total_fora_prazo++;
                            break;
                        case "CANCELADO":
                            total_cancelado++;
                            break;
                    }

                    switch(item.juridico)
                    {
                        case "OK":
                            total_ok++;
                            break;
                        case "NO PRAZO":
                            total_no_prazo++;
                            break;
                        case "FORA DO PRAZO":
                            total_fora_prazo++;
                            break;
                        case "CANCELADO":
                            total_cancelado++;
                            break;
                    }

                    switch(item.financeiro)
                    {
                        case "OK":
                            total_ok++;
                            break;
                        case "NO PRAZO":
                            total_no_prazo++;
                            break;
                        case "FORA DO PRAZO":
                            total_fora_prazo++;
                            break;
                        case "CANCELADO":
                            total_cancelado++;
                            break;
                    }

                    switch(item.trocas)
                    {
                        case "OK":
                            total_ok++;
                            break;
                        case "NO PRAZO":
                            total_no_prazo++;
                            break;
                        case "FORA DO PRAZO":
                            total_fora_prazo++;
                            break;
                        case "CANCELADO":
                            total_cancelado++;
                            break;
                    }

                    switch (item.garantia)
                    {
                        case "OK":
                            total_ok++;
                            break;
                        case "NO PRAZO":
                            total_no_prazo++;
                            break;
                        case "FORA DO PRAZO":
                            total_fora_prazo++;
                            break;
                        case "CANCELADO":
                            total_cancelado++;
                            break;
                    }

                    switch (item.marketing)
                    {
                        case "OK":
                            total_ok++;
                            break;
                        case "NO PRAZO":
                            total_no_prazo++;
                            break;
                        case "FORA DO PRAZO":
                            total_fora_prazo++;
                            break;
                        case "CANCELADO":
                            total_cancelado++;
                            break;
                    }

                    var avaliacao = this.AvaliacaoAgenda(int.Parse(item.id.ToString()));
                    item.avaliacao = avaliacao;

                    Object ok = total_ok;
                    Object noPrazo = total_no_prazo;
                    Object foraPrazo = total_fora_prazo;
                    Object cancelado = total_cancelado;
                    item.total_ok = ok;
                    item.total_no_prazo = noPrazo;
                    item.total_fora_prazo = foraPrazo;
                    item.total_cancelado = cancelado;
                }


                return _agendaInvites.Select(x =>
                {
                    x.data_cadastro = x.data_cadastro.ToString("dd/MM/yyyy");
                    x.data_reuniao = x.data_reuniao.ToString("dd/MM/yyyy");
                    return x;
                }).ToList();
            }

        }

        public List<dynamic> PlanoAcaoQtdeUsuarioEfetividade(string usuario)
        {
            string sql = @"SELECT COUNT(plano_acao_item.id) AS qtde
                                 ,ISNULL(plano_acao_item.efetividade, 0) AS efetividade
                             FROM plano_acao_item (NOLOCK)
                            INNER JOIN plano_acao_item_responsavel (NOLOCK)
                               ON plano_acao_item_responsavel.plano_acao_item = plano_acao_item.id
                            INNER JOIN usuarios (NOLOCK)
                               ON usuarios.id = plano_acao_item_responsavel.responsavel
                            WHERE usuarios.email LIKE CONCAT('%', @usuario, '%')
                            GROUP BY ISNULL(plano_acao_item.efetividade, 0); ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("usuario", usuario, DbType.String, ParameterDirection.Input);
                
                return (List<dynamic>) connection.Query<dynamic>(sql, parameter);
            }
        }

        public IEnumerable<dynamic> QtdeObservacoes(long? plano_acao_item_id)
        {
            string sql = @"SELECT COUNT(id) AS qtde FROM plano_acao_item_observacao (NOLOCK)
                            WHERE plano_acao_item = @plano_acao_item_id;";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("plano_acao_item_id", plano_acao_item_id, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parameter);
            }
        }

        public IEnumerable<PlanoAcaoItem_Model> DadosItensFilhos(long plano_acao_item_id)
        {
            string sql = @"SELECT plano_acao_item.id
                                 ,plano_acao_item.plano_acao
                                 ,plano_acao_item.plano_acao_item_pai
                                 ,plano_acao_item.descricao
                                 ,plano_acao_item.setor
                                 ,plano_acao_item.status
                                 ,CASE plano_acao_item.status WHEN 1 THEN 'Concluído' ELSE 'Em Andamento' END AS status_nome
                                 ,efetividade                                 
                                 ,CASE WHEN plano_acao_item.efetividade = 1 THEN 'Concluído no Prazo' 
                                       WHEN plano_acao_item.efetividade = 2 THEN 'Concluído com Atraso' 
                                       ELSE 'Em Andamento' 
                                  END AS efetividade_nome
                                 ,plano_acao_item.usuario
                                 ,plano_acao_item.usuario_email
                                 ,plano_acao_item.usuario_nome
                                 ,plano_acao_item.ficha_cadastro
                                 ,plano_acao_item.nova_regra
                                 ,plano_acao_item.posicao
                                 ,DATEDIFF(DAY, plano_acao_item.data_prazo, GETDATE()) AS dias_para_finalizar
                                 ,plano_acao_item.data_conclusao AS data_conclusao
                                 ,DATEDIFF(DAY, plano_acao_item.data_prazo, plano_acao_item.data_conclusao) AS dias_atraso
                                 ,plano_acao_item.data_prazo AS data_prazo
                                 ,plano_acao_item.data_cadastro AS data_cadastro
                                 ,plano_acao_item.data_alteracao AS data_alteracao
                                 ,setor.nome AS setor_nome
                             FROM plano_acao_item (NOLOCK)
                            INNER JOIN setor (NOLOCK)
                               ON setor.id = plano_acao_item.setor
                            WHERE plano_acao_item_pai = @plano_acao_item_id
                            ORDER BY posicao ASC;";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("plano_acao_item_id", plano_acao_item_id, DbType.Int32, ParameterDirection.Input);

                return connection.Query<PlanoAcaoItem_Model>(sql, parameter);
            }
        }

        private IEnumerable<dynamic> FilhosConcluidos(int plano_acao_item_id)
        {
            string sql = @"SELECT 
                                  COUNT(id) AS qtde 
                             FROM plano_acao_item (NOLOCK)
                            WHERE plano_acao_item_pai = @plano_acao_item_id
                              AND status IS NULL";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("plano_acao_item_id", plano_acao_item_id, DbType.Int32, ParameterDirection.Input);

                return connection.Query<string>(sql, parameter);
            }
        }

        public IEnumerable<dynamic> RetornoPlanoAcaoItens(string? usuario)
        {
            string sql = @"SELECT plano_acao_item.id
                                 ,plano_acao_item.plano_acao
                                 ,plano_acao_item.plano_acao_item_pai
                                 ,plano_acao_item.descricao
                                 ,plano_acao_item.setor
                                 ,plano_acao_item.status
                                 ,plano_acao_item.efetividade
                                 ,plano_acao_item.usuario
                                 ,plano_acao_item.usuario_email
                                 ,plano_acao_item.usuario_nome
                                 ,plano_acao_item.ficha_cadastro
                                 ,plano_acao_item.nova_regra
                                 ,plano_acao_item.posicao 
                                 ,setor.nome AS setor_nome
                                 ,plano_acao_item.data_conclusao
                                 ,plano_acao_item.data_prazo
                                 ,plano_acao_item.data_cadastro
                                 ,plano_acao_item.data_alteracao
                                 ,(SELECT COUNT(id) AS qtde FROM plano_acao_item_observacao (NOLOCK)
                                    WHERE plano_acao_item_observacao.plano_acao_item = plano_acao_item.id) AS qtde_observacoes
                                 ,(SELECT COUNT(plano2.id) AS qtde 
                                     FROM plano_acao_item (NOLOCK) AS plano2
                                    WHERE plano2.plano_acao_item_pai = plano_acao_item.id
                                      AND plano2.status IS NULL) AS qtd_filhos_concluidos
                             FROM plano_acao_item (NOLOCK)
                            INNER JOIN setor (NOLOCK)
                               ON setor.id = plano_acao_item.setor
                            WHERE EXISTS(SELECT 1 FROM plano_acao_item_responsavel (NOLOCK)
                                          INNER JOIN usuarios (NOLOCK)
                                             ON usuarios.id = plano_acao_item_responsavel.responsavel
                                          WHERE usuarios.email LIKE CONCAT('%', @usuario, '%')
                                            AND plano_acao_item_responsavel.plano_acao_item = plano_acao_item.id)
                              AND status IS NULL 
                            ORDER BY posicao ASC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("usuario", usuario, DbType.String, ParameterDirection.Input);
                
                List<dynamic> retorno = (List<dynamic>)connection.Query<dynamic>(sql, parameter);

                int cont = 1;
                foreach(var item in retorno)
                {
                    item.codigo = usuario?.ToString() + "." + cont.ToString();
                    var responsaveis = this.PlanoAcaoItensResponsavel(item.id);
                    
                    item.responsavel_nomes = new List<string>();
                    foreach (var responsavel in responsaveis)
                    {
                        item.responsavel_nomes.Add(responsavel.nome);
                    }

                    item.responsavel = responsaveis;
                    
                    item.status = item.status == 1 ? "Concluído" : "Em Andamento";

                    if (item.efetividade == 1)
                    {
                        item.efetividade = "Concluído no Prazo";
                    } 
                    else if (item.efetividade == 2)
                    {
                        item.efetividade = "Concluído com Atraso";
                    }
                    else
                    {
                        item.efetividade = "Em Andamento";
                    }
                    
                    ///* Observações */
                    //item.qtde_observacoes = QtdeObservacoes(item?.qtde).FirstOrDefault().qtde;
                    /* Filhos */
                    var dados_itens_filhos = DadosItensFilhos((int)item.id);
                    item.item_filho_check = "0";

                    item.filhos_concluidos = item.qtd_filhos_concluidos > 0 ? 
                        "não" : "sim";

                    cont++;
                }

                return retorno;
            }
        }


        public IEnumerable<dynamic> AgendaRelatorio(DateTime? dataAgenda, string? responsavel)
        {
            string sql = @"SELECT 
                                  agenda.id, 
                                  agenda.para, 
                                  agenda.avisar, 
                                  agenda.fornecedor, 
                                  agenda.fornecedor_descricao,
                                  agenda.fornecedor_novo,
                                  agenda.assunto,
                                  agenda.local,
                                  agenda.hora_inicial,
                                  agenda.hora_final,
                                  agenda.observacao,
                                  agenda.invite_enviado,
                                  agenda.usuario,
                                  agenda.usuario_nome,
                                  agenda.usuario_email,
                                  agenda.compras,
                                  agenda.vendas,
                                  agenda.exorbitante,
                                  agenda.recebimento,
                                  agenda.juridico,
                                  agenda.financeiro,
                                  agenda.trocas,
                                  agenda.garantia,
                                  agenda.marketing,
                                  agenda.ti,
                                  agenda.atendimento,
                                  agenda.rh,
                                  agenda.ga,
                                  agenda.gca,
                                  agenda.frete,
                                  agenda.facilities,
                                  agenda.desenvolvimento,
                                  agenda.data_reuniao,
                                  agenda.data_cadastro,
                                  agenda.data_alteracao
                             FROM agenda (NOLOCK)
                            WHERE (data_reuniao = @data_reuniao OR @data_reuniao IS NULL)
                              AND (usuario_email = @usuario_email OR @usuario_email IS NULL)
                            ORDER BY id DESC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                connection.Open();
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("data_reuniao", dataAgenda, DbType.DateTime, ParameterDirection.Input);
                parameters.Add("usuario_email", responsavel, DbType.String, ParameterDirection.Input);

                var retorno = connection.Query<dynamic>(sql, parameters);

                foreach (var item in retorno)
                {
                    item.fornecedor_descricao = item.fornecedor != null ? this.MarcaNome(item.fornecedor)[0].nome : null;
                    if (item.para != null)
                    {
                        var para = JsonConvert.DeserializeObject<dynamic>(item.para.ToString());
                        List<dynamic> listaPara = new List<dynamic>();
                        foreach (var usuario in para)
                        {
                            listaPara.Add(new
                            {
                                email = usuario.email.ToString(),
                                nome = usuario.nome.ToString(),
                                ticked = (bool)usuario.ticked
                            });
                        }

                        item.para = listaPara;
                    }

                    item.hora_inicial = item.hora_inicial.ToString();
                    item.hora_final = item.hora_final.ToString();

                    //item.avisar = JsonConvert.DeserializeObject<dynamic>(item.avisar.ToString());
                }

                return retorno;
            }
        }

        public long AgendaStatusSetor(long agenda, int setor, string comentario, long usuario, string usuario_email, string usuario_nome, string arquivo)
        {
            string sql = @"INSERT INTO agenda_setor(agenda, setor) 
                           SELECT @agenda, @setor 
                            WHERE NOT EXISTS(SELECT 1 FROM agenda_setor WHERE agenda = @agenda AND setor = @setor);

                           INSERT INTO agenda_interacoes (agenda, setor, comentario, usuario, usuario_nome, usuario_email, data_cadastro, arquivo) 
                           VALUES (@agenda, @setor, @comentario, @usuario, @usuario_nome, @usuario_email, GETDATE(), @arquivo);

                           SELECT @@IDENTITY AS insert_id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("agenda", agenda, DbType.Int64, ParameterDirection.Input);
                parameter.Add("setor", setor, DbType.Int32, ParameterDirection.Input);
                parameter.Add("comentario", comentario, DbType.String, ParameterDirection.Input);
                parameter.Add("usuario", usuario, DbType.Int64, ParameterDirection.Input);
                parameter.Add("usuario_email", usuario_email, DbType.String, ParameterDirection.Input);
                parameter.Add("usuario_nome", usuario_nome, DbType.String, ParameterDirection.Input);
                parameter.Add("arquivo", arquivo, DbType.String, ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parameter);
                return (long)query.FirstOrDefault().insert_id;
            }
        }

        public IEnumerable<dynamic> ObservacoesItens(long plano_acao_item)
        {
            string sql = @"SELECT descricao FROM plano_acao_item_observacao (NOLOCK)
                            WHERE plano_acao_item = @plano_acao_item; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("plano_acao_item", plano_acao_item, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parameter);
            }
        }


        public IEnumerable<dynamic> plano_acao_inserir(PlanoAcao_Model planoAcao)
        {
            string sql = @"INSERT INTO plano_acao(
                               fornecedor
                              ,marca
                              ,marca_nome
                              ,data_reuniao
                              ,descricao
                              ,data_cadastro
                              ,data_alteracao
                              ,usuario
                              ,usuario_email
                              ,usuario_nome
                              ,hora_inicial
                              ,hora_final
                              ,participantes
                              ,pauta
                              ,local
                           ) VALUES (
                               @fornecedor
                              ,@marca
                              ,@marca_nome
                              ,@data_reuniao
                              ,@descricao
                              ,@data_cadastro
                              ,@data_alteracao
                              ,@usuario
                              ,@usuario_email
                              ,@usuario_nome
                              ,@hora_inicial
                              ,@hora_final
                              ,@participantes
                              ,@pauta
                              ,@local
                           );
                           SELECT @@IDENTITY AS insert_id;";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("fornecedor", planoAcao.fornecedor, DbType.String, ParameterDirection.Input);
                parametros.Add("marca", planoAcao.marca, DbType.Int32, ParameterDirection.Input);
                parametros.Add("marca_nome", planoAcao.marca_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("data_reuniao", planoAcao.data_reuniao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("descricao", planoAcao.descricao, DbType.String, ParameterDirection.Input);
                parametros.Add("data_cadastro", DateTime.Now, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", planoAcao.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("usuario", planoAcao.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_email", planoAcao.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_nome", planoAcao.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("hora_inicial", planoAcao.hora_inicial, DbType.Time, ParameterDirection.Input);
                parametros.Add("hora_final", planoAcao.hora_final, DbType.Time, ParameterDirection.Input);
                parametros.Add("participantes", planoAcao.participantes, DbType.String, ParameterDirection.Input);
                parametros.Add("pauta", planoAcao.pauta, DbType.String, ParameterDirection.Input);
                parametros.Add("local", planoAcao.local, DbType.String, ParameterDirection.Input);
                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public int plano_acao_editar(PlanoAcao_Model planoAcao)
        {
            string sql = @"UPDATE plano_acao
                              SET fornecedor      = @fornecedor
                                 ,marca           = @marca
                                 ,marca_nome      = @marca_nome
                                 ,data_reuniao    = @data_reuniao
                                 ,descricao       = @descricao
                                 ,data_cadastro   = @data_cadastro
                                 ,data_alteracao  = @data_alteracao
                                 ,usuario         = @usuario
                                 ,usuario_email   = @usuario_email
                                 ,usuario_nome    = @usuario_nome
                                 ,hora_inicial    = @hora_inicial
                                 ,hora_final      = @hora_final
                                 ,participantes   = @participantes
                                 ,pauta           = @pauta
                                 ,local           = @local
                           WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("fornecedor", planoAcao.fornecedor, DbType.String, ParameterDirection.Input);
                parametros.Add("marca", planoAcao.marca, DbType.Int32, ParameterDirection.Input);
                parametros.Add("marca_nome", planoAcao.marca_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("data_reuniao", planoAcao.data_reuniao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("descricao", planoAcao.descricao, DbType.String, ParameterDirection.Input);
                parametros.Add("data_cadastro", planoAcao.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", DateTime.Now, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("usuario", planoAcao.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_email", planoAcao.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_nome", planoAcao.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("hora_inicial", planoAcao.hora_inicial, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("hora_final", planoAcao.hora_final, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("participantes", planoAcao.participantes, DbType.String, ParameterDirection.Input);
                parametros.Add("pauta", planoAcao.pauta, DbType.String, ParameterDirection.Input);
                parametros.Add("local", planoAcao.local, DbType.String, ParameterDirection.Input);
                parametros.Add("id", planoAcao.id, DbType.Int32, ParameterDirection.Input);
                
                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> usuario_Id(string email)
        {
            string sql = @"SELECT id FROM usuario WHERE email = @email; ";

            using var connection = new MySqlConnection(_connectionStringMysqlConnectParts);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("email", email, DbType.String, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> UsuarioPorId(string email)
        {
            string sql = @"SELECT id, setor FROM usuarios (NOLOCK) WHERE email = @email; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("email", email, DbType.String, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        /* Volta o item pai para o status de "Em Andamento" quando há itens filhos não concluídos */
        public IEnumerable<dynamic> VoltaStatusEmAndamento(long? plano_acao_item_pai)
        {
            string sql = @"UPDATE plano_acao_item SET data_conclusao = NULL, efetividade = NULL, status = NULL WHERE id = @plano_acao_item_pai";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao_item_pai", plano_acao_item_pai, DbType.Int64, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> InserirPlanoAcaoItem(PlanoAcaoItem_Model planoAcaoItem)
        {
            string sql = @"INSERT INTO plano_acao_item ( 
                               plano_acao
                              ,plano_acao_item_pai
                              ,descricao
                              ,setor
                              ,data_prazo
                              ,data_conclusao
                              ,status
                              ,efetividade
                              ,data_cadastro
                              ,data_alteracao
                              ,usuario
                              ,usuario_email
                              ,usuario_nome
                              ,ficha_cadastro
                              ,nova_regra
                              ,posicao
                           ) VALUES (
                               @plano_acao
                              ,@plano_acao_item_pai
                              ,@descricao
                              ,@setor
                              ,@data_prazo
                              ,@data_conclusao
                              ,@status
                              ,@efetividade
                              ,@data_cadastro
                              ,@data_alteracao
                              ,@usuario
                              ,@usuario_email
                              ,@usuario_nome
                              ,@ficha_cadastro
                              ,@nova_regra
                              ,@posicao
                           );
                           SELECT @@IDENTITY AS insert_id;";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao", planoAcaoItem.plano_acao, DbType.Int64, ParameterDirection.Input);
                parametros.Add("plano_acao_item_pai", planoAcaoItem.plano_acao_item_pai, DbType.Int64, ParameterDirection.Input);
                parametros.Add("descricao", planoAcaoItem.descricao, DbType.String, ParameterDirection.Input);
                parametros.Add("setor", planoAcaoItem.setor, DbType.Int32, ParameterDirection.Input);
                parametros.Add("data_prazo", planoAcaoItem.data_prazo, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_conclusao", planoAcaoItem.data_conclusao, DbType.DateTime, ParameterDirection.Input);                
                parametros.Add("status", planoAcaoItem.status, DbType.Int32, ParameterDirection.Input);
                parametros.Add("status_nome", planoAcaoItem.status_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("efetividade", planoAcaoItem.efetividade, DbType.Int32, ParameterDirection.Input);
                parametros.Add("data_cadastro", DateTime.Now, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", planoAcaoItem.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("usuario", planoAcaoItem.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_email", planoAcaoItem.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_nome", planoAcaoItem.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("ficha_cadastro", planoAcaoItem.ficha_cadastro, DbType.Int32, ParameterDirection.Input);
                parametros.Add("nova_regra", planoAcaoItem.nova_regra, DbType.Int32, ParameterDirection.Input);
                parametros.Add("posicao", planoAcaoItem.posicao, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public int AlteraPlanoAcaoItem(PlanoAcaoItem_Model planoAcaoItem)
        {
            string sql = @"UPDATE plano_acao_item
                               SET plano_acao          = @plano_acao
                                  ,plano_acao_item_pai = @plano_acao_item_pai
                                  ,descricao           = @descricao
                                  ,setor               = @setor
                                  ,data_prazo          = @data_prazo
                                  ,data_conclusao      = @data_conclusao
                                  ,status              = @status
                                  ,efetividade         = @efetividade
                                  ,data_cadastro       = @data_cadastro
                                  ,data_alteracao      = @data_alteracao
                                  ,usuario             = @usuario
                                  ,usuario_email       = @usuario_email
                                  ,usuario_nome        = @usuario_nome
                                  ,ficha_cadastro      = @ficha_cadastro
                                  ,nova_regra          = @nova_regra
                                  ,posicao             = @posicao
                           WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao", planoAcaoItem.plano_acao, DbType.Int64, ParameterDirection.Input);
                parametros.Add("plano_acao_item_pai", planoAcaoItem.plano_acao_item_pai, DbType.Int64, ParameterDirection.Input);
                parametros.Add("descricao", planoAcaoItem.descricao, DbType.String, ParameterDirection.Input);
                parametros.Add("setor", planoAcaoItem.setor, DbType.Int32, ParameterDirection.Input);
                parametros.Add("data_prazo", planoAcaoItem.data_prazo, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_conclusao", planoAcaoItem.data_conclusao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("status", planoAcaoItem.status, DbType.Int32, ParameterDirection.Input);
                parametros.Add("status_nome", planoAcaoItem.status_nome, DbType.Int32, ParameterDirection.Input);
                parametros.Add("efetividade", planoAcaoItem.efetividade, DbType.Int32, ParameterDirection.Input);
                parametros.Add("data_cadastro", DateTime.Now, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", planoAcaoItem.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("usuario", planoAcaoItem.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_email", planoAcaoItem.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_nome", planoAcaoItem.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("ficha_cadastro", planoAcaoItem.ficha_cadastro, DbType.Int32, ParameterDirection.Input);
                parametros.Add("nova_regra", planoAcaoItem.nova_regra, DbType.Int32, ParameterDirection.Input);
                parametros.Add("posicao", planoAcaoItem.posicao, DbType.Int32, ParameterDirection.Input);
                parametros.Add("id", planoAcaoItem.id, DbType.Int32, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> PlanoAcaoPai(long id)
        {
            string sql = @"SELECT plano_acao_item_pai FROM plano_acao_item (NOLOCK) WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> dados_item_filho(long? id)
        {
            string sql = @"SELECT plano_acao_item.id
                                 ,plano_acao_item.plano_acao
                                 ,plano_acao_item.plano_acao_item_pai
                                 ,plano_acao_item.descricao
                                 ,plano_acao_item.setor
                                 ,plano_acao_item.status
                                 ,plano_acao_item.efetividade
                                 ,plano_acao_item.usuario
                                 ,plano_acao_item.usuario_email
                                 ,plano_acao_item.usuario_nome
                                 ,plano_acao_item.ficha_cadastro
                                 ,plano_acao_item.nova_regra
                                 ,plano_acao_item.posicao
                                 ,plano_acao_item.data_conclusao
                                 ,plano_acao_item.data_prazo
                                 ,plano_acao_item.data_cadastro
                                 ,plano_acao_item.data_alteracao
                                 ,setor.nome AS setor_nome
                             FROM plano_acao_item (NOLOCK)
                            INNER JOIN setor (NOLOCK)
                               ON setor.id = plano_acao_item.setor
                            WHERE plano_acao_item.id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);

                List<dynamic> dados_itens = (List<dynamic>)connection.Query<dynamic>(sql, parametros);
                if (dados_itens.Count > 0)
                {
                    int cont = 1;
                    foreach(var plano_acao_item in dados_itens)
                    {
                        string agenda_item = id.ToString() + "." + cont.ToString();
                        plano_acao_item.codigo = agenda_item;

                        long plano_acao_item_id = plano_acao_item.id;

                        plano_acao_item.responsavel_nomes = new List<string>();
                        var listaResponsaveis = this.PlanoAcaoItensResponsavel(plano_acao_item.id);

                        foreach (var responsavel in listaResponsaveis)
                        {
                            plano_acao_item.responsavel_nomes.Add(responsavel.nome);
                        }

                        /* Observações */
                        List<dynamic> observacoes = (List<dynamic>)this.QtdeObservacoes((int)plano_acao_item.id);
                        plano_acao_item.qtde_observacoes = observacoes.FirstOrDefault().qtde;

                        /* Filhos */
                        var dados_itens_filhos = DadosItensFilhos(plano_acao_item.id);
                        plano_acao_item.qtde_itens_filhos = (dynamic)dados_itens_filhos.Count;

                        /* Itens filhos */
                        int cont2 = 1;

                        foreach(var plano_acao_item_filho in dados_itens_filhos)
                        {
                            agenda_item = id.ToString() + "." + cont.ToString() + "." + cont2.ToString();
                            plano_acao_item_filho.codigo = agenda_item;

                            plano_acao_item_id = plano_acao_item_filho.id;

                            listaResponsaveis = this.PlanoAcaoItensResponsavel(plano_acao_item_filho.id);

                            foreach (var responsavel in listaResponsaveis)
                            {
                                plano_acao_item_filho.responsavel_nomes = responsavel.nome;
                            }

                            cont2++;
                        }

                        plano_acao_item.itens_filhos = dados_itens_filhos;

                        /* Fim Itens filhos */

                        cont++;
                    }
                }

                return dados_itens;
            }
        }

        public IEnumerable<dynamic> PlanoAcaoItensGravar(PlanoAcaoItem_Model planoAcaoItem)
        {
            /* Volta o item pai para o status de "Em Andamento" quando há itens filhos não concluídos */
            if (planoAcaoItem.plano_acao_item_pai > 0)
            {
                var tmp = VoltaStatusEmAndamento(planoAcaoItem.plano_acao_item_pai);
            }

            // Pega o setor pelo responsável
            if (planoAcaoItem.responsavel.Any())
            {
                planoAcaoItem.setor = this.SetorPorUsuario(planoAcaoItem.responsavel.FirstOrDefault().id)?.id;
            }            

            // Verifica se existe o item do plano de ação e grava
            int id = 0;
            if (planoAcaoItem.id == null)
            {
                id = Convert.ToInt32(InserirPlanoAcaoItem(planoAcaoItem).FirstOrDefault().insert_id);

                if(planoAcaoItem.plano_acao_item_pai > 0)
                {
                    return dados_item_filho(planoAcaoItem.plano_acao_item_pai);
                }
            }
            else
            {
                id = (int)planoAcaoItem.id;

                planoAcaoItem.data_alteracao = DateTime.Now;

                this.AlteraPlanoAcaoItem(planoAcaoItem);

                List<dynamic> dados = (List<dynamic>)PlanoAcaoPai((long)planoAcaoItem.id);
                long? plano_acao_item_pai = dados.FirstOrDefault().plano_acao_item_pai;

                if (plano_acao_item_pai > 0)
                {
                    return this.dados_item_filho(plano_acao_item_pai);
                }
            }

            // Grava os responsáveis
            foreach(var item in planoAcaoItem.responsavel)
            {
                this.PlanoAcaoItemResponsavelInserir(id, item.id);
            }

            List<dynamic> lista = new List<dynamic>();
            lista.Add(new { 
                id = id 
            });
            return lista;
        }

        public IEnumerable<dynamic> plano_acao_item_progresso(long id)
        {
            string sql = @"SELECT (SELECT COUNT(1) FROM plano_acao_item AS item (NOLOCK)
                                    WHERE item.plano_acao = plano_acao_item.plano_acao
                                      AND status IN (1,2)) AS concluidos
                                 ,COUNT(1) AS total
                             FROM plano_acao_item (NOLOCK)
                            WHERE plano_acao = @id
                            GROUP BY plano_acao_item.plano_acao; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);

                List<dynamic> dados = (List<dynamic>)connection.Query<dynamic>(sql, parametros);
                dados.Select(c => { 
                    c.qtd_concluidos = c.concluidos.ToString() + "/" + c.total.ToString();
                    c.porcentagem_status = c.total == 0 ? "0%" : ((c.concluidos * 100) / c.total).ToString() + "%";
                    return c; 
                }).ToList();

                return dados;
            }
        }

        public IEnumerable<dynamic> FornecedorNome(int id)
        {
            string sql = @"SELECT DISTINCT 
                                  UPPER(LTRIM(RTRIM(TGEN_ENTBAS.ENTB_NOM_RAZ))) AS razao_social 
                             FROM TCOM_CLIFOR (NOLOCK)
                            INNER JOIN TCPR_PROFOR WITH (NOLOCK) ON TCPR_PROFOR.CLIF_COD = TCOM_CLIFOR.CLIF_COD
                            INNER JOIN TGEN_ENTBAS WITH (NOLOCK) ON TGEN_ENTBAS.ENTB_COD = TCOM_CLIFOR.ENTB_COD
                            INNER JOIN TCOM_PROSER WITH (NOLOCK) ON TCPR_PROFOR.PROS_COD = TCOM_PROSER.PROS_COD
                            INNER JOIN TCOM_PROCOD WITH (NOLOCK) ON TCOM_PROSER.PROS_COD = TCOM_PROCOD.PROS_COD
                            INNER JOIN TCOM_CODTIP WITH (NOLOCK) ON TCOM_PROCOD.CODT_COD = TCOM_CODTIP.CODT_COD
                            WHERE TCPR_PROFOR.PROS_COD = TCOM_PROCOD.PROS_COD
                              AND TCOM_PROCOD.CODT_COD in (2, 3, 7, 8, 9) 
                              AND TGEN_ENTBAS.ENTB_COD = @id; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> PlanoAcaoItensObservacao(int item)
        {
            string sql = @"SELECT id
                                 ,plano_acao
                                 ,plano_acao_item
                                 ,descricao
                                 ,data_cadastro
                                 ,data_alteracao
                                 ,usuario
                                 ,usuario_email
                                 ,usuario_nome
                                 ,data_prazo
                                 ,aprovado
                                 ,aprovado_usuario
                                 ,aprovado_usuario_nome
                                 ,aprovado_usuario_email
                                 ,data_aprovado
                                 ,CASE WHEN data_prazo IS NOT NULL AND aprovado != 1 THEN 1 ELSE 0 END AS pendente_aprovacao_data_prazo
                                 ,CASE WHEN data_prazo IS NOT NULL THEN 1 ELSE 0 END AS solicitacao_data_prazo
                             FROM plano_acao_item_observacao (NOLOCK)
                            WHERE plano_acao_item = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", item, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        /* Verfiica se há alguma solicitação de prazo pendente, e se houver atualiza com a nova solicitação */
        public IEnumerable<dynamic> SolicitacaoNaoAprovada(long plano_acao_item)
        {
            string sql = @"SELECT id FROM plano_acao_item_observacao (NOLOCK)
                            WHERE aprovado IS NULL AND plano_acao_item = @plano_acao_item; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao_item", plano_acao_item, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public int PlanoAcaoItensObservacaoAtualizar(PlanoAcaoItemObservacao observacao)
        {
            string sql = @"UPDATE plano_acao_item_observacao 
                              SET plano_acao             = @plano_acao
                                 ,plano_acao_item        = @plano_acao_item
                                 ,descricao              = @descricao 
                                 ,data_cadastro          = @data_cadastro
                                 ,data_alteracao         = @data_alteracao
                                 ,usuario                = @usuario
                                 ,usuario_email          = @usuario_email
                                 ,usuario_nome           = @usuario_nome
                                 ,data_prazo             = @data_prazo
                                 ,aprovado               = @aprovado
                                 ,aprovado_usuario       = @aprovado_usuario
                                 ,aprovado_usuario_nome  = @aprovado_usuario_nome
                                 ,aprovado_usuario_email = @aprovado_usuario_email
                                 ,data_aprovado          = @data_aprovado
                            WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao", observacao.plano_acao, DbType.Int32, ParameterDirection.Input);
                parametros.Add("plano_acao_item", observacao.plano_acao_item, DbType.Int32, ParameterDirection.Input);
                parametros.Add("descricao", observacao.descricao, DbType.String, ParameterDirection.Input);
                parametros.Add("data_cadastro", observacao.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", observacao.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("usuario", observacao.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_email", observacao.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_nome", observacao.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("data_prazo", observacao.data_prazo, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("aprovado", observacao.aprovado, DbType.Int32, ParameterDirection.Input);
                parametros.Add("aprovado_usuario", observacao.aprovado_usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("aprovado_usuario_nome", observacao.aprovado_usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("aprovado_usuario_email", observacao.aprovado_usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("data_aprovado", observacao.data_aprovado, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("id", observacao.id, DbType.Int32, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int PlanoAcaoItensObservacaoGravar(PlanoAcaoItemObservacao observacao)
        {
            string sql = @"INSERT INTO plano_acao_item_observacao(plano_acao
                                                                 ,plano_acao_item
                                                                 ,descricao 
                                                                 ,data_cadastro 
                                                                 ,data_alteracao 
                                                                 ,usuario 
                                                                 ,usuario_email 
                                                                 ,usuario_nome
                                                                 ,data_prazo
                                                                 ,aprovado
                                                                 ,aprovado_usuario
                                                                 ,aprovado_usuario_nome
                                                                 ,aprovado_usuario_email
                                                                 ,data_aprovado)
                           VALUES (@plano_acao
                                  ,@plano_acao_item
                                  ,@descricao
                                  ,@data_cadastro
                                  ,@data_alteracao
                                  ,@usuario
                                  ,@usuario_email
                                  ,@usuario_nome
                                  ,@data_prazo
                                  ,@aprovado
                                  ,@aprovado_usuario
                                  ,@aprovado_usuario_nome
                                  ,@aprovado_usuario_email
                                  ,@data_aprovado
                           );
                           SELECT @@IDENTITY AS insert_id; ";
            
            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao", observacao.plano_acao, DbType.Int32, ParameterDirection.Input);
                parametros.Add("plano_acao_item", observacao.plano_acao_item, DbType.Int32, ParameterDirection.Input);
                parametros.Add("descricao", observacao.descricao, DbType.String, ParameterDirection.Input);
                parametros.Add("data_cadastro", observacao.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", observacao.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("usuario", observacao.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_email", observacao.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_nome", observacao.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("data_prazo", observacao.data_prazo, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("aprovado", observacao.aprovado, DbType.Int32, ParameterDirection.Input);
                parametros.Add("aprovado_usuario", observacao.aprovado_usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("aprovado_usuario_nome", observacao.aprovado_usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("aprovado_usuario_email", observacao.aprovado_usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("data_aprovado", observacao.data_aprovado, DbType.DateTime, ParameterDirection.Input);

                return (int)connection.Query<dynamic>(sql, parametros).FirstOrDefault().insert_id;
            }
        }

        public IEnumerable<dynamic> dados_plano_acao(long planoAcaoId)
        {
            string sql = @"SELECT id, descricao, marca, data_reuniao 
                             FROM plano_acao (NOLOCK)
					        WHERE id IN (SELECT plano_acao 
                                           FROM plano_acao_item_observacao (NOLOCK)
                                          WHERE id = @id); ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", planoAcaoId, DbType.Int64, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<PlanoAcaoItem_Model> codigos_plano_acao_item(long planoAcaoId)
        {
            string sql = @"SELECT id, plano_acao_item_pai FROM plano_acao_item  (NOLOCK)
					        WHERE plano_acao IN (SELECT plano_acao 
                                                   FROM plano_acao_item_observacao (NOLOCK)
                                                  WHERE id = @id); ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", planoAcaoId, DbType.Int64, ParameterDirection.Input);

                return connection.Query<PlanoAcaoItem_Model>(sql, parametros);
            }
        }

        public IEnumerable<PlanoAcaoItem_Model> dados_plano_acao_item(long planoAcaoId)
        {
            string sql = @"SELECT * 
                             FROM plano_acao_item (NOLOCK)
					        WHERE id IN (SELECT plano_acao_item 
                                           FROM plano_acao_item_observacao (NOLOCK)
                                          WHERE id = @id); ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", planoAcaoId, DbType.Int64, ParameterDirection.Input);

                return connection.Query<PlanoAcaoItem_Model>(sql, parametros);
            }
        }

        public IEnumerable<PlanoAcaoItem_Model> PlanoAcaoItemPorId(long idPlanoAcaoItem)
        {
            string sql = @"SELECT id
                                 ,plano_acao
                                 ,plano_acao_item_pai
                                 ,descricao
                                 ,data_prazo
                             FROM plano_acao_item (NOLOCK)
                            WHERE id = @id; ";
            
            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", idPlanoAcaoItem, DbType.Int64, ParameterDirection.Input);

                return connection.Query<PlanoAcaoItem_Model>(sql, parametros);
            }
        }

        public IEnumerable<PlanoAcaoItem_Model> PlanoAcaoItemPorPlanoAcao(long idPlanoAcao)
        {
            string sql = @"SELECT id, data_prazo
                             FROM plano_acao_item (NOLOCK)
                            WHERE plano_acao = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", idPlanoAcao, DbType.Int64, ParameterDirection.Input);

                return connection.Query<PlanoAcaoItem_Model>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> data_prazo_em_aprovacao(long idObservacao)
        {
            string sql = @"SELECT id FROM plano_acao_item_observacao (NOLOCK)
                            WHERE id = @id 
                              AND data_prazo IS NOT NULL AND aprovado = 0; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", idObservacao, DbType.Int64, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public int ExcluiPlanoAcaoItemObservacao(long idObservacao)
        {
            string sql = @"DELETE FROM plano_acao_item_observacao (NOLOCK) WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", idObservacao, DbType.Int64, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<PlanoAcaoItem_Model> CodigosPlanoAcaoItem(long idPlanoAcaoItem)
        {
            string sql = @"SELECT id, plano_acao_item_pai FROM plano_acao_item (NOLOCK)
			                WHERE plano_acao IN (SELECT plano_acao FROM plano_acao_item (NOLOCK) WHERE id = @id)";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", idPlanoAcaoItem, DbType.Int64, ParameterDirection.Input);

                return connection.Query<PlanoAcaoItem_Model>(sql, parametros);
            }
        }

        public int ExcluirPlanoAcaoItemResponsavel(int idItem)
        {
            string sql = @"DELETE FROM plano_acao_item_responsavel WHERE plano_acao_item = @plano_acao_item; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao_item", idItem, DbType.Int64, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int ExcluirPlanoAcaoItem(long idItem)
        {
            string sql = @"DELETE FROM plano_acao_item WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", idItem, DbType.Int64, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int ExcluirItensPorPlanoAcao(long idPlano)
        {
            string sql = @"DELETE FROM plano_acao_item WHERE plano_acao = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", idPlano, DbType.Int64, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int ExcluirPlanoAcao(long idPlano)
        {
            string sql = @"DELETE FROM plano_acao WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", idPlano, DbType.Int64, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<PlanoAcaoItemObservacao> dados_observacao(long idObs)
        {
            string sql = @"SELECT id
                                 ,plano_acao
                                 ,plano_acao_item
                                 ,descricao
                                 ,data_cadastro
                                 ,data_alteracao
                                 ,usuario
                                 ,usuario_email
                                 ,usuario_nome
                                 ,data_prazo
                                 ,aprovado
                                 ,aprovado_usuario
                                 ,aprovado_usuario_nome
                                 ,aprovado_usuario_email
                                 ,data_aprovado
                             FROM plano_acao_item_observacao (NOLOCK)
                            WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", idObs, DbType.Int64, ParameterDirection.Input);

                return connection.Query<PlanoAcaoItemObservacao>(sql, parametros);
            }
        }

        public int AprovarPlanoAcaoItemObservacao(int aprovado, long usuario, string usuario_nome, string usuario_email, long idItem, DateTime data_prazo)
        {
            string sql = @"
                UPDATE plano_acao_item_observacao 
                   SET aprovado = @aprovado, 
                       data_aprovado = GETDATE(), 
                       aprovado_usuario = @usuario, 
                       aprovado_usuario_nome = @usuario_nome, 
                       aprovado_usuario_email = @usuario_email 
                   WHERE id = @id;
                
                UPDATE plano_acao_item 
                   SET data_prazo = @data_prazo 
                 WHERE id = (SELECT plano_acao_item FROM plano_acao_item_observacao WHERE id = @id);
            ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("aprovado", aprovado, DbType.Int64, ParameterDirection.Input);
                parametros.Add("usuario", usuario, DbType.Int64, ParameterDirection.Input);
                parametros.Add("usuario_nome", usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("data_prazo", data_prazo, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("id", idItem, DbType.Int32, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int ConcluiPlanoAcaoItem(long idItem, int efetividade)
        {
            string sql = @"UPDATE plano_acao_item 
                              SET status = 1, efetividade = @efetividade, data_conclusao = GETDATE() 
                            WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", idItem, DbType.Int64, ParameterDirection.Input);
                parametros.Add("efetividade", efetividade, DbType.Int32, ParameterDirection.Input);
                
                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> EmailRelatorio()
        {
            string sql = @"SELECT id
                                 ,nome
                                 ,email
                                 ,usuario
                                 ,usuario_nome
                                 ,usuario_email
                                 ,emails.data_cadastro
                                 ,emails.data_alteracao
                             FROM emails (NOLOCK); ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> AgendaDatas()
        {
            string sql = @"SELECT DISTINCT data_reuniao
                             FROM agenda  (NOLOCK)
                            WHERE data_reuniao >= GETDATE()
                            ORDER BY data_reuniao ASC";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> InsereAgenda(Agenda_Model agenda)
        {
            string sql = @"INSERT INTO agenda(para
                                             ,avisar
                                             ,fornecedor
                                             ,fornecedor_descricao
                                             ,fornecedor_novo
                                             ,assunto
                                             ,local
                                             ,data_reuniao
                                             ,hora_inicial
                                             ,hora_final
                                             ,observacao
                                             ,invite_enviado
                                             ,data_cadastro
                                             ,data_alteracao
                                             ,usuario
                                             ,usuario_nome
                                             ,usuario_email
                                             ,compras
                                             ,vendas
                                             ,exorbitante
                                             ,recebimento
                                             ,juridico
                                             ,financeiro
                                             ,trocas
                                             ,garantia
                                             ,marketing
                                             ,ti
                                             ,atendimento
                                             ,rh
                                             ,ga
                                             ,gca
                                             ,frete
                                             ,facilities
                                             ,desenvolvimento
                           ) VALUES (
                                             @para
                                            ,@avisar
                                            ,@fornecedor
                                            ,@fornecedor_descricao
                                            ,@fornecedor_novo
                                            ,@assunto
                                            ,@local
                                            ,@data_reuniao
                                            ,@hora_inicial
                                            ,@hora_final
                                            ,@observacao
                                            ,@invite_enviado
                                            ,@data_cadastro
                                            ,@data_alteracao
                                            ,@usuario
                                            ,@usuario_nome
                                            ,@usuario_email
                                            ,@compras
                                            ,@vendas
                                            ,@exorbitante
                                            ,@recebimento
                                            ,@juridico
                                            ,@financeiro
                                            ,@trocas
                                            ,@garantia
                                            ,@marketing
                                            ,@ti
                                            ,@atendimento
                                            ,@rh
                                            ,@ga
                                            ,@gca
                                            ,@frete
                                            ,@facilities
                                            ,@desenvolvimento
                           ); 
                           SELECT @@IDENTITY AS insert_id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("para", agenda.para, DbType.String, ParameterDirection.Input);
                parametros.Add("avisar", agenda.avisar, DbType.String, ParameterDirection.Input);
                parametros.Add("fornecedor", agenda.fornecedor, DbType.Int32, ParameterDirection.Input);
                parametros.Add("fornecedor_descricao", agenda.fornecedor_descricao, DbType.String, ParameterDirection.Input);
                parametros.Add("fornecedor_novo", agenda.fornecedor_novo, DbType.Int32, ParameterDirection.Input);
                parametros.Add("assunto", agenda.assunto, DbType.String, ParameterDirection.Input);
                parametros.Add("local", agenda.local, DbType.String, ParameterDirection.Input);
                parametros.Add("data_reuniao", agenda.data_reuniao, DbType.Date, ParameterDirection.Input);
                parametros.Add("hora_inicial", agenda.hora_inicial.ToString("HH:mm:ss"), DbType.String, ParameterDirection.Input);
                parametros.Add("hora_final", agenda.hora_final.ToString("HH:mm:ss"), DbType.String, ParameterDirection.Input);
                parametros.Add("observacao", agenda.observacao, DbType.String, ParameterDirection.Input);
                parametros.Add("invite_enviado", agenda.invite_enviado, DbType.Int32, ParameterDirection.Input);
                parametros.Add("data_cadastro", agenda.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", agenda.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("usuario", agenda.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_nome", agenda.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", agenda.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("compras", agenda.compras, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("vendas", agenda.vendas, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("exorbitante", agenda.exorbitante, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("recebimento", agenda.recebimento, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("juridico", agenda.juridico, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("financeiro", agenda.financeiro, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("trocas", agenda.trocas, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("garantia", agenda.garantia, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("marketing", agenda.marketing, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("ti", agenda.ti, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("atendimento", agenda.atendimento, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("rh", agenda.rh, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("ga", agenda.ga, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("gca", agenda.gca, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("frete", agenda.frete, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("facilities", agenda.facilities, DbType.Boolean, ParameterDirection.Input);
                parametros.Add("desenvolvimento", agenda.desenvolvimento, DbType.Boolean, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
            
        }

        public int AlteraAgenda(Agenda_Model agenda)
        {
            string sql = @"UPDATE agenda 
                              SET para
                                 ,avisar
                                 ,fornecedor
                                 ,fornecedor_descricao
                                 ,fornecedor_novo
                                 ,assunto
                                 ,local
                                 ,data_reuniao
                                 ,hora_inicial
                                 ,hora_final
                                 ,observacao
                                 ,invite_enviado
                                 ,data_cadastro
                                 ,data_alteracao
                                 ,usuario
                                 ,usuario_nome
                                 ,usuario_email
                                 ,compras
                                 ,vendas
                                 ,exorbitante
                                 ,recebimento
                                 ,juridico
                                 ,financeiro
                                 ,trocas
                                 ,garantia
                                 ,marketing
                                 ,ti
                                 ,atendimento
                                 ,rh
                                 ,ga
                                 ,gca
                                 ,frete
                                 ,facilities
                                 ,desenvolvimento
                            WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("para", agenda.para, DbType.String, ParameterDirection.Input);
                parametros.Add("avisar", agenda.avisar, DbType.String, ParameterDirection.Input);
                parametros.Add("fornecedor", agenda.fornecedor, DbType.Int32, ParameterDirection.Input);
                parametros.Add("fornecedor_descricao", agenda.fornecedor_descricao, DbType.String, ParameterDirection.Input);
                parametros.Add("fornecedor_novo", agenda.fornecedor_novo, DbType.Int32, ParameterDirection.Input);
                parametros.Add("assunto", agenda.assunto, DbType.String, ParameterDirection.Input);
                parametros.Add("local", agenda.local, DbType.String, ParameterDirection.Input);
                parametros.Add("data_reuniao", agenda.data_reuniao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("hora_inicial", agenda.hora_inicial, DbType.Time, ParameterDirection.Input);
                parametros.Add("hora_final", agenda.hora_final, DbType.Time, ParameterDirection.Input);
                parametros.Add("observacao", agenda.observacao, DbType.String, ParameterDirection.Input);
                parametros.Add("invite_enviado", agenda.invite_enviado, DbType.Int32, ParameterDirection.Input);
                parametros.Add("data_cadastro", agenda.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", agenda.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("usuario", agenda.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_nome", agenda.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", agenda.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("compras", agenda.compras, DbType.Int32, ParameterDirection.Input);
                parametros.Add("vendas", agenda.vendas, DbType.Int32, ParameterDirection.Input);
                parametros.Add("exorbitante", agenda.exorbitante, DbType.Int32, ParameterDirection.Input);
                parametros.Add("recebimento", agenda.recebimento, DbType.Int32, ParameterDirection.Input);
                parametros.Add("juridico", agenda.juridico, DbType.Int32, ParameterDirection.Input);
                parametros.Add("financeiro", agenda.financeiro, DbType.Int32, ParameterDirection.Input);
                parametros.Add("trocas", agenda.trocas, DbType.Int32, ParameterDirection.Input);
                parametros.Add("garantia", agenda.garantia, DbType.Int32, ParameterDirection.Input);
                parametros.Add("marketing", agenda.marketing, DbType.Int32, ParameterDirection.Input);
                parametros.Add("ti", agenda.ti, DbType.Int32, ParameterDirection.Input);
                parametros.Add("atendimento", agenda.atendimento, DbType.Int32, ParameterDirection.Input);
                parametros.Add("rh", agenda.rh, DbType.Int32, ParameterDirection.Input);
                parametros.Add("ga", agenda.ga, DbType.Int32, ParameterDirection.Input);
                parametros.Add("gca", agenda.gca, DbType.Int32, ParameterDirection.Input);
                parametros.Add("frete", agenda.frete, DbType.Int32, ParameterDirection.Input);
                parametros.Add("facilities", agenda.facilities, DbType.Int32, ParameterDirection.Input);
                parametros.Add("desenvolvimento", agenda.desenvolvimento, DbType.Int32, ParameterDirection.Input);
                parametros.Add("id", agenda.id, DbType.Int32, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> AgendaPorId(long id)
        {
            string sql = @"SELECT id
                                 ,para
                                 ,avisar
                                 ,fornecedor
                                 ,fornecedor_descricao
                                 ,fornecedor_novo
                                 ,local
                                 ,assunto
                                 ,data_reuniao
                                 ,data_cadastro
                                 ,data_alteracao
                                 ,hora_inicial
                                 ,hora_final
                                 ,observacao
                                 ,invite_enviado
                                 ,usuario
                                 ,usuario_nome
                                 ,usuario_email
                                 ,compras
                                 ,vendas
                                 ,exorbitante
                                 ,recebimento
                                 ,juridico
                                 ,financeiro
                                 ,trocas
                                 ,garantia
                                 ,marketing
                                 ,ti
                                 ,atendimento
                                 ,rh
                                 ,ga
                                 ,gca
                                 ,frete
                                 ,facilities
                                 ,desenvolvimento
                             FROM agenda (NOLOCK)
                            WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public int ExcluirAgendaInteracoes(long id)
        {
            string sql = @"DELETE FROM agenda_interacoes WHERE agenda = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int32, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int ExcluirAgenda(long id)
        {
            string sql = @"DELETE FROM agenda WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int32, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> avaliacao_existente(long agenda, int setor)
        {
            string sql = @"SELECT id FROM agenda_avaliacao (NOLOCK) WHERE agenda = @agenda AND setor = @setor; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("agenda", agenda, DbType.Int64, ParameterDirection.Input);
                parametros.Add("setor", setor, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> InsereAgendaAvaliacao(Agenda_Avaliacao_Model agendaAvaliacao)
        {
            string sql = @"INSERT INTO agenda_avaliacao (
                                agenda
                               ,setor
                               ,nota
                               ,comentario
                               ,usuario
                               ,usuario_nome
                               ,usuario_email
                               ,data_cadastro
                               ,data_alteracao
                           ) VALUES (
                                @agenda
                               ,@setor
                               ,@nota
                               ,@comentario
                               ,@usuario
                               ,@usuario_nome
                               ,@usuario_email
                               ,@data_cadastro
                               ,@data_alteracao
                           ); 
                           SELECT @@IDENTITY AS insert_id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("agenda", agendaAvaliacao.agenda, DbType.Int64, ParameterDirection.Input);
                parametros.Add("setor", agendaAvaliacao.setor, DbType.Int32, ParameterDirection.Input);
                parametros.Add("nota", agendaAvaliacao.nota, DbType.Int32, ParameterDirection.Input);
                parametros.Add("comentario", agendaAvaliacao.comentario, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario", agendaAvaliacao.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_nome", agendaAvaliacao.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", agendaAvaliacao.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("data_cadastro", agendaAvaliacao.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", agendaAvaliacao.data_alteracao, DbType.DateTime, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public int AlteraAgendaAvaliacao(Agenda_Avaliacao_Model agendaAvaliacao)
        {
            string sql = @"UPDATE agenda_avaliacao
                              SET agenda         = @agenda
                                 ,setor          = @setor
                                 ,nota           = @nota
                                 ,comentario     = @comentario
                                 ,usuario        = @usuario
                                 ,usuario_nome   = @usuario_nome
                                 ,usuario_email  = @usuario_email
                                 ,data_cadastro  = @data_cadastro
                                 ,data_alteracao = @data_alteracao
                            WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("agenda", agendaAvaliacao.agenda, DbType.Int64, ParameterDirection.Input);
                parametros.Add("setor", agendaAvaliacao.setor, DbType.Int32, ParameterDirection.Input);
                parametros.Add("nota", agendaAvaliacao.nota, DbType.Int32, ParameterDirection.Input);
                parametros.Add("comentario", agendaAvaliacao.comentario, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario", agendaAvaliacao.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_nome", agendaAvaliacao.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", agendaAvaliacao.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("data_cadastro", agendaAvaliacao.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", agendaAvaliacao.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("id", agendaAvaliacao.id, DbType.Int32, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> AgendaAvaliacoes()
        {
            string sql = @"SELECT id
			                     ,agenda.fornecedor
                                 ,agenda.para
                                 ,agenda.avisar
                                 ,fornecedor_novo
                                 ,assunto
                                 ,local
                                 ,hora_inicial
                                 ,hora_final
                                 ,observacao
                                 ,invite_enviado
                                 ,usuario
                                 ,usuario_nome
                                 ,usuario_email
                                 ,compras
                                 ,vendas
                                 ,exorbitante
                                 ,recebimento
                                 ,juridico
                                 ,financeiro
                                 ,trocas
                                 ,garantia
                                 ,marketing
                                 ,ti
                                 ,atendimento
                                 ,rh
                                 ,ga
                                 ,gca
                                 ,frete
                                 ,facilities
                                 ,desenvolvimento
			                     ,agenda.data_reuniao
			                     ,agenda.data_cadastro
			                     ,agenda.data_alteracao
			                 FROM agenda (NOLOCK)
			                WHERE agenda.id IN (SELECT DISTINCT agenda_avaliacao.agenda FROM agenda_avaliacao (NOLOCK)) 
			                ORDER BY agenda.id DESC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> AgendaAvaliacao(long id)
        {
            string sql = @"SELECT id
                                 ,agenda
                                 ,setor
                                 ,nota
                                 ,comentario
                                 ,usuario
                                 ,usuario_nome
                                 ,usuario_email
                                 ,data_cadastro
                                 ,data_alteracao
                             FROM agenda_avaliacao (NOLOCK)
			                WHERE agenda = @id 
                            ORDER BY setor ASC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> AgendaRotinaAviso()
        {
            string sql = @"SELECT id
                                 ,para
                                 ,avisar
                                 ,fornecedor
                                 ,fornecedor_descricao
                                 ,fornecedor_novo
                                 ,assunto
                                 ,local
                                 ,data_reuniao
                                 ,hora_inicial
                                 ,hora_final
                                 ,observacao
                                 ,invite_enviado
                                 ,data_cadastro
                                 ,data_alteracao
                                 ,usuario
                                 ,usuario_nome
                                 ,usuario_email
                                 ,compras
                                 ,vendas
                                 ,exorbitante
                                 ,recebimento
                                 ,juridico
                                 ,financeiro
                                 ,trocas
                                 ,garantia
                                 ,marketing
                                 ,ti
                                 ,atendimento
                                 ,rh
                                 ,ga
                                 ,gca
                                 ,frete
                                 ,facilities
                                 ,desenvolvimento
                                 ,DATEDIFF(DAY, data_reuniao, GETDATE()) AS qtde_dias 
                             FROM agenda (NOLOCK)
                            WHERE DATEDIFF(DAY, data_reuniao, GETDATE()) BETWEEN 0 AND 5; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }


        public IEnumerable<dynamic> AgendaInteracoes(long id)
        {
            string sql = @"SELECT agenda_interacoes.id
                                 ,agenda_interacoes.agenda
                                 ,agenda_interacoes.setor
                                 ,agenda_interacoes.comentario
                                 ,agenda_interacoes.arquivo
                                 ,agenda_interacoes.usuario
                                 ,agenda_interacoes.usuario_nome
                                 ,agenda_interacoes.usuario_email
                                 ,agenda_interacoes.data_cadastro
                                 ,setor.nome AS nome_setor
                             FROM agenda_interacoes (NOLOCK)
                            INNER JOIN agenda (NOLOCK) ON agenda.id = agenda_interacoes.agenda 
                            INNER JOIN setor (NOLOCK) ON setor.id = agenda_interacoes.setor
                            WHERE agenda_interacoes.agenda = @id
                            ORDER BY agenda_interacoes.data_cadastro ASC";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros)
                    .Select(x => {
                        x.data_cadastro = x.data_cadastro.ToString("dd/MM/yyyy");
                        return x;
                    }).ToList() ;
            }
        }

        public IEnumerable<dynamic> AgendaInteracoesPorId(long id)
        {
            string sql = @"SELECT arquivo FROM agenda_interacoes (NOLOCK) WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public int AtualizaNomeArquivo(long id, string arquivo)
        {
            string sql = @"UPDATE agenda_interacoes SET arquivo = @arquivo WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);
                parametros.Add("arquivo", arquivo, DbType.String, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> Fornecedores()
        {
            string sql = @"SELECT DISTINCT 
                                  TGEN_ENTBAS.ENTB_COD AS id, 
                                  UPPER(LTRIM(RTRIM(TGEN_ENTBAS.ENTB_NOM_RAZ))) AS razao_social 
                             FROM TCOM_CLIFOR (NOLOCK)
                            INNER JOIN TCPR_PROFOR WITH (NOLOCK) ON TCPR_PROFOR.CLIF_COD = TCOM_CLIFOR.CLIF_COD
                            INNER JOIN TGEN_ENTBAS WITH (NOLOCK) ON TGEN_ENTBAS.ENTB_COD = TCOM_CLIFOR.ENTB_COD
                            INNER JOIN TCOM_PROSER WITH (NOLOCK) ON TCPR_PROFOR.PROS_COD = TCOM_PROSER.PROS_COD
                            INNER JOIN TCOM_PROCOD WITH (NOLOCK) ON TCOM_PROSER.PROS_COD = TCOM_PROCOD.PROS_COD
                            INNER JOIN TCOM_CODTIP WITH (NOLOCK) ON TCOM_PROCOD.CODT_COD = TCOM_CODTIP.CODT_COD
                            WHERE TCPR_PROFOR.PROS_COD = TCOM_PROCOD.PROS_COD
                              AND TCOM_PROCOD.CODT_COD in (2, 3, 7, 8, 9); ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> EmailDados(long id)
        {
            string sql = @"SELECT id
                                 ,nome
                                 ,email
                                 ,usuario
                                 ,usuario_nome
                                 ,usuario_email
                                 ,emails.data_cadastro
                                 ,emails.data_alteracao
                             FROM emails (NOLOCK)
                            WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public int EmailDeletar(long id)
        {
            string sql = @"DELETE FROM emails WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int UsuariosDepartamentoDeletar(long id)
        {
            string sql = @"DELETE FROM usuarios WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int64, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public bool VerificaUsuarioExistente(string email)
        {
            string sql = @"SELECT id FROM usuarios (NOLOCK) WHERE email = @email; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("email", email, DbType.String, ParameterDirection.Input);

                List<dynamic> usuarios = (List<dynamic>)connection.Query<dynamic>(sql, parametros);

                return usuarios.Count > 0;
            }
        }

        public long UsuariosDepartamentoInserir(Usuarios_Model usuario)
        {
            string sql = @"INSERT INTO usuarios(
                               nome
                              ,email
                              ,setor
                              ,data_cadastro
                              ,ativo
                              ,usuario
                              ,usuario_nome
                              ,usuario_email
                           ) VALUES (
                               @nome
                              ,@email
                              ,@setor
                              ,@data_cadastro
                              ,@ativo
                              ,@usuario
                              ,@usuario_nome
                              ,@usuario_email
                           );
                           SELECT @@IDENTITY AS insert_id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("nome", usuario.nome, DbType.String, ParameterDirection.Input);
                parametros.Add("email", usuario.email, DbType.String, ParameterDirection.Input);
                parametros.Add("setor", usuario.setor, DbType.Int32, ParameterDirection.Input);
                parametros.Add("data_cadastro", usuario.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("ativo", usuario.ativo, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario", usuario.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_nome", usuario.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", usuario.usuario_email, DbType.String, ParameterDirection.Input);

                return (long)connection.Query<dynamic>(sql, parametros).FirstOrDefault().insert_id;
            }
        }

        public int UsuariosDepartamentoAtualizar(Usuarios_Model usuario)
        {
            string sql = @"UPDATE usuarios
                              SET nome           = @nome
                                 ,email          = @email
                                 ,setor          = @setor
                                 ,data_alteracao = @data_alteracao
                                 ,ativo          = @ativo
                                 ,usuario        = @usuario
                                 ,usuario_nome   = @usuario_nome
                                 ,usuario_email  = @usuario_email
                            WHERE id = @id";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("nome", usuario.nome, DbType.String, ParameterDirection.Input);
                parametros.Add("email", usuario.email, DbType.String, ParameterDirection.Input);
                parametros.Add("setor", usuario.setor, DbType.Int32, ParameterDirection.Input);
                parametros.Add("data_alteracao", usuario.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("ativo", usuario.ativo, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario", usuario.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_nome", usuario.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", usuario.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("id", usuario.id, DbType.Int32, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> setor_nome(int setor)
        {
            string sql = @"SELECT id, nome FROM setor (NOLOCK) WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", setor, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public long EmailsInserir(Emails_Model email)
        {
            string sql = @"INSERT INTO emails(nome
                                             ,email
                                             ,usuario
                                             ,usuario_nome
                                             ,usuario_email
                                             ,data_cadastro
                                             ,data_alteracao
                           ) VALUES (
                                              @nome
                                             ,@email
                                             ,@usuario
                                             ,@usuario_nome
                                             ,@usuario_email
                                             ,@data_cadastro
                                             ,@data_alteracao
                           );
                           SELECT @@IDENTITY AS insert_id;";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("nome", email.nome, DbType.String, ParameterDirection.Input);
                parametros.Add("email", email.email, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario", email.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_nome", email.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", email.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("data_cadastro", email.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", email.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                
                return (long)connection.Query<dynamic>(sql, parametros).FirstOrDefault().insert_id;
            }
        }

        public int EmailsAtualizar(Emails_Model email)
        {
            string sql = @"UPDATE emails
                              SET nome           = @nome
                                 ,email          = @email
                                 ,usuario        = @usuario
                                 ,usuario_nome   = @usuario_nome
                                 ,usuario_email  = @usuario_email
                                 ,data_alteracao = @data_alteracao
                            WHERE id = @id";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("nome", email.nome, DbType.String, ParameterDirection.Input);
                parametros.Add("email", email.email, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario", email.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_nome", email.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", email.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("data_alteracao", email.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("id", email.id, DbType.Int64, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> arquivos_padroes()
        {
            string sql = @"SELECT id
                                 ,nome
                                 ,setor
                                 ,comentario
                                 ,arquivo
                                 ,usuario
                                 ,usuario_nome
                                 ,usuario_email
                                 ,data_cadastro
                                 ,data_alteracao
                             FROM arquivos_padroes (NOLOCK)";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public int ArquivosPadroesInserir(Arquivos_Padroes_Model arquivo)
        {
            string sql = @"INSERT INTO arquivos_padroes(
                               nome
                              ,setor
                              ,comentario
                              ,arquivo
                              ,usuario
                              ,usuario_nome
                              ,usuario_email
                              ,data_cadastro
                              ,data_alteracao
                           ) VALUES (
                               @nome
                              ,@setor
                              ,@comentario
                              ,@arquivo
                              ,@usuario
                              ,@usuario_nome
                              ,@usuario_email
                              ,@data_cadastro
                              ,@data_alteracao
                           );
                           SELECT @@IDENTITY AS insert_id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("nome", arquivo.nome, DbType.String, ParameterDirection.Input);
                parametros.Add("setor", arquivo.setor, DbType.Int32, ParameterDirection.Input);
                parametros.Add("comentario", arquivo.comentario, DbType.String, ParameterDirection.Input);
                parametros.Add("arquivo", arquivo.arquivo, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario", arquivo.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_nome", arquivo.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", arquivo.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("data_cadastro", arquivo.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", arquivo.data_alteracao, DbType.DateTime, ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parametros);
                return (int)query.FirstOrDefault().insert_id;
            }
        }

        public int ArquivosPadroesAtualizar(Arquivos_Padroes_Model arquivo)
        {
            string sql = @"UPDATE arquivos_padroes
                              SET nome           = @nome
                                 ,setor          = @setor
                                 ,comentario     = @comentario
                                 ,arquivo        = @arquivo
                                 ,usuario        = @usuario
                                 ,usuario_nome   = @usuario_nome
                                 ,usuario_email  = @usuario_email
                                 ,data_cadastro  = @data_cadastro
                                 ,data_alteracao = @data_alteracao
                            WHERE id = @id;

                           SELECT @@IDENTITY AS insert_id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("nome", arquivo.nome, DbType.String, ParameterDirection.Input);
                parametros.Add("setor", arquivo.setor, DbType.Int32, ParameterDirection.Input);
                parametros.Add("comentario", arquivo.comentario, DbType.String, ParameterDirection.Input);
                parametros.Add("arquivo", arquivo.arquivo, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario", arquivo.usuario, DbType.Int32, ParameterDirection.Input);
                parametros.Add("usuario_nome", arquivo.usuario_nome, DbType.String, ParameterDirection.Input);
                parametros.Add("usuario_email", arquivo.usuario_email, DbType.String, ParameterDirection.Input);
                parametros.Add("data_cadastro", arquivo.data_cadastro, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("data_alteracao", arquivo.data_alteracao, DbType.DateTime, ParameterDirection.Input);
                parametros.Add("id", arquivo.id, DbType.Int32, ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parametros);
                return query.FirstOrDefault().insert_id;
            }
        }

        public Arquivos_Padroes_Model ArquivosPadroesPorId(int id)
        {
            string sql = @"SELECT id
                                 ,nome
                                 ,setor
                                 ,comentario
                                 ,arquivo
                                 ,usuario
                                 ,usuario_nome
                                 ,usuario_email
                                 ,data_cadastro
                                 ,data_alteracao 
                             FROM arquivos_padroes (NOLOCK)
                            WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int32, ParameterDirection.Input);
                
                return  connection.Query<Arquivos_Padroes_Model>(sql, parametros).FirstOrDefault();
            }
        }

        public int ArquivosPadroesDeletar(int id)
        {
            string sql = @"DELETE FROM arquivos_padroes WHERE id = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", id, DbType.Int32, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> dashboard_atrasados()
        {
            string sql = @"SELECT id 
                             FROM plano_acao_item (NOLOCK)
                            WHERE GETDATE() > data_prazo AND data_conclusao IS NULL; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> QtdEmAndamento(string email)
        {
            string sql = @"SELECT COUNT(id) AS qtde 
                             FROM plano_acao_item (NOLOCK)
                            WHERE EXISTS(SELECT 1 FROM plano_acao_item_responsavel
                                          INNER JOIN usuarios (NOLOCK)
                                             ON usuarios.id = plano_acao_item_responsavel.responsavel
                                          WHERE usuarios.email LIKE CONCAT('%',@responsavel_email,'%')
                                            AND plano_acao_item_responsavel.plano_acao_item = plano_acao_item.id)
                              AND data_conclusao IS NULL 
                              AND GETDATE() <= data_prazo; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("responsavel_email", email, DbType.String, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> QtdConcluido(string email)
        {
            string sql = @"SELECT COUNT(id) AS qtde FROM plano_acao_item (NOLOCK)
                            WHERE EXISTS(SELECT 1 FROM plano_acao_item_responsavel
                                          INNER JOIN usuarios (NOLOCK)
                                             ON usuarios.id = plano_acao_item_responsavel.responsavel
                                          WHERE usuarios.email LIKE CONCAT('%',@responsavel_email,'%')
                                            AND plano_acao_item_responsavel.plano_acao_item = plano_acao_item.id)
                              AND data_conclusao IS NOT NULL; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("responsavel_email", email, DbType.String, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> QtdAtrasados(string email)
        {
            string sql = @"SELECT COUNT(id) AS qtde 
                             FROM plano_acao_item (NOLOCK)
                            WHERE EXISTS(SELECT 1 FROM plano_acao_item_responsavel
                                          INNER JOIN usuarios (NOLOCK)
                                             ON usuarios.id = plano_acao_item_responsavel.responsavel
                                          WHERE usuarios.email LIKE CONCAT('%',@responsavel_email,'%')
                                            AND plano_acao_item_responsavel.plano_acao_item = plano_acao_item.id)
                              AND GETDATE() > data_prazo AND data_conclusao IS NULL; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("responsavel_email", email, DbType.String, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> QtdPlanoAcaoItem()
        {
            string sql = @"SELECT COUNT(id) AS qtde FROM plano_acao_item (NOLOCK); ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> QtdPlanoAcaoItemEmAndamento()
        {
            string sql = @"SELECT COUNT(id) AS qtde FROM plano_acao_item (NOLOCK)
                            WHERE data_conclusao IS NULL AND GETDATE() <= data_prazo; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> QtdPlanoAcaoItemEmAtraso()
        {
            string sql = @"SELECT COUNT(id) AS qtde FROM plano_acao_item (NOLOCK)
                            WHERE GETDATE() > data_prazo AND data_conclusao IS NULL;";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> QtdPlanoAcaoItemConcluidosNoPrazo()
        {
            string sql = @"SELECT COUNT(id) AS qtde FROM plano_acao_item (NOLOCK) WHERE efetividade = 1; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> QtdPlanoAcaoItemConcluidosComAtraso()
        {
            string sql = @"SELECT COUNT(id) AS qtde FROM plano_acao_item (NOLOCK) WHERE efetividade = 2; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> QtdPlanoAcaoItemAguardandoData()
        {
            string sql = @"SELECT COUNT(id) AS qtde FROM plano_acao_item (NOLOCK) WHERE data_prazo IS NULL; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> PlanoAcaoItem()
        {
            string sql = @"SELECT id, setor FROM plano_acao_item (NOLOCK); ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public int? ResponsavelId(string email, string nome, int? ticked)
        {
            string sql = @"SELECT id FROM usuarios (NOLOCK)
                            WHERE email = @email
                              AND nome = @nome
                              AND ISNULL(ticked, 0) = ISNULL(@ticked, 0);";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("email", email, DbType.String, ParameterDirection.Input);
                parametros.Add("nome", nome, DbType.String, ParameterDirection.Input);
                parametros.Add("ticked", ticked, DbType.Int32, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros).FirstOrDefault()?.id;
            }
        }

        public int ResponsavelInserir(string email, string nome, int? ticked)
        {
            string sql = @"INSERT INTO usuarios(email,  nome)
                           VALUES (@email, @nome);

                           SELECT @@IDENTITY AS insert_id;";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("email", email, DbType.String, ParameterDirection.Input);
                parametros.Add("nome", nome, DbType.String, ParameterDirection.Input);
                parametros.Add("ticked", ticked, DbType.Int32, ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parametros);
                return (int)query.FirstOrDefault().insert_id;
            }
        }

        public int PlanoAcaoItemResponsavelInserir(int idPlanoAcaoItem, int idResponsavel)
        {
            string sql = @"INSERT INTO plano_acao_item_responsavel(plano_acao_item
                                                                  ,responsavel)
                           SELECT @plano_acao_item
                                 ,@responsavel
                            WHERE NOT EXISTS(SELECT 1 FROM plano_acao_item_responsavel
                                              WHERE plano_acao_item = @plano_acao_item
                                                AND responsavel = @responsavel); ";
            
            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao_item", idPlanoAcaoItem, DbType.Int32, ParameterDirection.Input);
                parametros.Add("responsavel", idResponsavel, DbType.Int32, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<Responsavel_Model> PlanoAcaoItensResponsavel(int idPlanoAcaoItem)
        {
            string sql = @"SELECT usuarios.id, usuarios.email, usuarios.nome, plano_acao_item_responsavel.ticked
                             FROM plano_acao_item_responsavel (NOLOCK)
                            INNER JOIN usuarios (NOLOCK)
                               ON usuarios.id = plano_acao_item_responsavel.responsavel
                            WHERE plano_acao_item = @plano_acao_item; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("plano_acao_item", idPlanoAcaoItem, DbType.Int32, ParameterDirection.Input);

                return connection.Query<Responsavel_Model>(sql, parametros);
            }
        }

        public Setor SetorPorUsuario(int idUsuario)
        {
            const string sql = @"SELECT setor.id, setor.nome
                                   FROM Sigeco.dbo.usuarios
                                   LEFT OUTER JOIN Sigeco.dbo.setor
                                     ON setor.id = usuarios.setor
                                  WHERE usuarios.id = @idUsuario; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("idUsuario", idUsuario, DbType.Int32, ParameterDirection.Input);

                return connection.Query<Setor>(sql, parametros).FirstOrDefault();
            }
        }


    }
}
