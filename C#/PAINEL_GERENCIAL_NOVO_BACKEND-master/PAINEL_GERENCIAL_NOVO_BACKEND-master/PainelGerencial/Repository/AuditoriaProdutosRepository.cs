using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PainelGerencial.Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public class AuditoriaProdutosRepository : IAuditoriaProdutosRepository
    {
        private readonly string _connectionString;

        public AuditoriaProdutosRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PainelGerencialServer");
        }

        public IEnumerable<dynamic> DadosProduto()
        {
            /* Seleciona os produtos pai */
            string sql = @"SELECT 
                                  [PROS_COD] AS codigo,
                                  [PROS_EXT_COD] AS dk,
                                  [PROS_NOM] AS nome,
                                  [PROS_PES] AS peso,
                                  [CLAP_COD] AS classe,
                                  CASE CLAP_COD
                                    WHEN 1 THEN 'Normal'
                                    WHEN 7 THEN 'Produto Novo'
                                    WHEN 20 THEN 'Anúncio Novo'
                                  END AS classe_nome,
                                  [PROS_CTM_COM] AS comprimento,
                                  [PROS_CTM_LAR] AS largura,
                                  [PROS_CTM_ESP] AS espessura  
                             FROM [ABACOS].[dbo].[TCOM_PROSER] 
                            WHERE PROS_CFB = 'Pai' 
                              AND CLAP_COD IN(1, 7, 20) /* Normal | Produto Novo | Anúncio Novo */";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> DadosProduto(string codigoExterno)
        {
            /* Seleciona os produtos pai */
            const string sql = @"SELECT idproducts AS id,
                                        barCode AS codigo_barras,
                                        dk,
                                        dkDad AS dk_pai,
                                        description AS descricao,
                                        width AS largura,
                                        lenght AS comprimento,
                                        thickness AS espessura,
                                        weight AS peso,
                                        audited AS auditado,
                                        dateAudited AS data_auditoria,
                                        changed AS informacao_alterada,
                                        dateChanged AS data_alteracao,
                                        reports AS gerado_relatorio,
                                        dateReports AS data_geracao_relatorio,
                                        email AS email_envio,
                                        dateSend AS data_envio,
                                        'sim' AS auditado_triagem,
                                        nameProviders AS fornecedor 
                                   FROM Sigeco.dbo.products 
                                  WHERE dk = @dk 
                                  ORDER BY idproducts DESC; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public dynamic DadosAbacos(string codigoExterno)
        {
            const string sql = @"SELECT produto.PROS_CTM_LAR AS largura, 
                                        produto.PROS_CTM_COM AS comprimento,
                                        produto.PROS_CTM_ESP AS espessura, 
                                        produto.PROS_PES AS peso,
                                        RTRIM(LTRIM(classe.CLAP_NOM)) AS classe,
                                        RTRIM(LTRIM(grupo.GRUP_NOM)) AS grupo,
                                        RTRIM(LTRIM(marca.MARP_NOM)) As marca,
                                        RTRIM(LTRIM(sub_grupo.SUBP_NOM)) AS sub_grupo 
                                   FROM TCOM_PROSER AS produto 
                                  INNER JOIN TCOM_SUBPRO AS sub_grupo ON sub_grupo.SUBP_COD = produto.SUBP_COD 
                                  INNER JOIN TCOM_GRUPRO AS grupo ON grupo.GRUP_COD = sub_grupo.GRUP_COD 
                                  INNER JOIN TCOM_CLAPRO AS classe ON classe.CLAP_COD = produto.CLAP_COD
                                  INNER JOIN TCOM_MARPRO AS marca ON marca.MARP_COD = produto.MARP_COD
                                  WHERE produto.PROS_EXT_COD = @dk; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parametros);
                
                if (query.FirstOrDefault().espessura > 100)
                {
                    var comprimento_comp = query.FirstOrDefault().comprimento;

                    query.FirstOrDefault().comprimento = query.FirstOrDefault().espessura;
                    query.FirstOrDefault().espessura = comprimento_comp;
                }

                return query.FirstOrDefault();
            }
        }


        public IEnumerable<dynamic> KitsProduto(string codigoExterno)
        {
            const string sql = @"SELECT produto_pai.PROS_EXT_COD AS dk_produto_pai,
                                        produto_pai.PROS_NOM AS nome_produto_pai, 
                                        produto.PROS_EXT_COD AS dk_produto,
                                        produto.PROS_NOM AS nome_produto  
                                   FROM TCOM_COMPRO AS produto_kit 
                                  INNER JOIN TCOM_PROSER AS produto ON produto.PROS_COD = produto_kit.PROS_COD  
                                  INNER JOIN TCOM_PROSER AS produto_pai ON produto_pai.PROS_COD = produto.PROS_COD_PAI 
                                  WHERE produto_kit.COMP_COD_PRO IN (SELECT PROS_COD FROM TCOM_PROSER WHERE PROS_EXT_COD = @dk) 
                                  ORDER BY produto_pai.PROS_NOM ASC; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }



        public async Task<int> LimparProdutosPaiAsync()
        {
            string sql = @"DELETE FROM Sigeco.dbo.produtos_pai; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return await connection.ExecuteAsync(sql);
            }
        }

        public async Task<int> LimparProdutosFilhosAsync()
        {
            string sql = @"DELETE FROM Sigeco.dbo.produtos_filhos; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return await connection.ExecuteAsync(sql);
            }
        }

        public int InsereProdutoPai(ProdutoPai_Model produto)
        {
            string sql = @"INSERT INTO Sigeco.dbo.produtos_pai(id
                                                              ,dk
                                                              ,nome
                                                              ,comprimento
                                                              ,largura
                                                              ,espessura
                                                              ,peso
                                                              ,classe
                                                              ,classe_nome
                                                              ,estoque)
                           VALUES (@id
                                  ,@dk
                                  ,@nome
                                  ,@comprimento
                                  ,@largura
                                  ,@espessura
                                  ,@peso
                                  ,@classe
                                  ,@classe_nome
                                  ,@estoque
                           ); ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", produto.id, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("dk", produto.dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("nome", produto.nome, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("comprimento", produto.comprimento, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("largura", produto.largura, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("espessura", produto.espessura, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("peso", produto.peso, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("classe", produto.classe, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("classe_nome", produto.classe_nome, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("estoque", produto.estoque, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int InsereProdutoPaiEmLote()
        {
            string sql = @"INSERT INTO Sigeco.dbo.produtos_pai(id, dk, nome, peso, classe, classe_nome, comprimento, largura, espessura)
                           SELECT 
                                  [PROS_COD] AS codigo,
                                  [PROS_EXT_COD] AS dk,
                                  [PROS_NOM] AS nome,
                                  [PROS_PES] AS peso,
                                  [CLAP_COD] AS classe,
                                  CASE CLAP_COD
                                    WHEN 1 THEN 'Normal'
                                    WHEN 7 THEN 'Produto Novo'
                                    WHEN 20 THEN 'Anúncio Novo'
                                  END AS classe_nome,
                                  [PROS_CTM_COM] AS comprimento,
                                  [PROS_CTM_LAR] AS largura,
                                  [PROS_CTM_ESP] AS espessura  
                             FROM [ABACOS].[dbo].[TCOM_PROSER] 
                            WHERE PROS_CFB = 'Pai' 
                              AND CLAP_COD IN(1, 7, 20) /* Normal | Produto Novo | Anúncio Novo */;";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Execute(sql);
            }
        }

        public IEnumerable<dynamic> DksPai()
        {
            string sql = @"SELECT id, dk FROM Sigeco.dbo.produtos_pai; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> DadosProdutoPai(int prosCod)
        {
            string sql = @"SELECT [PROS_COD] AS codigo,
                                  [PROS_EXT_COD] AS dk,
                                  [PROS_NOM] AS nome,
                                  (SELECT TOP(1) TEST_SALPRO.SALP_QTF_DIS FROM [ABACOS].[dbo].[TEST_SALPRO] WHERE TEST_SALPRO.PROS_COD = TCOM_PROSER.PROS_COD AND TEST_SALPRO.ALMO_COD = 8) AS estoque,
                                  [PROS_PES] AS peso,
                                  [PROS_CTM_COM] AS comprimento,
                                  [PROS_CTM_LAR] AS largura,
                                  [PROS_CTM_ESP] AS espessura   
                             FROM [ABACOS].[dbo].[TCOM_PROSER] 
                            WHERE PROS_COD_PAI = @pros_cod
                           	  AND PROS_CHR_REGFISKIT = 0 
                           	  AND PROS_CFB != 'Pai'";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pros_cod", prosCod, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public int InsereProdutoFilho(ProdutoFilho_Model produto)
        {
            string sql = @"INSERT INTO Sigeco.dbo.produtos_filhos(id
                                                                 ,dk
                                                                 ,id_pai
                                                                 ,dk_pai
                                                                 ,nome
                                                                 ,comprimento
                                                                 ,largura
                                                                 ,espessura
                                                                 ,peso
                                                                 ,estoque
                           ) VALUES (
                               @id
                              ,@dk
                              ,@id_pai
                              ,@dk_pai
                              ,@nome
                              ,@comprimento
                              ,@largura
                              ,@espessura
                              ,@peso
                              ,@estoque
                           ); ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", produto.id, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("dk", produto.dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("id_pai", produto.id, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("dk_pai", produto.dk_pai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("nome", produto.nome, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("comprimento", produto.comprimento, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("largura", produto.largura, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("espessura", produto.espessura, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("peso", produto.peso, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("estoque", produto.estoque, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                
                return connection.Execute(sql, parametros);
            }
        }

        public int InsereProdutoFilhoEmLote()
        {
            string sql = @"INSERT INTO Sigeco.dbo.produtos_filhos(id, dk, id_pai, dk_pai, nome, estoque, peso, comprimento, largura, espessura)
                           SELECT [PROS_COD] AS codigo,
                                  [PROS_EXT_COD] AS dk,
                                  PROS_COD_PAI AS id_pai,
                                  produtos_pai.dk AS dk_pai, 
                                  [PROS_NOM] AS nome,
                                  (SELECT TOP(1) TEST_SALPRO.SALP_QTF_DIS FROM [ABACOS].[dbo].[TEST_SALPRO] WHERE TEST_SALPRO.PROS_COD = TCOM_PROSER.PROS_COD AND TEST_SALPRO.ALMO_COD = 8) AS estoque,
                                  [PROS_PES] AS peso,
                                  [PROS_CTM_COM] AS comprimento,
                                  [PROS_CTM_LAR] AS largura,
                                  [PROS_CTM_ESP] AS espessura   
                             FROM [ABACOS].[dbo].[TCOM_PROSER] 
                            INNER JOIN Sigeco.dbo.produtos_pai
                               ON TCOM_PROSER.PROS_COD_PAI = produtos_pai.id
                            WHERE PROS_COD_PAI IN (SELECT id FROM Sigeco.dbo.produtos_pai)
                           	  AND PROS_CHR_REGFISKIT = 0 
                           	  AND PROS_CFB != 'Pai'; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Execute(sql);
            }
        }

        public IEnumerable<ProdutoPai_Model> DadosProdutosPai()
        {
            string sql = @"SELECT id, dk, comprimento, largura, espessura, peso FROM Sigeco.dbo.produtos_pai; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<ProdutoPai_Model>(sql);
            }
        }

        public IEnumerable<ProdutoFilho_Model> DadosProdutoFilho(int paiId)
        {
            string sql = @"SELECT id, dk, nome, comprimento, largura, espessura, peso, estoque 
                            FROM Sigeco.dbo.produtos_filhos 
                           WHERE id_pai = @pai_id; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pai_id", paiId, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return connection.Query<ProdutoFilho_Model>(sql, parametros);
            }
        }

        public int LimpaProdutosDivergentes()
        {
            string sql = @"DELETE FROM Sigeco.dbo.Divergencias_Produtos;
                           DELETE FROM Sigeco.dbo.produtos_divergentes; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Execute(sql);
            }
        }

        public int InsereDivergencia(Divergencia_Produto_Model divergencia, int idProduto)
        {
            string sql = @"INSERT INTO Sigeco.dbo.Divergencias_Produtos(
                              id_Produto
                             ,tipo
                             ,valor_pai
                             ,valor_kit
                             ,diferenca
                           ) VALUES (
                              @id_Produto
                             ,@tipo
                             ,@valor_pai
                             ,@valor_kit
                             ,@diferenca
                           ); ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id_Produto", idProduto, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("tipo", divergencia.tipo, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("valor_pai", divergencia.valor_pai, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("valor_kit", divergencia.valor_kit, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("diferenca", divergencia.diferenca, System.Data.DbType.Double, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int InsereProdutosDivergentes(ProdutoDivergente_Model produto)
        {
            string sql = @"INSERT INTO Sigeco.dbo.produtos_divergentes(id
                                                                      ,dk
                                                                      ,id_pai
                                                                      ,dk_pai
                                                                      ,nome
                                                                      ,estoque
                           ) VALUES (@id
                                    ,@dk
                                    ,@id_pai
                                    ,@dk_pai
                                    ,@nome
                                    ,@estoque
                           );";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id", produto.id, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("dk", produto.dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("id_pai", produto.id_pai, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("dk_pai", produto.dk_pai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("nome", produto.nome, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("estoque", produto.estoque, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                connection.Execute(sql, parametros);

                foreach(var item in produto.divergencias)
                {
                    this.InsereDivergencia(item, produto.id);
                }

                return produto.id;
            }
        }

        public IEnumerable<Divergencia_Produto_Model> ListaDivergencias(int idProduto)
        {
            string sql = @"SELECT id
                                 ,id_Produto
                                 ,tipo
                                 ,valor_pai
                                 ,valor_kit
                                 ,diferenca
                             FROM Sigeco.dbo.Divergencias_Produtos
                            WHERE id_Produto = @produto; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("produto", idProduto, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return connection.Query<Divergencia_Produto_Model>(sql, parametros);
            }
        }

        public IEnumerable<ProdutoDivergente_Model> ListaProdutosDivergentesAsync()
        {
            string sql = @"SELECT produtos_divergentes.id, 
                                  produtos_divergentes.dk,  
                                  produtos_divergentes.nome, 
                                  produtos_divergentes.divergencias,
                                  produtos_divergentes.id_pai, 
                                  produtos_divergentes.dk_pai, 
                                  produtos_pai.nome AS nome_pai,
                                  produtos_pai.classe_nome AS classe,
                                  produtos_divergentes.estoque 
                             FROM Sigeco.dbo.produtos_divergentes 
                            INNER JOIN Sigeco.dbo.produtos_pai 
                               ON produtos_pai.id = produtos_divergentes.id_pai; ";

            using var connection = new SqlConnection(_connectionString);
            {
                var query = connection.Query<dynamic>(sql);

                List<ProdutoDivergente_Model> divergente = new List<ProdutoDivergente_Model>();

                foreach(var item in query)
                {
                    divergente.Add(new ProdutoDivergente_Model()
                    {
                        id = item.id,
                        dk = item.dk,
                        nome = item.nome,
                        divergencias = this.ListaDivergencias(item.id),
                        id_pai = item.id_pai,
                        dk_pai = item.dk_pai,
                        ProdutoPai = new ProdutoPai_Model
                        {
                            nome = item.nome_pai,
                            classe_nome = item.classe
                        }
                    });
                }

                return divergente;
            }
        }

        public bool ProdutoExistente(string codigoExterno)
        {
            string sql = @"SELECT PROS_COD AS dk FROM ABACOS.dbo.TCOM_PROSER WHERE PROS_EXT_COD = @codigo; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("codigo", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query(sql, parametros).Any();
            }
        }

        public IEnumerable<dynamic> Fornecedores(string dk)
        {
            string sql = @"SELECT DISTINCT
                                  TGEN_ENTBAS.ENTB_NOM_RAZ AS razao_social 
                             FROM TCOM_CLIFOR
                            INNER JOIN TCPR_PROFOR WITH (NOLOCK) ON TCPR_PROFOR.CLIF_COD = TCOM_CLIFOR.CLIF_COD
                            INNER JOIN TGEN_ENTBAS WITH (NOLOCK) ON TGEN_ENTBAS.ENTB_COD = TCOM_CLIFOR.ENTB_COD
                            INNER JOIN TCOM_PROSER WITH (NOLOCK) ON TCPR_PROFOR.PROS_COD = TCOM_PROSER.PROS_COD
                            INNER JOIN TCOM_PROCOD WITH (NOLOCK) ON TCOM_PROSER.PROS_COD = TCOM_PROCOD.PROS_COD
                            INNER JOIN TCOM_CODTIP WITH (NOLOCK) ON TCOM_PROCOD.CODT_COD = TCOM_CODTIP.CODT_COD
                            WHERE TCPR_PROFOR.PROS_COD = TCOM_PROCOD.PROS_COD
                              AND TCOM_PROCOD.CODT_COD in (2, 3, 7, 8, 9)
                              AND TCOM_PROSER.PROS_EXT_COD = @dk; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> FornecedoresProduto(string codigoExterno)
        {
            const string sql = @"SELECT TCOM_CLIFOR.CLIF_EXT_COD AS fornecedor_codigo,
                                        TGEN_ENTBAS.ENTB_NOM_RAZ AS fornecedor_nome,
                                        TCPR_PROFOR.PROF_CFB AS fornecedor_codigo_produto,
                                        CASE TCPR_PROFOR.PROF_CHR_SN_PAD
                                          WHEN 'S' THEN 'Sim'
                                          ELSE 'Não'
                                        END AS fornecedor_principal 
                                   FROM TCPR_PROFOR
                                  INNER JOIN TCOM_CLIFOR ON TCOM_CLIFOR.CLIF_COD = TCPR_PROFOR.CLIF_COD
                                  INNER JOIN TGEN_ENTBAS ON TGEN_ENTBAS.ENTB_COD = TCOM_CLIFOR.ENTB_COD
                                  WHERE TCPR_PROFOR.PROS_COD = (SELECT PROS_COD FROM TCOM_PROSER WHERE PROS_EXT_COD = @dk)";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }


        public async Task<IList<ProdutoDivergente_Model>> RelatorioMedidasDivergentes(string dk, string nome)
        {
            string sql = @"SELECT TOP 1000
                                  Divergencias_Produtos.id,
                                  Divergencias_Produtos.id_Produto,
                                  Divergencias_Produtos.tipo,
                                  Divergencias_Produtos.valor_pai,
                                  Divergencias_Produtos.valor_kit,
                                  Divergencias_Produtos.diferenca,
                                  produtos_divergentes.id, 
                                  produtos_divergentes.dk,  
                                  produtos_divergentes.nome, 
                                  produtos_divergentes.id_pai, 
                                  produtos_divergentes.dk_pai, 
                                  produtos_pai.id,
                                  produtos_pai.nome AS nome_produto_pai
                             FROM Sigeco.dbo.Divergencias_Produtos 
                            INNER JOIN Sigeco.dbo.produtos_divergentes 
                               ON Divergencias_Produtos.id_Produto = produtos_divergentes.id
                            INNER JOIN Sigeco.dbo.produtos_pai ON produtos_pai.id = produtos_divergentes.id_pai 
                            WHERE (@dk IS NULL OR produtos_divergentes.dk = @dk)
                              AND (@nome IS NULL OR produtos_divergentes.nome LIKE '%' + @nome + '%') ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("nome", nome, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                List<ProdutoDivergente_Model> datas = new List<ProdutoDivergente_Model>();

                List<Divergencia_Produto_Model> relatorio = (List<Divergencia_Produto_Model>)await connection
                    .QueryAsync<Divergencia_Produto_Model, ProdutoDivergente_Model, ProdutoPai_Model, Divergencia_Produto_Model>(
                    sql: sql,
                    map: (divergencia, produto, pai) => {
                        if (produto.divergencias == null)
                        {
                            produto.divergencias = new List<Divergencia_Produto_Model>();
                        }
                        if (divergencia?.id_Produto == produto?.id)
                        {
                            produto.divergencias.Add(divergencia);
                        }
                        var r = datas.FirstOrDefault(x => x.id == divergencia.id_Produto);
                        if (r != null)
                        {
                            r.divergencias.Add(divergencia);
                        }
                        else
                        {
                            datas.Add(produto);
                        }
                        return divergencia;
                    },
                    param: parametros, 
                    splitOn: "id"
                    );

                

                return datas;
            }
        }


        public IEnumerable<dynamic> RelatorioAuditoria()
        {
            string sql = @"SELECT 
                                  idproducts AS id,
                                  barCode AS codigo_barras,
                                  dk,
                                  dkDad AS dk_pai,
                                  description AS descricao,
                                  width AS largura,
                                  lenght AS comprimento,
                                  thickness AS espessura,
                                  weight AS peso,
                                  audited AS auditado,
                                  dateAudited AS data_auditoria,
                                  changed AS informacao_alterada,
                                  dateChanged AS data_alteracao,
                                  reports AS gerado_relatorio,
                                  dateReports AS data_geracao_relatorio,
                                  email AS email_envio,
                                  dateSend AS data_envio,
                                  nameProviders AS fornecedor 
                             FROM Sigeco.dbo.products (NOLOCK)
                            WHERE changed = 'sim' 
                              AND reports IS NULL 
                            ORDER BY dk DESC ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }


        public IEnumerable<dynamic> Historico(string dk)
        {
            const string sql = @"SELECT id
                                       ,dk
                                       ,nome
                                       ,email
                                       ,data_cadastro
                                       ,dados
                                   FROM Sigeco.dbo.historico
                                  WHERE dk = @dk; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parametros);

                return query.Select(x =>
                {
                    x.dados = JsonConvert.DeserializeObject<dynamic>(x.dados);
                    return x;
                }).ToList();
            }
        }

        public bool ProdutoKit(string codigoExterno)
        {
            const string sql = @"SELECT PROS_CHR_REGFISKIT AS kit FROM TCOM_PROSER WHERE PROS_COD = @id_produto OR PROS_EXT_COD = @id_produto; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id_produto", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros).FirstOrDefault().kit == "1";
            }
        }

        public IEnumerable<dynamic> RelatorioGeral(DateTime? dataInicial, DateTime? dataFinal, int pagina = 1, int offset = 100)
        {
            const string sql = @"SELECT idproducts AS id
                                       ,barCode AS codigo_barras
                                       ,dk
                                       ,dkDad AS dk_pai
                                       ,description AS descricao
                                       ,width AS largura
                                       ,lenght AS comprimento
                                       ,thickness AS espessura
                                       ,weight AS peso
                                       ,audited AS auditado
                                       ,dateAudited AS data_auditoria
                                       ,changed AS informacao_alterada
                                       ,dateChanged AS data_alteracao
                                       ,reports AS gerado_relatorio
                                       ,dateReports AS data_geracao_relatorio
                                       ,email AS email_envio
                                       ,dateSend AS data_envio
                                       ,nameProviders AS fornecedor
                                       ,(SELECT COUNT(1)
                                           FROM Sigeco.dbo.products (NOLOCK)
                                          WHERE 
                                                (
                                                    dateAudited BETWEEN @data_inicial AND @data_final
                                                    OR 
                                                    dateReports BETWEEN @data_inicial AND @data_final 
                                                    OR 
                                                    dateChanged BETWEEN @data_inicial AND @data_final 
                                                )
                                        ) AS QtdRegistros
                                   FROM Sigeco.dbo.products (NOLOCK)
                                  WHERE 
                                        (
                                            dateAudited BETWEEN @data_inicial AND @data_final
                                            OR 
                                            dateReports BETWEEN @data_inicial AND @data_final 
                                            OR 
                                            dateChanged BETWEEN @data_inicial AND @data_final 
                                        )
                                  ORDER BY idproducts
                                 OFFSET (@Pagina - 1) * @ItensPorPagina ROWS
                                  FETCH NEXT @ItensPorPagina ROWS ONLY; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("data_inicial", dataInicial, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("data_final", dataFinal, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("pagina", pagina, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("ItensPorPagina", offset, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parametros);
                return query.Select(x =>
                {
                    x.data_auditoria = x.data_auditoria.ToString("dd/MM/yyyy hh:mm:ss");
                    x.data_alteracao = x.data_alteracao.ToString("dd/MM/yyyy hh:mm:ss");
                    x.data_geracao_relatorio = x.data_geracao_relatorio?.ToString("dd/MM/yyyy hh:mm:ss");
                    x.data_envio = x.data_envio?.ToString("dd/MM/yyyy hh:mm:ss");
                    return x;
                }).ToList();
            }
        }


        public bool VerificaPeso(string dk, float peso)
        {
            const string sql = @"
                SELECT SUM(PROS_PES) AS total_peso 
                  FROM ABACOS.dbo.TCOM_PROSER 
                 WHERE PROS_COD IN (SELECT COMP_COD_PRO AS dks_produtos_kit 
                                      FROM ABACOS.dbo.TCOM_COMPRO 
	                                 INNER JOIN ABACOS.dbo.TCOM_PROSER AS ProdutoKit
	                                    ON ProdutoKit.PROS_COD = TCOM_COMPRO.PROS_COD
                                     WHERE PROS_EXT_COD = @dk AND PROS_CHR_REGFISKIT = 1);
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                var query = connection.Query<dynamic>(sql, parametros);

                if (query.Any(x => x.total_peso != null))
                {
                    return peso > query.FirstOrDefault().total_peso;
                }
                return true;
            }

        }

        public IEnumerable<dynamic> BuscaProdutoCorrigido(int idProduto, string dk)
        {
            const string sql = @"
                SELECT description AS nome
                      ,width AS largura
                      ,lenght AS comprimento
                      ,thickness AS espessura
                      ,weight AS peso
                      ,dkDad AS dk_pai 
                  FROM Sigeco.dbo.products 
                 WHERE dk = @dk 
                   AND idproducts = @id_produto;
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("id_produto", idProduto, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);


                var query = connection.Query<dynamic>(sql, parametros);

                return query.ToList();

            }
        }

        public IEnumerable<dynamic> VerificaKit(string dk)
        {
            const string sql = @"SELECT 
                                COMP_COD_PRO AS dk_kit 
                                FROM TCOM_COMPRO 
                                WHERE PROS_COD = @dk ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parametros);

                return query.ToList();
            }
        }

        public IEnumerable<dynamic> VerificaFilhosKit(string dk)
        {
            const string sql = @"
                SELECT ProdutoKit.PROS_EXT_COD AS dk_kit 
                  FROM ABACOS.dbo.TCOM_COMPRO AS Componente
                 INNER JOIN ABACOS.dbo.TCOM_PROSER AS Produto
                    ON Produto.PROS_COD = Componente.PROS_COD
                  LEFT JOIN ABACOS.dbo.TCOM_PROSER AS ProdutoKit
                    ON ProdutoKit.PROS_COD = Componente.COMP_COD_PRO
                 WHERE Produto.PROS_EXT_COD = @dk;
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parametros);

                return query.ToList();
            }
        }

        public bool CorrigidoAtualizaProser(double largura, double comprimento, double espessura, double peso, List<string> condIn)
        {
            const string sql = @"
                UPDATE ABACOS.dbo.TCOM_PROSER 
                   SET PROS_CTM_LAR = @largura
					  ,PROS_CTM_COM = @comprimento
                      ,PROS_CTM_ESP = @espessura
                      ,PROS_PES     = @peso 
                 WHERE PROS_EXT_COD IN (@condIn);
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("largura", largura, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("comprimento", comprimento, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("espessura", espessura, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("peso", peso, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("condIn", condIn);

                int retorno = connection.Execute(sql, parametros);

                return retorno > 0;
            }
        }

        public bool CorrigidoAtualizaProducts(string dk, int idProduto)
        {
            const string sql = @"
                UPDATE Sigeco.dbo.products 
                   SET reports = 'Corrigido', dateReports = GETDATE(), dateAudited = GETDATE() 
                 WHERE dk = @dk AND idproducts = @idProduto;
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", dk, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("idProduto", idProduto, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                
                int query = connection.Execute(sql, parametros);

                return query > 0;
            }
        }

        public bool InsereHistoricoVtex(Dictionary<string, object> data)
        {
            const string sql = @"
                                INSERT INTO Sigeco.dbo.historico_vtex VALUES(@dk, @nome, @data, @retorno)
                               ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dk", data["dk"], System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("nome", data["nome"], System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("data", DateTime.Now, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("retorno", data["retorno"].ToString(), System.Data.DbType.String, System.Data.ParameterDirection.Input);
               
                var query = connection.Execute(sql, parametros);

                return query > 0;
            }
        }
    }
}
