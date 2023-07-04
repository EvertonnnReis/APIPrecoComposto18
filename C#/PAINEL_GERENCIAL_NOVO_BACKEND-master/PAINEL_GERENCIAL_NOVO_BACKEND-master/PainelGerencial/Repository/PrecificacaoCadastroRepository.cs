using AbacosWSERP;
using AbacosWSPlataforma;
using Dapper;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Configuration;
using PainelGerencial.Domain.PrecificacaoCadastro;
using PainelGerencial.Domain.ShoppingDePrecos;
using PainelGerencial.Domain.Vtex;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public class PrecificacaoCadastroRepository : IPrecificacaoCadastroRepository
    {
        private readonly string _connectionString;

        public PrecificacaoCadastroRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PainelGerencialServer");
        }

        public async Task<IEnumerable<SolicitacoesKit_Model>> SolicitacaoKitPendente(string produtoCodigoExterno)
        {
            const string sql = @"
                SELECT SolicitacoesKit.Codigo
                      ,SolicitacoesKit.ProdutoCodigoExterno
                      ,SolicitacoesKit.ProdutoCodigoExternoPai
                      ,SolicitacoesKit.ProdutoNome
                      ,SolicitacoesKit.ProdutoNomePai
                      ,SolicitacoesKit.ProdutoGrupoCodigo
                      ,SolicitacoesKit.SolicitanteResponsavelEmail
                      ,SolicitacoesKit.AprovadorResponsavelEmail
                      ,SolicitacoesKit.SolicitacaoStatusCodigo
                      ,SolicitacoesKit.Confirmado
                      ,SolicitacoesKit.ProdutoListaPrecoCodigo
                      ,SolicitacoesKit.ConfirmacaoEmail
                      ,SolicitacoesKit.DataAtualizacaoRegistro
                      ,SolicitacoesKit.DataCriacaoRegistro
                      ,SolicitacoesKit.Aprovado
                      ,SolicitacoesKit.Observacao
                      ,SolicitacoesKitComponentes.Codigo
                      ,SolicitacoesKitComponentes.ProdutoCodigoExterno
                      ,SolicitacoesKitComponentes.ProdutoCodigoExternoPai
                      ,SolicitacoesKitComponentes.DataAtualizacaoRegistro
                      ,SolicitacoesKitComponentes.DataCriacaoRegistro
                      ,SolicitacoesKitComponentes.SolicitacaoKit_Codigo
                      ,SolicitacoesKitComponentes.Quantidade
                      ,SolicitacoesKitComponentes.ProdutoNome
                      ,SolicitacoesKitComponentes.ProdutoNomePai
                      ,SolicitacoesKitComponentes.ProdutoListaPrecoCodigo
                      ,SolicitacoesKitComponentes.ProdutoPrecoTabela
                      ,SolicitacoesKitComponentes.ProdutoPrecoPromocional
                      ,SolicitacoesKitComponentes.ProdutoPrecoTabelaNovo
                      ,SolicitacoesKitComponentes.ProdutoPrecoPromocionalNovo
                      ,SolicitacoesKitComponentes.ProdutoPrecoTabelaUnitarioNovo
                      ,SolicitacoesKitComponentes.ProdutoPrecoPromocionalUnitarioNovo
                  FROM PrecificacaoCadastro.dbo.SolicitacoesKit
                 INNER JOIN PrecificacaoCadastro.dbo.SolicitacoesKitComponentes
                    ON SolicitacoesKitComponentes.SolicitacaoKit_Codigo = SolicitacoesKit.Codigo
                 WHERE ISNULL(SolicitacoesKit.Confirmado, 0) = 0 
                   AND SolicitacoesKit.ProdutoCodigoExterno = @ProdutoCodigoExterno
                 ORDER BY SolicitacoesKit.DataCriacaoRegistro;
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("ProdutoCodigoExterno", produtoCodigoExterno, 
                    System.Data.DbType.String, System.Data.ParameterDirection.Input);

                var solicitacoes = new List<SolicitacoesKit_Model>();
                var dados = connection.Query<SolicitacoesKit_Model, SolicitacoesKitComponentes_Model, SolicitacoesKit_Model>(
                    sql: sql,
                    map: (kit, kitComponente) =>
                    {
                        if (kitComponente != null)
                        {
                            if (kit.SolicitacoesKitComponentes == null)
                            {
                                kit.SolicitacoesKitComponentes = new List<SolicitacoesKitComponentes_Model>();
                            }
                            kit.SolicitacoesKitComponentes.Add(kitComponente);
                        }

                        var existeKit = solicitacoes.Where(x => x.Codigo == kit.Codigo).ToList();
                        if (existeKit.Any())
                        {
                            existeKit.FirstOrDefault().SolicitacoesKitComponentes.Add(kitComponente);
                        }
                        else
                        {
                            solicitacoes.Add(kit);
                        }
                        
                        return kit;
                    },
                    param: parametros,
                    splitOn: "Codigo,Codigo"
                );

                return solicitacoes;
            }
        }

        public async Task<IEnumerable<Solicitacao_Model>> ListarSolicitacoes(
            string? ordenarPor, 
            int pagina, 
            int deslocamento,
            string? produtoCodigoExternoPai,
            int solicitacaoTipoCodigo,
            int solicitacaoStatusCodigo,
            int? produtoListaPrecoCodigo,
            int? produtoGrupoCodigo,
            string produtoGrupoCodigos,
            bool somentePrecosReduzidos,
            string solicitanteResponsavelEmail,
            string aprovadorResponsavelEmail,
            DateTime dataInicial,
            DateTime dataFinal,
            bool somentePais
            )
        {
            const string sql = @"
                WITH Consulta AS (
                SELECT ROW_NUMBER() OVER(ORDER BY Codigo) AS linha
                      ,Codigo
                      ,SolicitacaoTipoCodigo
                      ,ProdutoCodigoExterno
                      ,ProdutoCodigoExternoPai
                      ,ProdutoNome
                      ,ProdutoNomePai
                      ,ProdutoGrupoCodigo
                      ,SolicitanteResponsavelEmail
                      ,AprovadorResponsavelEmail
                      ,Observacao
                      ,SolicitacaoStatusCodigo
                      ,ProdutoNomeNovo
                      ,ProdutoNomeReduzidoNovo
                      ,ProdutoDescricaoNova
                      ,ProdutoListaPrecoCodigo
                      ,ProdutoPrecoTabela
                      ,ProdutoPrecoPromocional
                      ,ProdutoPrecoTabelaNovo
                      ,ProdutoPrecoPromocionalNovo
                      ,ProdutoClasse
                      ,ProdutoClasseNova
                      ,DataAtualizacaoRegistro
                      ,DataCriacaoRegistro
                  FROM PrecificacaoCadastro.dbo.Solicitacoes
                 WHERE SolicitacaoTipoCodigo = @SolicitacaoTipoCodigo
                   AND SolicitacaoStatusCodigo = @SolicitacaoStatusCodigo
                   AND (@produtoCodigoExternoPai IS NULL OR ProdutoCodigoExternoPai = @produtoCodigoExternoPai)
                   AND (@ProdutoListaPrecoCodigo IS NULL OR ProdutoListaPrecoCodigo = @ProdutoListaPrecoCodigo)
                   AND (@produtoGrupoCodigo IS NULL OR ProdutoGrupoCodigo = @produtoGrupoCodigo)
               )
               SELECT * 
                 FROM Consulta
                ORDER BY DataCriacaoRegistro
               OFFSET (@pagina - 1) * @deslocamento ROWS
                FETCH NEXT @deslocamento ROWS ONLY;
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("SolicitacaoTipoCodigo", solicitacaoTipoCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("SolicitacaoStatusCodigo", solicitacaoStatusCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("pagina", pagina, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("deslocamento", deslocamento, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoCodigoExternoPai", produtoCodigoExternoPai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoListaPrecoCodigo", produtoListaPrecoCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("produtoGrupoCodigo", produtoGrupoCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                var dados = connection.Query<Solicitacao_Model>(sql, parametros);

                return dados;
            }
        }

        public async Task<IEnumerable<SolicitacoesKit_Model>> ListarSolicitacoesKit(
            string? produtoCodigoExternoPai,
            int? produtoListaPrecoCodigo,
            int? produtoGrupoCodigo,
            string? solicitanteResponsavelEmail,
            string? aprovadorResponsavelEmail,
            int pagina = 1,
            int deslocamento = 50
            )
        {
            const string sql = @"
                SELECT SolicitacoesKit.Codigo
                      ,SolicitacoesKit.ProdutoCodigoExterno
                      ,SolicitacoesKit.ProdutoCodigoExternoPai
                      ,SolicitacoesKit.ProdutoNome
                      ,SolicitacoesKit.ProdutoNomePai
                      ,SolicitacoesKit.ProdutoGrupoCodigo
                      ,SolicitacoesKit.SolicitanteResponsavelEmail
                      ,SolicitacoesKit.AprovadorResponsavelEmail
                      ,SolicitacoesKit.SolicitacaoStatusCodigo
                      ,SolicitacoesKit.Confirmado
                      ,SolicitacoesKit.ProdutoListaPrecoCodigo
                      ,SolicitacoesKit.ConfirmacaoEmail
                      ,SolicitacoesKit.DataAtualizacaoRegistro
                      ,SolicitacoesKit.DataCriacaoRegistro
                      ,SolicitacoesKit.Aprovado
                      ,SolicitacoesKit.Observacao
                      ,SolicitacoesKitComponentes.Codigo
                      ,SolicitacoesKitComponentes.ProdutoCodigoExterno
                      ,SolicitacoesKitComponentes.ProdutoCodigoExternoPai
                      ,SolicitacoesKitComponentes.ProdutoPrecoTabela
                      ,SolicitacoesKitComponentes.ProdutoPrecoTabelaNovo
                      ,SolicitacoesKitComponentes.ProdutoPrecoTabelaUnitarioNovo
                      ,SolicitacoesKitComponentes.ProdutoPrecoPromocional
                      ,SolicitacoesKitComponentes.ProdutoPrecoPromocionalNovo
                      ,SolicitacoesKitComponentes.ProdutoPrecoPromocionalUnitarioNovo
                      ,SolicitacoesKitComponentes.Quantidade
                      ,SolicitacoesKitComponentes.ProdutoListaPrecoCodigo
                  FROM PrecificacaoCadastro.dbo.SolicitacoesKit
                 INNER JOIN PrecificacaoCadastro.dbo.SolicitacoesKitComponentes
                    ON SolicitacoesKitComponentes.SolicitacaoKit_Codigo = SolicitacoesKit.Codigo
                 WHERE Confirmado = 0
                   AND (@solicitanteResponsavelEmail IS NULL OR SolicitacoesKit.SolicitanteResponsavelEmail = @solicitanteResponsavelEmail)
                   AND (@aprovadorResponsavelEmail IS NULL OR SolicitacoesKit.AprovadorResponsavelEmail = @aprovadorResponsavelEmail)
                   AND (@produtoCodigoExternoPai IS NULL OR SolicitacoesKit.ProdutoCodigoExternoPai = @produtoCodigoExternoPai)
                   AND (@ProdutoListaPrecoCodigo IS NULL OR SolicitacoesKit.ProdutoListaPrecoCodigo = @ProdutoListaPrecoCodigo)
                   AND (@produtoGrupoCodigo IS NULL OR SolicitacoesKit.ProdutoGrupoCodigo = @produtoGrupoCodigo)
                   AND SolicitacoesKit.SolicitacaoStatusCodigo = 0
                 ORDER BY SolicitacoesKit.Codigo
                OFFSET (@pagina - 1) * @deslocamento ROWS
                FETCH NEXT @deslocamento ROWS ONLY;
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pagina", pagina, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("deslocamento", deslocamento, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoCodigoExternoPai", produtoCodigoExternoPai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoListaPrecoCodigo", produtoListaPrecoCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("produtoGrupoCodigo", produtoGrupoCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("solicitanteResponsavelEmail", solicitanteResponsavelEmail, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("aprovadorResponsavelEmail", aprovadorResponsavelEmail, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                var retorno = new List<SolicitacoesKit_Model>();
                var dados = await connection.QueryAsync<SolicitacoesKit_Model, SolicitacoesKitComponentes_Model, SolicitacoesKit_Model>(
                    sql: sql, 
                    map: (solicitacao, componente) =>
                    {
                        if (solicitacao.SolicitacoesKitComponentes == null)
                        {
                            solicitacao.SolicitacoesKitComponentes = new List<SolicitacoesKitComponentes_Model>();
                        }
                        solicitacao.SolicitacoesKitComponentes.Add(componente);

                        var existeKit = retorno.Where(x => x.Codigo == solicitacao.Codigo);
                        if (existeKit.Any())
                        {
                            if (existeKit.FirstOrDefault().SolicitacoesKitComponentes == null)
                            {
                                existeKit.FirstOrDefault().SolicitacoesKitComponentes = new List<SolicitacoesKitComponentes_Model>();
                            }
                            existeKit.FirstOrDefault().SolicitacoesKitComponentes.Add(componente);
                        }
                        else
                        {
                            retorno.Add(solicitacao);
                        }
                        return solicitacao;
                    },
                    param: parametros,
                    splitOn: "Codigo");

                return retorno;
            }
        }

        public int ConfirmarSolicitacaoKit(int codigoSolicitacaoKit)
        {
            const string sql = @"
                UPDATE PrecificacaoCadastro.dbo.SolicitacoesKit 
                   SET Confirmado = 1
                      ,DataAtualizacaoRegistro = GETDATE()
                      ,ConfirmacaoEmail = 'bruno.oyamada@connectparts.com.br'
                 WHERE Codigo = @Codigo;
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("Codigo", codigoSolicitacaoKit, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int FinalizaSolicitacaoKit(int codigo)
        {
            const string sql = @"
                UPDATE PrecificacaoCadastro.dbo.SolicitacoesKit
                   SET SolicitacoesKit.SolicitacaoStatusCodigo = 4
                      ,DataAtualizacaoRegistro = GETDATE()
                 WHERE SolicitacoesKit.Codigo = @codigo; 
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("Codigo", codigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int GravaSolicitacaoTemporaria(SolicitacaoTemporaria_Model soliticacao)
        {
            const string sql = @"
                INSERT INTO ConnectParts.dbo.Precificacoes_Temporarias(
                    ProdutoCodigoExterno
                   ,ProdutoCodigoExternoPai
                   ,Email
                   ,ProdutoPai
                   ,Kit
                   ,Grupo
                   ,ProdutoCusto
                   ,ValorDe
                   ,ValorPor
                   ,ValorDefinido
                   ,PercentualAjuste
                   ,TabelaPreco
                   ,ValorMinimo
                   ,TipoKit
                   ,DataAtualizacaoRegistro
                   ,DataCriacaoRegistro
                   ,Observacao
                   ,ValorMinimoCalculado
                ) VALUES (@ProdutoCodigoExterno
                         ,@ProdutoCodigoExternoPai
                         ,@Email
                         ,@ProdutoPai
                         ,@Kit
                         ,@Grupo
                         ,@ProdutoCusto
                         ,@ValorDe
                         ,@ValorPor
                         ,@ValorDefinido
                         ,@PercentualAjuste
                         ,@TabelaPreco
                         ,@ValorMinimo
                         ,@TipoKit
                         ,@DataAtualizacaoRegistro
                         ,@DataCriacaoRegistro
                         ,@Observacao
                         ,@ValorMinimoCalculado);
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("ProdutoCodigoExterno", soliticacao.ProdutoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoCodigoExternoPai", soliticacao.ProdutoCodigoExternoPai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoNome", soliticacao.ProdutoNome, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoNomePai", soliticacao.ProdutoNomePai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("Email", soliticacao.Email, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoPai", soliticacao.ProdutoPai, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                parametros.Add("Kit", soliticacao.Kit, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                parametros.Add("Grupo", soliticacao.Grupo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoCusto", soliticacao.ProdutoCusto, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("ValorDe", soliticacao.ValorDe, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("ValorPor", soliticacao.ValorPor, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("ValorDefinido", soliticacao.ValorDefinido, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("PercentualAjuste", soliticacao.PercentualAjuste, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("TabelaPreco", soliticacao.TabelaPreco, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("ValorMinimo", soliticacao.ValorMinimo, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("TipoKit", soliticacao.TipoKit, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("DataAtualizacaoRegistro", DateTime.Now, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("DataCriacaoRegistro", DateTime.Now, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("Observacao", soliticacao.Observacao, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("ValorMinimoCalculado", soliticacao.ValorMinimoCalculado, System.Data.DbType.Double, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int GerarSolicitacoesTemporarias(string dkProdutoPai, string emailSolicitante, int tipoKit, int listaPreco = 2)
        {
            const string sql = @"
                DECLARE @possuiKit INT = 0, @possuiNaoKit INT = 0, @TipoKit INT = 0, @FilhosKit INT = 0;
                DECLARE @TabelaTmp TABLE (ProdutoCodigoExterno VARCHAR(120)
                                         ,ProdutoCodigoExternoPai VARCHAR(120)
                                         ,Email VARCHAR(120)
                                         ,ProdutoPai BIT
                                         ,Kit BIT
                                         ,Grupo INT
                                         ,ProdutoCusto DECIMAL(12,2)
                                         ,ValorDe DECIMAL(12,2)
                                         ,ValorPor DECIMAL(12,2)
                                         ,ValorDefinido DECIMAL(12,2)
                                         ,PercentualAjuste DECIMAL(8,4)
                                         ,TabelaPreco INT
                                         ,ValorMinimo DECIMAL(12,2)
                                         ,TipoKit INT
                                         ,DataAtualizacaoRegistro DATETIME
                                         ,DataCriacaoRegistro DATETIME
                                         ,Observacao INT
                                         ,ValorMinimoCalculado DECIMAL(12,2));
                
                SELECT @possuiKit = 1
                  FROM ABACOS.dbo.TCOM_PROSER AS Produto
                  LEFT OUTER JOIN ABACOS.dbo.TCOM_PROSER AS ProdutoPai
                    ON ProdutoPai.PROS_COD = Produto.PROS_COD_PAI 
                 WHERE ProdutoPai.PROS_EXT_COD = @produtoPai
                   AND Produto.PROS_CHR_SN_COM = 'S';
                
                SELECT @possuiNaoKit = 1
                  FROM ABACOS.dbo.TCOM_PROSER AS Produto
                  LEFT OUTER JOIN ABACOS.dbo.TCOM_PROSER AS ProdutoPai
                    ON ProdutoPai.PROS_COD = Produto.PROS_COD_PAI 
                 WHERE ProdutoPai.PROS_EXT_COD = @produtoPai
                   AND Produto.PROS_CHR_SN_COM = 'N'
                   AND ProdutoPai.PROS_EXT_COD <> produto.PROS_EXT_COD;

                SELECT @FilhosKit = 1
                  FROM ABACOS.dbo.TCOM_PROSER AS Produto
                  INNER JOIN ABACOS.dbo.TCOM_PROSER AS ProdutoPai
                    ON ProdutoPai.PROS_COD = Produto.PROS_COD_PAI 
                 INNER JOIN ABACOS.dbo.TCOM_COMPRO 
                    ON Produto.PROS_COD = TCOM_COMPRO.PROS_COD
                 WHERE ProdutoPai.PROS_EXT_COD = @produtoPai
                   AND ProdutoPai.PROS_EXT_COD <> produto.PROS_EXT_COD;

                IF (@possuiKit = 1 AND @possuiNaoKit = 1 AND @FilhosKit = 0)
                BEGIN
                    SET @TipoKit = 1;
                END
                ELSE IF (@possuiKit = 1 AND @possuiNaoKit = 0 AND @FilhosKit = 0)
                BEGIN
                    SET @TipoKit = 2;
                END
                ELSE IF (@possuiKit = 0 AND @possuiNaoKit = 1 AND @FilhosKit = 0)
                BEGIN
                    SET @TipoKit = 3;
                END
                ELSE IF (@FilhosKit = 1)
                BEGIN
                    SET @TipoKit = 5;
                END
                
                INSERT INTO Connectparts.dbo.Precificacoes_Temporarias(
                            ProdutoCodigoExterno
                           ,ProdutoCodigoExternoPai
                           ,Email
                           ,ProdutoPai
                           ,Kit
                           ,Grupo
                           ,ProdutoCusto
                           ,ValorDe
                           ,ValorPor
                           ,ValorDefinido
                           ,PercentualAjuste
                           ,TabelaPreco
                           ,ValorMinimo
                           ,TipoKit
                           ,DataAtualizacaoRegistro
                           ,DataCriacaoRegistro
                           ,Observacao
                           ,ValorMinimoCalculado)
                SELECT TCOM_PROSER.PROS_EXT_COD
                      ,Pai.PROS_EXT_COD
                      ,@emailSolicitante
                      ,CASE WHEN TCOM_PROSER.PROS_COD_PAI IS NULL THEN 0 ELSE 1 END AS ProdutoPai
	                  ,CASE WHEN TCOM_PROSER.PROS_CFB = 'Kit' THEN 1 ELSE 0 END AS Kit
	                  ,TCOM_GRUPRO.GRUP_COD
	                  ,ROUND(ISNULL(CASE TCOM_PROSER.pros_chr_sn_com
                                      WHEN 'S' THEN ABACOS.dbo.Fest_s_custotkit(TCOM_PROSER.pros_cod, 8,
                                                  'B', 'N')
                                      ELSE TEST_SALPRO.SALP_VAL_CUSBRU
                                    END, 0), 2)                       AS CustoBruto
                      ,TCOM_PROLIS.PROL_VAL_PRE
                      ,TCOM_PROLIS.PROL_VAL_PREPRO
                      ,0 AS ValorDefinido
                      ,0 AS PercentualAjuste
                      ,TCOM_PROLIS.LISP_COD
                      ,ISNULL((SELECT SUM(ISNULL(PrecoComponente.PROL_VAL_PREPRO * TCOM_COMPRO.COMP_QTF, 0))
                                 FROM ABACOS.DBO.TCOM_COMPRO WITH (NOLOCK) 
                                 LEFT JOIN ABACOS.DBO.TCOM_PROSER AS ProdutoComponente WITH (NOLOCK) 
                                   ON ProdutoComponente.PROS_COD = TCOM_COMPRO.COMP_COD_PRO
                                 LEFT JOIN ABACOS.dbo.TCOM_PROLIS AS PrecoComponente WITH (NOLOCK) 
                                   ON ProdutoComponente.PROS_COD = PrecoComponente.PROS_COD 
                                  AND PrecoComponente.LISP_COD = @listaPreco
                                WHERE TCOM_COMPRO.PROS_COD = TCOM_PROSER.PROS_COD), 0) AS ValorMinimo
                      ,@TipoKit AS TipoKit
	                  ,GETDATE() AS DataAtualizacaoRegistro
                      ,GETDATE() AS DataCriacaoRegistro
                      ,NULL AS Observacao
	                  ,(SELECT SUM(ISNULL(PrecoComponente.PROL_VAL_PREPRO * TCOM_COMPRO.COMP_QTF, 0))
                          FROM ABACOS.DBO.TCOM_COMPRO WITH (NOLOCK) 
                          LEFT JOIN ABACOS.DBO.TCOM_PROSER AS ProdutoComponente WITH (NOLOCK) 
                            ON ProdutoComponente.PROS_COD = TCOM_COMPRO.COMP_COD_PRO
                          LEFT JOIN ABACOS.dbo.TCOM_PROLIS AS PrecoComponente WITH (NOLOCK) 
                            ON ProdutoComponente.PROS_COD = PrecoComponente.PROS_COD 
                           AND PrecoComponente.LISP_COD = @listaPreco
                         WHERE TCOM_COMPRO.PROS_COD = TCOM_PROSER.PROS_COD) AS ValorMinimoCalculado
                  FROM ABACOS.dbo.TCOM_PROSER (NOLOCK)
                 INNER JOIN ABACOS.dbo.TCOM_PROLIS (NOLOCK) ON (TCOM_PROLIS.PROS_COD = TCOM_PROSER.PROS_COD)
                 INNER JOIN ABACOS.dbo.TCOM_FAMPRO (NOLOCK) ON (TCOM_PROSER.FAMP_COD = TCOM_FAMPRO.FAMP_COD)
                 INNER JOIN ABACOS.dbo.TCOM_SUBPRO (NOLOCK) ON (TCOM_PROSER.SUBP_COD = TCOM_SUBPRO.SUBP_COD)
                 INNER JOIN ABACOS.dbo.TCOM_GRUPRO (NOLOCK) ON (TCOM_SUBPRO.GRUP_COD = TCOM_GRUPRO.GRUP_COD)
                 INNER JOIN ABACOS.dbo.TCOM_MARPRO (NOLOCK) ON (TCOM_PROSER.MARP_COD = TCOM_MARPRO.MARP_COD)
                 INNER JOIN ABACOS.dbo.TCOM_CLAPRO (NOLOCK) ON (TCOM_PROSER.CLAP_COD = TCOM_CLAPRO.CLAP_COD)
                 INNER JOIN ABACOS.dbo.TCOM_LISPRE (NOLOCK) ON (TCOM_PROLIS.LISP_COD = TCOM_LISPRE.LISP_COD)
                 INNER JOIN ABACOS.dbo.TGEN_UNIMON (NOLOCK) ON (TCOM_LISPRE.UNIM_COD = TGEN_UNIMON.UNIM_COD)
                 INNER JOIN ABACOS.dbo.TACE_USUSIS (NOLOCK) ON (TCOM_PROLIS.USUS_COD = TACE_USUSIS.USUS_COD)
                  LEFT JOIN ABACOS.dbo.TCOM_PRCCOM (NOLOCK) ON (TCOM_PROLIS.PROS_COD = TCOM_PRCCOM.PROS_COD)
                  LEFT JOIN ABACOS.dbo.TEST_SALPRO
                                    ON TEST_SALPRO.pros_cod = TCOM_PROSER.pros_cod
                                   AND TEST_SALPRO.almo_cod = 8
                  LEFT JOIN ABACOS.dbo.TCOM_PROSER AS Pai (NOLOCK) ON (TCOM_PROSER.PROS_COD_PAI = Pai.PROS_COD)
                 WHERE (EXISTS (SELECT 1 
                                  FROM ABACOS.dbo.TCOM_LISUNI X (NOLOCK) 
                                 INNER JOIN ABACOS.dbo.TGEN_UNINEG Y (NOLOCK) ON (X.UNIN_COD = Y.UNIN_COD)
                                 WHERE (X.LISP_COD=TCOM_PROLIS.LISP_COD)
                                   AND (Y.EMPR_COD=1)))
                   AND ((NOT EXISTS (SELECT 1 
                                       FROM ABACOS.dbo.TCOM_USULIS X (NOLOCK) 
                                      INNER JOIN ABACOS.dbo.TACE_USUSIS Y (NOLOCK) ON (X.USUS_COD = Y.USUS_COD)
                                      WHERE (LISP_COD=TCOM_PROLIS.LISP_COD) AND (Y.USUS_DAT_FIM IS NULL))) OR
                        (EXISTS (SELECT 1 FROM ABACOS.dbo.TCOM_USULIS (NOLOCK) WHERE LISP_COD=TCOM_PROLIS.LISP_COD)))

                   AND (TCOM_PROLIS.LISP_COD = @listaPreco) 

                   AND ((SELECT PROS_EXT_COD FROM ABACOS.dbo.TCOM_PROSER AS Produto WHERE PROS_COD=TCOM_PROSER.PROS_COD_PAI)=@produtoPai) 

                   AND (TCOM_PROSER.PROS_DAT_FIM IS NULL);


                -- Produtos que contém o pai como componente
                INSERT INTO @TabelaTmp(
                            ProdutoCodigoExterno
                           ,ProdutoCodigoExternoPai
                           ,Email
                           ,ProdutoPai
                           ,Kit
                           ,Grupo
                           ,ProdutoCusto
                           ,ValorDe
                           ,ValorPor
                           ,ValorDefinido
                           ,PercentualAjuste
                           ,TabelaPreco
                           ,ValorMinimo
                           ,TipoKit
                           ,DataAtualizacaoRegistro
                           ,DataCriacaoRegistro
                           ,Observacao
                           ,ValorMinimoCalculado)
                SELECT ProdutoKit.PROS_EXT_COD
                      ,ProdutoPai.PROS_EXT_COD
                      ,@emailSolicitante
                      ,CASE WHEN ProdutoKit.PROS_COD_PAI IS NULL THEN 0 ELSE 1 END AS ProdutoPai
	                  ,CASE WHEN ProdutoKit.PROS_CFB = 'Kit' THEN 1 ELSE 0 END AS Kit
	                  ,TCOM_GRUPRO.GRUP_COD
	                  ,ROUND(ISNULL(CASE ProdutoKit.pros_chr_sn_com
                                      WHEN 'S' THEN ABACOS.dbo.Fest_s_custotkit(ProdutoKit.PROS_COD, 8,
                                                  'B', 'N')
                                      ELSE TEST_SALPRO.SALP_VAL_CUSBRU
                                    END, 0), 2)                       AS CustoBruto
                      ,TCOM_PROLIS.PROL_VAL_PRE
                      ,TCOM_PROLIS.PROL_VAL_PREPRO
                      ,0 AS ValorDefinido
                      ,0 AS PercentualAjuste
                      ,TCOM_PROLIS.LISP_COD
                      ,ISNULL((SELECT SUM(ISNULL(PrecoComponente.PROL_VAL_PREPRO * TCOM_COMPRO.COMP_QTF, 0))
                                 FROM ABACOS.DBO.TCOM_COMPRO WITH (NOLOCK) 
                                 LEFT JOIN ABACOS.DBO.TCOM_PROSER AS ProdutoComponente WITH (NOLOCK) 
                                   ON ProdutoComponente.PROS_COD = TCOM_COMPRO.COMP_COD_PRO
                                 LEFT JOIN ABACOS.dbo.TCOM_PROLIS AS PrecoComponente WITH (NOLOCK) 
                                   ON ProdutoComponente.PROS_COD = PrecoComponente.PROS_COD 
                                  AND PrecoComponente.LISP_COD = @listaPreco
                                WHERE TCOM_COMPRO.PROS_COD = ProdutoKit.PROS_COD), 0) AS ValorMinimo
                      ,@TipoKit AS TipoKit
	                  ,GETDATE() AS DataAtualizacaoRegistro
                      ,GETDATE() AS DataCriacaoRegistro
                      ,NULL AS Observacao
	                  ,CASE WHEN (SELECT ISNULL(SUM(KITP_VAL_PREPRO * KITP_QTF), 0)
                                    FROM ABACOS.dbo.FCOM_M_KITPRE(ProdutoKit.PROS_COD, @listaPreco)) > 0 THEN
                         (SELECT ISNULL(SUM(KITP_VAL_PREPRO * KITP_QTF), 0)
                           FROM ABACOS.dbo.FCOM_M_KITPRE(ProdutoKit.PROS_COD, @listaPreco))
                       ELSE (SELECT SUM(ISNULL(PrecoComponente.PROL_VAL_PREPRO * TCOM_COMPRO.COMP_QTF, 0))
                               FROM ABACOS.DBO.TCOM_COMPRO WITH (NOLOCK) 
                               LEFT JOIN ABACOS.DBO.TCOM_PROSER AS ProdutoComponente WITH (NOLOCK) 
                                 ON ProdutoComponente.PROS_COD = TCOM_COMPRO.COMP_COD_PRO
                               LEFT JOIN ABACOS.dbo.TCOM_PROLIS AS PrecoComponente WITH (NOLOCK) 
                                 ON ProdutoComponente.PROS_COD = PrecoComponente.PROS_COD 
                                AND PrecoComponente.LISP_COD = @listaPreco
                              WHERE TCOM_COMPRO.PROS_COD = ProdutoKit.PROS_COD)
                       END AS ValorMinimoCalculado
                  FROM ABACOS.dbo.TCOM_PROSER ProdutoKit (NOLOCK)
                 INNER JOIN ABACOS.dbo.TCOM_PROLIS (NOLOCK) 
                    ON (TCOM_PROLIS.PROS_COD = ProdutoKit.PROS_COD)
                   AND TCOM_PROLIS.LISP_COD = @listaPreco
                 INNER JOIN ABACOS.dbo.TCOM_PROSER ProdutoPai (NOLOCK)
                    ON ProdutoPai.PROS_COD = ProdutoKit.PROS_COD_PAI
                 INNER JOIN ABACOS.dbo.TCOM_SUBPRO (NOLOCK) ON (ProdutoKit.SUBP_COD = TCOM_SUBPRO.SUBP_COD)
                 INNER JOIN ABACOS.dbo.TCOM_GRUPRO (NOLOCK) ON (TCOM_SUBPRO.GRUP_COD = TCOM_GRUPRO.GRUP_COD)
                  LEFT JOIN ABACOS.dbo.TEST_SALPRO
                    ON TEST_SALPRO.PROS_COD = ProdutoKit.PROS_COD
                   AND TEST_SALPRO.ALMO_COD = 8
                 WHERE (1=1)
                   AND ProdutoKit.PROS_DAT_FIM IS NULL
                   AND EXISTS(SELECT 1 FROM ABACOS.dbo.TCOM_COMPRO Componentes (NOLOCK) 
                               INNER JOIN ABACOS.dbo.TCOM_PROSER ProdutoComponente (NOLOCK) 
                                  ON (Componentes.COMP_COD_PRO = ProdutoComponente.PROS_COD) 
                               WHERE Componentes.PROS_COD = ProdutoKit.PROS_COD
                                 AND ProdutoComponente.PROS_EXT_COD IN (SELECT ProdutoCodigoExterno 
                                                                          FROM CONNECTPARTS.dbo.Precificacoes_Temporarias
                                                                         WHERE Email = @emailSolicitante) )
                   AND NOT EXISTS (SELECT 1 
                                     FROM connectparts.dbo.Precificacoes_Temporarias 
                                    WHERE ProdutoKit.PROS_EXT_COD = Precificacoes_Temporarias.ProdutoCodigoExterno
                                      AND Email = @emailSolicitante);

              -- Pais dos kits
              INSERT INTO @TabelaTmp (
                          ProdutoCodigoExterno
                         ,ProdutoCodigoExternoPai
                         ,Email
                         ,ProdutoPai
                         ,Kit
                         ,Grupo
                         ,ProdutoCusto
                         ,ValorDe
                         ,ValorPor
                         ,ValorDefinido
                         ,PercentualAjuste
                         ,TabelaPreco
                         ,ValorMinimo
                         ,TipoKit
                         ,DataAtualizacaoRegistro
                         ,DataCriacaoRegistro
                         ,Observacao
                         ,ValorMinimoCalculado)
              SELECT TCOM_PROSER.PROS_EXT_COD
                    ,Pai.PROS_EXT_COD
                    ,@emailSolicitante
                    ,CASE WHEN TCOM_PROSER.PROS_COD_PAI IS NULL THEN 0 ELSE 1 END AS ProdutoPai
                    ,CASE WHEN TCOM_PROSER.PROS_CFB = 'Kit' THEN 1 ELSE 0 END AS Kit
                    ,TCOM_GRUPRO.GRUP_COD
                    ,ROUND(ISNULL(CASE TCOM_PROSER.pros_chr_sn_com
                                    WHEN 'S' THEN 
                                      ABACOS.dbo.Fest_s_custotkit(TCOM_PROSER.pros_cod, 8,
                                                                  'B', 'N')
                                    ELSE TEST_SALPRO.SALP_VAL_CUSBRU
                                  END, 0), 2)                       AS CustoBruto
                    ,TCOM_PROLIS.PROL_VAL_PRE
                    ,TCOM_PROLIS.PROL_VAL_PREPRO
                    ,0 AS ValorDefinido
                    ,0 AS PercentualAjuste
                    ,TCOM_PROLIS.LISP_COD
                    ,ISNULL((SELECT SUM(ISNULL(PrecoComponente.PROL_VAL_PREPRO * TCOM_COMPRO.COMP_QTF, 0))
                               FROM ABACOS.DBO.TCOM_COMPRO WITH (NOLOCK) 
                               LEFT JOIN ABACOS.DBO.TCOM_PROSER AS ProdutoComponente WITH (NOLOCK) 
                                 ON ProdutoComponente.PROS_COD = TCOM_COMPRO.COMP_COD_PRO
                               LEFT JOIN ABACOS.dbo.TCOM_PROLIS AS PrecoComponente WITH (NOLOCK) 
                                 ON ProdutoComponente.PROS_COD = PrecoComponente.PROS_COD 
                                AND PrecoComponente.LISP_COD = @listaPreco
                              WHERE TCOM_COMPRO.PROS_COD = TCOM_PROSER.PROS_COD), 0) AS ValorMinimo
                    ,@TipoKit AS TipoKit
                    ,GETDATE() AS DataAtualizacaoRegistro
                    ,GETDATE() AS DataCriacaoRegistro
                    ,NULL AS Observacao
                    ,(SELECT SUM(ISNULL(PrecoComponente.PROL_VAL_PREPRO * TCOM_COMPRO.COMP_QTF, 0))
                        FROM ABACOS.DBO.TCOM_COMPRO WITH (NOLOCK) 
                        LEFT JOIN ABACOS.DBO.TCOM_PROSER AS ProdutoComponente WITH (NOLOCK) 
                          ON ProdutoComponente.PROS_COD = TCOM_COMPRO.COMP_COD_PRO
                        LEFT JOIN ABACOS.dbo.TCOM_PROLIS AS PrecoComponente WITH (NOLOCK) 
                          ON ProdutoComponente.PROS_COD = PrecoComponente.PROS_COD 
                         AND PrecoComponente.LISP_COD = @listaPreco
                       WHERE TCOM_COMPRO.PROS_COD = TCOM_PROSER.PROS_COD) AS ValorMinimoCalculado
                FROM ABACOS.dbo.TCOM_PROSER (NOLOCK)
               INNER JOIN ABACOS.dbo.TCOM_PROLIS (NOLOCK) ON (TCOM_PROLIS.PROS_COD = TCOM_PROSER.PROS_COD)
               INNER JOIN ABACOS.dbo.TCOM_FAMPRO (NOLOCK) ON (TCOM_PROSER.FAMP_COD = TCOM_FAMPRO.FAMP_COD)
               INNER JOIN ABACOS.dbo.TCOM_SUBPRO (NOLOCK) ON (TCOM_PROSER.SUBP_COD = TCOM_SUBPRO.SUBP_COD)
               INNER JOIN ABACOS.dbo.TCOM_GRUPRO (NOLOCK) ON (TCOM_SUBPRO.GRUP_COD = TCOM_GRUPRO.GRUP_COD)
               INNER JOIN ABACOS.dbo.TCOM_MARPRO (NOLOCK) ON (TCOM_PROSER.MARP_COD = TCOM_MARPRO.MARP_COD)
               INNER JOIN ABACOS.dbo.TCOM_CLAPRO (NOLOCK) ON (TCOM_PROSER.CLAP_COD = TCOM_CLAPRO.CLAP_COD)
               INNER JOIN ABACOS.dbo.TCOM_LISPRE (NOLOCK) ON (TCOM_PROLIS.LISP_COD = TCOM_LISPRE.LISP_COD)
               INNER JOIN ABACOS.dbo.TGEN_UNIMON (NOLOCK) ON (TCOM_LISPRE.UNIM_COD = TGEN_UNIMON.UNIM_COD)
               INNER JOIN ABACOS.dbo.TACE_USUSIS (NOLOCK) ON (TCOM_PROLIS.USUS_COD = TACE_USUSIS.USUS_COD)
                LEFT JOIN ABACOS.dbo.TCOM_PRCCOM (NOLOCK) ON (TCOM_PROLIS.PROS_COD = TCOM_PRCCOM.PROS_COD)
                LEFT JOIN ABACOS.dbo.TEST_SALPRO
                                  ON TEST_SALPRO.pros_cod = TCOM_PROSER.pros_cod
                                 AND TEST_SALPRO.almo_cod = 8
                LEFT JOIN ABACOS.dbo.TCOM_PROSER AS Pai (NOLOCK) ON (TCOM_PROSER.PROS_COD_PAI = Pai.PROS_COD)
               WHERE (EXISTS (SELECT 1 
                                FROM ABACOS.dbo.TCOM_LISUNI X (NOLOCK) 
                               INNER JOIN ABACOS.dbo.TGEN_UNINEG Y (NOLOCK) ON (X.UNIN_COD = Y.UNIN_COD)
                               WHERE (X.LISP_COD=TCOM_PROLIS.LISP_COD)
                                 AND (Y.EMPR_COD=1)))
                 AND ((NOT EXISTS (SELECT 1 
                                     FROM ABACOS.dbo.TCOM_USULIS X (NOLOCK) 
                                    INNER JOIN ABACOS.dbo.TACE_USUSIS Y (NOLOCK) ON (X.USUS_COD = Y.USUS_COD)
                                    WHERE (LISP_COD=TCOM_PROLIS.LISP_COD) AND (Y.USUS_DAT_FIM IS NULL))) OR
                      (EXISTS (SELECT 1 FROM ABACOS.dbo.TCOM_USULIS (NOLOCK) WHERE LISP_COD=TCOM_PROLIS.LISP_COD)))

                 AND (TCOM_PROLIS.LISP_COD = @listaPreco) 

                 -- Pais dos kits
                 -- inicio
                 AND EXISTS(SELECT 1
                             FROM @TabelaTmp
                            WHERE Pai.PROS_EXT_COD COLLATE SQL_Latin1_General_CP1_CI_AS = ProdutoCodigoExternoPai COLLATE SQL_Latin1_General_CP1_CI_AS
                              AND NOT EXISTS(SELECT 1 FROM @TabelaTmp TabelaAux 
                                              WHERE TabelaAux.ProdutoCodigoExterno COLLATE DATABASE_DEFAULT = TCOM_PROSER.PROS_EXT_COD COLLATE DATABASE_DEFAULT))
                 -- fim
                 

                 AND (TCOM_PROSER.PROS_DAT_FIM IS NULL)

                 AND NOT EXISTS (SELECT 1 
                                   FROM connectparts.dbo.Precificacoes_Temporarias 
                                  WHERE TCOM_PROSER.PROS_EXT_COD = Precificacoes_Temporarias.ProdutoCodigoExterno
                                    AND Email = @emailSolicitante);

                INSERT INTO Connectparts.dbo.Precificacoes_Temporarias(
                            ProdutoCodigoExterno
                           ,ProdutoCodigoExternoPai
                           ,Email
                           ,ProdutoPai
                           ,Kit
                           ,Grupo
                           ,ProdutoCusto
                           ,ValorDe
                           ,ValorPor
                           ,ValorDefinido
                           ,PercentualAjuste
                           ,TabelaPreco
                           ,ValorMinimo
                           ,TipoKit
                           ,DataAtualizacaoRegistro
                           ,DataCriacaoRegistro
                           ,Observacao
                           ,ValorMinimoCalculado)
                SELECT ProdutoCodigoExterno
                      ,ProdutoCodigoExternoPai
                      ,Email
                      ,ProdutoPai
                      ,Kit
                      ,Grupo
                      ,ProdutoCusto 
                      ,ValorDe 
                      ,ValorPor 
                      ,ValorDefinido 
                      ,PercentualAjuste 
                      ,TabelaPreco 
                      ,ValorMinimo 
                      ,TipoKit 
                      ,DataAtualizacaoRegistro 
                      ,DataCriacaoRegistro 
                      ,Observacao 
                      ,ValorMinimoCalculado
                  FROM @TabelaTmp AS TabelaTmp
                 ORDER BY CAST(TabelaTmp.ProdutoCodigoExternoPai AS INT), CAST(TabelaTmp.ProdutoCodigoExterno AS INT)

            ";

            using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("produtoPai", dkProdutoPai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("emailSolicitante", emailSolicitante, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("listaPreco", listaPreco, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                
                return connection.Execute(sql, parametros);
            }
        }

        public async Task<IEnumerable<SolicitacaoTemporaria_Model>> ListaSolicitacoesTemporarias(string email)
        {
            const string sql = @"
                SELECT Id AS Codigo
                      ,Email
                      ,Kit
                      ,ProdutoPai 
                      ,ProdutoCodigoExterno 
                      ,ProdutoCodigoExternoPai 
                      ,TCOM_PROSER.PROS_NOM AS ProdutoNome
                      ,Pai.PROS_NOM AS ProdutoNomePai
                      ,ValorDefinido 
                      ,ValorDe 
                      ,ValorPor
                      ,TabelaPreco 
                      ,Grupo 
                      ,ProdutoCusto 
                      ,TipoKit 
                      ,Observacao 
                      ,ValorMinimo
                      ,ValorMinimoCalculado 
                      ,PercentualAjuste
                      ,Alterado
                  FROM ConnectParts.dbo.Precificacoes_Temporarias (NOLOCK)
                 INNER JOIN ABACOS.dbo.TCOM_PROSER (NOLOCK)
                    ON TCOM_PROSER.PROS_EXT_COD = Precificacoes_Temporarias.ProdutoCodigoExterno
                  LEFT OUTER JOIN ABACOS.dbo.TCOM_PROSER AS Pai (NOLOCK)
				    ON Pai.PROS_EXT_COD = Precificacoes_Temporarias.ProdutoCodigoExternoPai
                 WHERE Email = @email 
                ORDER BY Id;
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("email", email, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return await connection.QueryAsync<SolicitacaoTemporaria_Model>(sql, parametros);
            }
        }

        public async Task<int> CancelarERemoverPorUsuario(string email)
        {
            const string sql = @"
                DELETE 
                  FROM ConnectParts.dbo.Precificacoes_Temporarias
                 WHERE Email = @email";

            using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("email", email, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return await connection.ExecuteAsync(sql, parametros);
            }
        }

        public async Task<SolicitacoesKit_Model> ObterRecente(int listaPreco, string produtoCodigoExterno)
        {
            const string sql = @"SELECT SolicitacoesKit.Codigo
                                       ,SolicitacoesKit.ProdutoCodigoExterno
                                       ,SolicitacoesKit.ProdutoCodigoExternoPai
                                       ,SolicitacoesKit.ProdutoNome
                                       ,SolicitacoesKit.ProdutoNomePai
                                       ,SolicitacoesKit.ProdutoGrupoCodigo
                                       ,SolicitacoesKit.SolicitanteResponsavelEmail
                                       ,SolicitacoesKit.AprovadorResponsavelEmail
                                       ,SolicitacoesKit.SolicitacaoStatusCodigo
                                       ,SolicitacoesKit.Confirmado
                                       ,SolicitacoesKit.ProdutoListaPrecoCodigo
                                       ,SolicitacoesKit.ConfirmacaoEmail
                                       ,SolicitacoesKit.DataAtualizacaoRegistro
                                       ,SolicitacoesKit.DataCriacaoRegistro
                                       ,SolicitacoesKit.Aprovado
                                       ,SolicitacoesKit.Observacao
                                   FROM PrecificacaoCadastro.dbo.SolicitacoesKit
                                  WHERE ProdutoCodigoExterno = @produto
                                    AND ProdutoListaPrecoCodigo = @listaPreco
                                    AND ISNULL(Confirmado, 0) = 0; ";

            using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("listaPreco", listaPreco, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("produto", produtoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return (SolicitacoesKit_Model)connection.Query<SolicitacoesKit_Model>(sql, parametros);
            }
        }

        public async Task<int> AtualizarSolicitacaoTemporaria(SolicitacaoTemporaria_Model solicitacao)
        {
            const string sql = @"
                EXEC CONNECTPARTS.dbo.SP_SalvarPrecificacao
                     @ProdutoCodigoExterno = @ProdutoCodigoExterno
                    ,@listaPreco = @listaPreco
                    ,@Email = @Email
                    ,@ValorDefinido = @ValorDefinido
                    ,@ValorMinimoCalculado = @ValorMinimoCalculado
                    ,@Codigo = @Codigo
            ";

                using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("ProdutoCodigoExterno", solicitacao.ProdutoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("Email", solicitacao.Email, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ValorDefinido", solicitacao.ValorDefinido, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("ValorMinimoCalculado", solicitacao.ValorMinimoCalculado, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("listaPreco", solicitacao.TabelaPreco, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("Codigo", solicitacao.Codigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return await connection.ExecuteAsync(sql, parametros);
            }
        }

        public async Task<int> GerarSolicitacoes(Solicitacao_Model solicitacao)
        {
            const string sql = @"
                INSERT INTO CONNECTPARTS.dbo.Precificacoes(
                    ProdutoCodigoExterno
                   ,ProdutoCodigoExternoPai
                   ,ProdutoNome
                   ,SolicitanteResponsavelEmail
                   ,AprovadorResponsavelEmail
                   ,Observacao
                   ,Aprovado
                   ,DataConfirmacao
                   ,ProdutoListaPrecoCodigo
                   ,ProdutoPrecoTabela
                   ,ProdutoPrecoPromocional
                   ,ProdutoPrecoTabelaNovo
                   ,ProdutoPrecoPromocionalNovo
                   ,DataAtualizacaoRegistro
                   ,DataCriacaoRegistro
                ) VALUES (
                    @ProdutoCodigoExterno
                   ,@ProdutoCodigoExternoPai
                   ,@ProdutoNome
                   ,@SolicitanteResponsavelEmail
                   ,@AprovadorResponsavelEmail
                   ,@Observacao
                   ,NULL
                   ,NULL
                   ,@ProdutoListaPrecoCodigo
                   ,@ProdutoPrecoTabela
                   ,@ProdutoPrecoPromocional
                   ,@ProdutoPrecoTabelaNovo
                   ,@ProdutoPrecoPromocionalNovo
                   ,@DataAtualizacaoRegistro
                   ,GETDATE()
                );
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("SolicitacaoTipoCodigo", solicitacao.SolicitacaoTipoCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoCodigoExterno", solicitacao.ProdutoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoCodigoExternoPai", solicitacao.ProdutoCodigoExternoPai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoNome", solicitacao.ProdutoNome, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoNomePai", solicitacao.ProdutoNomePai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoGrupoCodigo", solicitacao.ProdutoGrupoCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("SolicitanteResponsavelEmail", solicitacao.SolicitanteResponsavelEmail, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("AprovadorResponsavelEmail", solicitacao.AprovadorResponsavelEmail, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("Observacao", solicitacao.Observacao, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("SolicitacaoStatusCodigo", solicitacao.SolicitacaoStatusCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoNomeNovo", solicitacao.ProdutoNomeNovo, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoNomeReduzidoNovo", solicitacao.ProdutoNomeReduzidoNovo, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoDescricaoNova", solicitacao.ProdutoDescricaoNova, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoListaPrecoCodigo", solicitacao.ProdutoListaPrecoCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoPrecoTabela", solicitacao.ProdutoPrecoTabela, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoPrecoPromocional", solicitacao.ProdutoPrecoPromocional, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoPrecoTabelaNovo", solicitacao.ProdutoPrecoTabelaNovo, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("DataAtualizacaoRegistro", DateTime.Now, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoPrecoPromocionalNovo", solicitacao.ProdutoPrecoPromocionalNovo, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoClasse", solicitacao.ProdutoClasse, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoClasseNova", solicitacao.ProdutoClasseNova, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("DataAtualizacaoRegistro", DateTime.Now, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);

                return await connection.ExecuteAsync(sql, parametros);
            }
        }

        public async Task<int> GerarSolicitacoesKits(Solicitacao_Model solicitacao)
        {
            const string sql = @"
                INSERT INTO PrecificacaoCadastro.dbo.SolicitacoesKit (
                    ProdutoCodigoExterno
                   ,ProdutoCodigoExternoPai
                   ,ProdutoNome
                   ,ProdutoNomePai
                   ,ProdutoGrupoCodigo
                   ,SolicitanteResponsavelEmail
                   ,AprovadorResponsavelEmail
                   ,SolicitacaoStatusCodigo
                   ,Confirmado
                   ,ProdutoListaPrecoCodigo
                   ,ConfirmacaoEmail
                   ,DataAtualizacaoRegistro
                   ,DataCriacaoRegistro
                   ,Aprovado
                   ,Observacao
                ) VALUES (
                    @ProdutoCodigoExterno
                   ,@ProdutoCodigoExternoPai
                   ,@ProdutoNome
                   ,@ProdutoNomePai
                   ,@ProdutoGrupoCodigo
                   ,@SolicitanteResponsavelEmail
                   ,NULL
                   ,@SolicitacaoStatusCodigo
                   ,@Confirmado
                   ,@ProdutoListaPrecoCodigo
                   ,NULL
                   ,@DataAtualizacaoRegistro
                   ,GETDATE()
                   ,@Aprovado
                   ,@Observacao
                );
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("ProdutoCodigoExterno", solicitacao.ProdutoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoCodigoExternoPai", solicitacao.ProdutoCodigoExternoPai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoNome", solicitacao.ProdutoNome, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoNomePai", solicitacao.ProdutoNomePai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoGrupoCodigo", solicitacao.ProdutoGrupoCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("SolicitanteResponsavelEmail", solicitacao.SolicitanteResponsavelEmail, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("SolicitacaoStatusCodigo", solicitacao.SolicitacaoStatusCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("Confirmado", 0, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoListaPrecoCodigo", solicitacao.ProdutoListaPrecoCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("DataAtualizacaoRegistro", DateTime.Now, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("Aprovado", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("Observacao", solicitacao.Observacao, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return await connection.ExecuteAsync(sql, parametros);
            }
        }

        public async Task<IEnumerable<ListaPrecos_Model>> ListarListaPrecos()
        {
            const string sql = @"
                SELECT ListaPrecos.Id
                      ,ListaPrecos.Nome
                      ,ListaPrecos.PercentualAjuste
                      ,ListaPrecos.Ativa
                      ,ListaPrecos.Id_Lista_Base
                      ,ListasBases.Id
                      ,ListasBases.Nome
                  FROM ConnectParts.dbo.ListaPrecos
                  LEFT JOIN ConnectParts.dbo.ListaPrecos AS ListasBases
                    ON ListasBases.Id = ListaPrecos.Id_Lista_Base
                 WHERE ListaPrecos.Id NOT IN (2, 21);
            ";

            var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<ListaPrecos_Model>(
                    sql: sql,
                    types: new[] { 
                        typeof(ListaPrecos_Model),
                        typeof(ListaPrecos_Model),
                    },
                    map: obj =>
                    {
                        ListaPrecos_Model listaPreco = obj[0] as ListaPrecos_Model;
                        ListaPrecos_Model listaBase = obj[1] as ListaPrecos_Model;
                        listaPreco.ListaPrecosBase = listaBase;

                        return listaPreco;
                    },
                    splitOn: "Id, Id"
                    );
            }
        }

        public async Task<IEnumerable<ListaPrecos_Model>> ListarLojas()
        {
            const string sql = @"
                SELECT ListaPrecos.Id
                      ,ListaPrecos.Nome
                      ,ListaPrecos.PercentualAjuste
                      ,ListaPrecos.Ativa
                      ,ListaPrecos.Id_Lista_Base
                      ,ListasBases.Id
                      ,ListasBases.Nome
                  FROM ConnectParts.dbo.ListaPrecos
                  LEFT JOIN ConnectParts.dbo.ListaPrecos AS ListasBases
                    ON ListasBases.Id = ListaPrecos.Id_Lista_Base;
            ";

            var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<ListaPrecos_Model>(
                    sql: sql,
                    types: new[] {
                        typeof(ListaPrecos_Model),
                        typeof(ListaPrecos_Model),
                    },
                    map: obj =>
                    {
                        ListaPrecos_Model listaPreco = obj[0] as ListaPrecos_Model;
                        ListaPrecos_Model listaBase = obj[1] as ListaPrecos_Model;
                        listaPreco.ListaPrecosBase = listaBase;

                        return listaPreco;
                    },
                    splitOn: "Id, Id"
                    );
            }
        }

        public async Task<int> AtualizaListaPreco(ListaPrecos_Model lista)
        {
            const string sql = @"
                UPDATE ConnectParts.dbo.ListaPrecos
                SET PercentualAjuste = @PercentualAjuste
                   ,Ativa = @Ativa
                   ,Id_Lista_Base = @ListaBase
                WHERE Id = @Lista;
            ";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("PercentualAjuste", lista.PercentualAjuste, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("Lista", lista.Id, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("Ativa", lista.Ativa, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                parametros.Add("ListaBase", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                if (lista.ListaPrecosBase?.Id > 0)
                {
                    parametros.Add("ListaBase", lista.ListaPrecosBase.Id, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                }                

                return await connection.ExecuteAsync(sql, parametros);
            }
        }

        public async Task<IEnumerable<dynamic>> SolicitacoesListar(SolicitacoesParametrosContarListar_Model solicitacao, int limite, int deslocamento)
        {
            const string sql = @"
                SELECT Solicitacoes.Codigo AS Codigo, 
                       Solicitacoes.SolicitacaoTipoCodigo AS SolicitacaoTipoCodigo, 
                       Solicitacoes.ProdutoCodigoExterno AS ProdutoCodigoExterno, 
                       Solicitacoes.ProdutoCodigoExternoPai AS ProdutoCodigoExternoPai, 
                       Solicitacoes.ProdutoNome AS ProdutoNome, 
                       Solicitacoes.ProdutoNomePai AS ProdutoNomePai, 
                       Solicitacoes.ProdutoGrupoCodigo AS ProdutoGrupoCodigo, 
                       Solicitacoes.SolicitanteResponsavelEmail AS SolicitanteResponsavelEmail, 
                       Solicitacoes.AprovadorResponsavelEmail AS AprovadorResponsavelEmail, 
                       Solicitacoes.Observacao AS Observacao, 
                       Solicitacoes.SolicitacaoStatusCodigo AS SolicitacaoStatusCodigo, 
                       Solicitacoes.ProdutoNomeNovo AS ProdutoNomeNovo, 
                       Solicitacoes.ProdutoNomeReduzidoNovo AS ProdutoNomeReduzidoNovo, 
                       Solicitacoes.ProdutoDescricaoNova AS ProdutoDescricaoNova, 
                       Solicitacoes.ProdutoListaPrecoCodigo AS ProdutoListaPrecoCodigo, 
                       Solicitacoes.ProdutoPrecoTabela AS ProdutoPrecoTabela, 
                       Solicitacoes.ProdutoPrecoPromocional AS ProdutoPrecoPromocional, 
                       Solicitacoes.ProdutoPrecoTabelaNovo AS ProdutoPrecoTabelaNovo, 
                       Solicitacoes.ProdutoPrecoPromocionalNovo AS ProdutoPrecoPromocionalNovo, 
                       Solicitacoes.ProdutoClasse AS ProdutoClasse, 
                       Solicitacoes.ProdutoClasseNova AS ProdutoClasseNova, 
                       Solicitacoes.DataAtualizacaoRegistro AS DataAtualizacaoRegistro, 
                       Solicitacoes.DataCriacaoRegistro AS DataCriacaoRegistro
                  FROM PrecificacaoCadastro.dbo.Solicitacoes
                 WHERE (
                        (Solicitacoes.SolicitacaoTipoCodigo = 3 AND Solicitacoes.ProdutoCodigoExternoPai = Solicitacoes.ProdutoCodigoExterno) OR (Solicitacoes.SolicitacaoTipoCodigo = 5)
                   )
                   AND Solicitacoes.[SolicitacaoStatusCodigo] = @StatusCodigo
                 ORDER BY ROW_NUMBER() OVER (ORDER BY Solicitacoes.[ProdutoCodigoExterno] ASC)
                OFFSET @deslocamento ROWS FETCH NEXT @limite ROWS ONLY 
            ";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("StatusCodigo", solicitacao.SolicitacaoStatusCodigo, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("limite", limite, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("deslocamento", deslocamento, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return await connection.QueryAsync(sql, parametros);
            }
        }

        public async Task<IEnumerable<ListaPrecos_Model>> PercentualAjuste(int listaBase)
        {
            const string sql = @"
                SELECT Id, Nome, PercentualAjuste, Ativa, Id_Lista_Base
                  FROM CONNECTPARTS.dbo.ListaPrecos
                 WHERE Ativa = 1
                   AND (Id_Lista_Base = @lista OR Id = @lista)
            ";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("lista", listaBase, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return await connection.QueryAsync<ListaPrecos_Model>(sql, parametros);
            }
        }

        public async Task<IEnumerable<dynamic>> Precificacoes(
            string Email, 
            string CodigoExternoPai, 
            DateTime? DataInicio, 
            DateTime? DataFim, 
            int? Plataforma,
            int pagina)
        {
            const string sql = @"
                DECLARE @QtdPagina INT = 100;
                SELECT Precificacoes.Id
                      ,ProdutoCodigoExterno
                      ,ProdutoCodigoExternoPai
                      ,ProdutoNome
                      ,SolicitanteResponsavelEmail
                      ,AprovadorResponsavelEmail
                      ,Observacao
                      ,Aprovado
                      ,DataConfirmacao
                      ,ProdutoListaPrecoCodigo
                      ,ProdutoPrecoTabela
                      ,ProdutoPrecoPromocional
                      ,ProdutoPrecoTabelaNovo
                      ,ProdutoPrecoPromocionalNovo
                      ,DataAtualizacaoRegistro
                      ,DataCriacaoRegistro
                      ,COUNT(1) OVER() AS Total
                      ,ListaPrecos.LISP_NOM
                  FROM CONNECTPARTS.dbo.Precificacoes
                 INNER JOIN ABACOS.dbo.TCOM_LISPRE ListaPrecos
                    ON ListaPrecos.LISP_COD = Precificacoes.ProdutoListaPrecoCodigo
                 WHERE Aprovado IS NULL
                   AND DataConfirmacao IS NULL
                   AND (@codigoPai IS NULL OR ProdutoCodigoExternoPai = @CodigoPai)
                   AND (@Email IS NULL OR SolicitanteResponsavelEmail LIKE '%' + @Email + '%')
                   AND (@DataInicio IS NULL OR DataCriacaoRegistro BETWEEN @DataInicio AND DATEADD(SECOND, -1, DATEADD(DAY, 1, @DataFim)))
                   AND (@plataforma IS NULL OR ProdutoListaPrecoCodigo = @plataforma)
                 ORDER BY Precificacoes.Id
                OFFSET ((@NumPagina -1) * @QtdPagina) ROWS
                 FETCH NEXT @QtdPagina ROWS ONLY;
            ";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("Email", Email, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoPai", CodigoExternoPai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("DataInicio", DataInicio, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("DataFim", DataFim, System.Data.DbType.Date, System.Data.ParameterDirection.Input);
                parametros.Add("NumPagina", pagina, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("plataforma", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                if (Plataforma != null)
                {
                    parametros.Add("plataforma", Plataforma, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                }

                return await connection.QueryAsync<dynamic>(sql, parametros);
            }
        }

        public async Task<int> AvaliarPreco(int codigo, bool aprovado, string email)
        {
            const string sql = @"
                UPDATE CONNECTPARTS.dbo.Precificacoes
                   SET Aprovado = @aprovado
                      ,DataConfirmacao = GETDATE()
                      ,AprovadorResponsavelEmail = @Email
                 WHERE Id = @codigo;
            ";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("codigo", codigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("aprovado", aprovado, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                parametros.Add("Email", email, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return await connection.ExecuteAsync(sql, parametros);
            }
        }

        public async Task<IEnumerable<Precificacao_Model>> PrecificacaoPorId(int id)
        {
            const string sql = @"
                SELECT Precificacoes.ProdutoCodigoExterno
                      ,Precificacoes.ProdutoCodigoExternoPai
                      ,Precificacoes.ProdutoNome
                      ,Precificacoes.SolicitanteResponsavelEmail
                      ,Precificacoes.AprovadorResponsavelEmail
                      ,Precificacoes.Observacao
                      ,Precificacoes.Aprovado
                      ,Precificacoes.DataConfirmacao
                      ,Precificacoes.ProdutoListaPrecoCodigo
                      ,Precificacoes.ProdutoPrecoTabela
                      ,Precificacoes.ProdutoPrecoPromocional
                      ,Precificacoes.ProdutoPrecoTabelaNovo
                      ,Precificacoes.ProdutoPrecoPromocionalNovo
                      ,Precificacoes.DataAtualizacaoRegistro
                      ,Precificacoes.DataCriacaoRegistro
                      ,ListaPrecos.LISP_COD AS Id
                      ,ListaPrecos.LISP_NOM AS Nome
                  FROM CONNECTPARTS.dbo.Precificacoes
                 INNER JOIN ABACOS.dbo.TCOM_LISPRE ListaPrecos
                    ON ListaPrecos.LISP_COD = Precificacoes.ProdutoListaPrecoCodigo
                 WHERE Precificacoes.Id = @id;
            ";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("Id", id, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return await connection.QueryAsync<Precificacao_Model>(
                    sql: sql,
                    types: new[]
                    {
                        typeof(Precificacao_Model),
                        typeof(ListaPrecos_Model),
                    },
                    map: obj =>
                    {
                        Precificacao_Model precificacao = obj[0] as Precificacao_Model;
                        ListaPrecos_Model listaPreco = obj[1] as ListaPrecos_Model;

                        precificacao.ListaPreco = listaPreco;

                        return precificacao;
                    },
                    param: parametros,
                    splitOn: "Id, Id"
                );
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioPrecificacoes(
            string Email,
            string CodigoExternoPai,
            string? CodigoExterno,
            DateTime? DataInicio,
            DateTime? DataFim,
            int? Plataforma,
            bool? aprovado,
            bool? pendente,
            int pagina)
        {
            const string sql = @"
                DECLARE @QtdPagina INT = 100;
                SELECT Precificacoes.Id
                      ,ProdutoCodigoExterno
                      ,ProdutoCodigoExternoPai
                      ,ProdutoNome
                      ,SolicitanteResponsavelEmail
                      ,AprovadorResponsavelEmail
                      ,Observacao
                      ,Aprovado
                      ,DataConfirmacao
                      ,ProdutoListaPrecoCodigo
                      ,ProdutoPrecoTabela
                      ,ProdutoPrecoPromocional
                      ,ProdutoPrecoTabelaNovo
                      ,ProdutoPrecoPromocionalNovo
                      ,DataAtualizacaoRegistro
                      ,DataCriacaoRegistro
                      ,COUNT(1) OVER() AS Total
                      ,ListaPrecos.Nome
                  FROM CONNECTPARTS.dbo.Precificacoes
                 INNER JOIN CONNECTPARTS.dbo.ListaPrecos
                    ON ListaPrecos.Id = Precificacoes.ProdutoListaPrecoCodigo
                 WHERE 1 = CASE 
                             WHEN @aprovado IS NULL THEN 1
                             WHEN Aprovado = 1 AND @aprovado = 1 THEN 1
                             WHEN Aprovado = 0 AND @aprovado = 0 THEN 1
                             ELSE 0
                           END
                   AND 1 = CASE 
                           WHEN @pendente IS NULL THEN 1
                           WHEN @pendente = 1 AND Aprovado IS NULL THEN 1 
                           WHEN @pendente = 0 AND Aprovado IS NOT NULL THEN 1
                           END
				   AND (@codigoPai IS NULL OR ProdutoCodigoExternoPai = @CodigoPai)
                   AND (@codigoExterno IS NULL OR ProdutoCodigoExterno = @codigoExterno)
                   AND (@Email IS NULL OR SolicitanteResponsavelEmail LIKE '%' + @Email + '%')
                   AND (@DataInicio IS NULL OR DataCriacaoRegistro BETWEEN @DataInicio AND DATEADD(SECOND, -1, DATEADD(DAY, 1, @DataFim)))
                   AND (@plataforma IS NULL OR ProdutoListaPrecoCodigo = @plataforma)
                 ORDER BY Precificacoes.Id
                OFFSET ((@NumPagina -1) * @QtdPagina) ROWS
                 FETCH NEXT @QtdPagina ROWS ONLY;
            ";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("Email", Email, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoPai", CodigoExternoPai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("codigoExterno", CodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("DataInicio", DataInicio, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("DataFim", DataFim, System.Data.DbType.Date, System.Data.ParameterDirection.Input);
                parametros.Add("NumPagina", pagina, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("aprovado", null, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                if (aprovado != null)
                {
                    parametros.Add("aprovado", aprovado, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                }
                parametros.Add("pendente", null, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                if (pendente != null)
                {
                    parametros.Add("pendente", pendente, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                }
                parametros.Add("plataforma", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                if (Plataforma != null)
                {
                    parametros.Add("plataforma", Plataforma, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                }

                return await connection.QueryAsync<dynamic>(sql, parametros);
            }
        }

    }
}
