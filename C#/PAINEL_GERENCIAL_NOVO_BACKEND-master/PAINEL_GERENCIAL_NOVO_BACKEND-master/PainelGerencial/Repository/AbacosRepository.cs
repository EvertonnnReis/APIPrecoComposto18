using Dapper;
using Domain.Abacos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PainelGerencial.Domain.Abacos;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public class AbacosRepository: IAbacosRepository
    {
        private readonly string _connectionString;

        public AbacosRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PainelGerencialServer");
        }

        public IEnumerable<dynamic> GetProsCod(string produto)
        {
            string sql = @"SELECT PROS_COD FROM ABACOS.dbo.TCOM_PROSER 
                            WHERE PROS_EXT_COD = @produto; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("produto", produto, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public IEnumerable<dynamic> GetProdutosPorPedido(string pedido)
        {
            string sql = @"SELECT DISTINCT TCOM_PROSER.PROS_COD
                             FROM ABACOS.dbo.TCOM_PEDSAI
                            INNER JOIN ABACOS.dbo.TCOM_ITEPDS
                               ON TCOM_ITEPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                            INNER JOIN ABACOS.dbo.TCOM_PROSER
                               ON TCOM_PROSER.PROS_COD = TCOM_ITEPDS.PROS_COD
                            WHERE PEDS_EXT_COD = @pedido ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedido", pedido, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public int CargaProduto(string codigoExterno)
        {
            string sql = @"EXEC ABACOS.dbo.prpl_P_logpro @pPROS_COD = @cod_kpl, @pINTF_COD = 46, @pACAO = 'C'; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("cod_kpl", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> PedidoStatus(string codigoExterno)
        {
            const string sql = @"SELECT TCOM_STAPDS.STAP_COD, TCOM_STAPDS.STAP_NOM
                                   FROM ABACOS.dbo.TCOM_PEDSAI (NOLOCK)
                                  INNER JOIN ABACOS.dbo.TCOM_STAPDS (NOLOCK) ON TCOM_PEDSAI.STAP_COD = TCOM_STAPDS.STAP_COD
                                  WHERE TCOM_PEDSAI.PEDS_EXT_COD = @pedido; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedido", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query(sql, parametros);
            }
        }

        public IEnumerable<TCOM_PEDSAI> PedidoIntegrado(string codigoExterno)
        {
            const string sql = @"SELECT PEDS_COD, PEDS_EXT_COD
                                   FROM ABACOS.dbo.TCOM_PEDSAI
                                  WHERE PEDS_EXT_COD = @pedido
                                     OR PEDS_EXT_COD LIKE @pedido + '-%';";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedido", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query<TCOM_PEDSAI>(sql, parametros);
            }
        }

        public async Task<IEnumerable<TCOM_PEDSAI>> PedidoNotaFiscal(string codigoExterno)
        {
            const string sql = @"SELECT TOP 1
                                        Pedido.PEDS_COD
                                       ,Pedido.PEDS_EXT_COD
                                       ,NotaFiscal.NOTF_COD
                                       ,Nfe.NOTF_COD
                                       ,Nfe.NFEE_TXT_ENVXML
                                   FROM ABACOS.dbo.TCOM_PEDSAI           AS Pedido 
                                   LEFT JOIN ABACOS.dbo.TGEN_NOTFIS      AS NotaFiscal 
                                     ON NotaFiscal.PEDS_COD = Pedido.PEDS_COD 
                                   LEFT JOIN ABACOS.dbo.TGEN_NFEENV      AS Nfe
                                     ON Nfe.NOTF_COD = NotaFiscal.NOTF_COD 
                                    -- AND Nfe.NFEE_CHR_TIP = 'A' 
                                    AND Nfe.NFEE_STA_CONSTA = 100
                                  WHERE Pedido.PEDS_EXT_COD = @pedidoCodigoExterno
                                    AND Pedido.STAP_COD IN (11, 27)
                                  ORDER BY Pedido.UNIN_COD DESC; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedidoCodigoExterno", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return await connection.QueryAsync<TCOM_PEDSAI, TGEN_NOTFIS, TGEN_NFEENV, TCOM_PEDSAI>(
                    sql: sql, 
                    param: parametros,
                    map: (Pedido, NotaFiscal, Nfe) =>
                    {
                        Pedido.TGEN_NOTFIS = NotaFiscal;
                        if (Nfe != null)
                        {
                            NotaFiscal.TGEN_NFEENV = Nfe;
                        }                        
                        return Pedido;
                    },
                    splitOn: "PEDS_COD, NOTF_COD, NOTF_COD");
            }
        }

        public async Task<IEnumerable<dynamic>> ProdutoListaPreco(int listaPreco, string produtoCodigoExterno)
        {
            const string sql = @"SELECT TCOM_PROLIS.PROL_COD
                                       ,TCOM_PROLIS.LISP_COD
                                       ,TCOM_LISPRE.LISP_NOM
                                       ,TCOM_PROLIS.PROS_COD
                                       ,TCOM_PROSER.PROS_EXT_COD
                                       ,TCOM_PROSER.PROS_NOM
                                       ,PROL_VAL_PRE    = TCOM_PROLIS.PROL_VAL_PRE
                                       ,PROL_VAL_PREPRO = TCOM_PROLIS.PROL_VAL_PREPRO
                                       ,TCOM_PROLIS.PROL_DAT_INIPRO
                                       ,TCOM_PROLIS.PROL_DAT_FIMPRO
                                   FROM ABACOS.dbo.TCOM_PROLIS   (NOLOCK) 
                                  INNER JOIN ABACOS.dbo.TCOM_PROSER (NOLOCK) 
                                     ON (TCOM_PROLIS.PROS_COD = TCOM_PROSER.PROS_COD)
                                  INNER JOIN ABACOS.dbo.TCOM_LISPRE (NOLOCK) 
                                     ON (TCOM_PROLIS.LISP_COD = TCOM_LISPRE.LISP_COD)
                                  WHERE (EXISTS (SELECT 1 FROM TCOM_LISUNI X (NOLOCK) 
                                                  INNER JOIN TGEN_UNINEG Y (NOLOCK) 
                                                     ON (X.UNIN_COD = Y.UNIN_COD)
                                                  WHERE (X.LISP_COD=TCOM_PROLIS.LISP_COD)
                                                    AND (Y.EMPR_COD=1)))
                                    AND ((NOT EXISTS (SELECT 1 FROM TCOM_USULIS X (NOLOCK) 
                                                       INNER JOIN TACE_USUSIS Y (NOLOCK) 
                                                          ON (X.USUS_COD = Y.USUS_COD)
                                                       WHERE (LISP_COD=TCOM_PROLIS.LISP_COD) 
                                                         AND (Y.USUS_DAT_FIM IS NULL))) OR
                                         (EXISTS (SELECT 1 FROM TCOM_USULIS (NOLOCK) 
                                 	               WHERE LISP_COD=TCOM_PROLIS.LISP_COD 
                                                     AND USUS_COD=1476)))
                                    AND (TCOM_PROLIS.LISP_COD = @lista) 
                                    AND (TCOM_PROSER.PROS_EXT_COD = @produtoCodigoExterno) 
                                    AND (TCOM_PROSER.PROS_DAT_FIM IS NULL) 
                                  ORDER BY TCOM_PROLIS.PROS_COD; ";
            var connection = new SqlConnection(_connectionString);
            {
                var parametros = new DynamicParameters();
                parametros.Add("lista", listaPreco, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("produtoCodigoExterno", produtoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return await connection.QueryAsync<dynamic>(sql, parametros);
            }
        }

        public bool ExisteProdutoAbacos(string protocoloProduto)
        {
            const string sql = @"SELECT 1 FROM IntegracoesVtex_.dbo.ProdutoAbacos
                                  WHERE ProtocoloProduto = @ProtocoloProduto; ";

            var connection = new SqlConnection(_connectionString);
            {
                var parametros = new DynamicParameters();
                parametros.Add("ProtocoloProduto", protocoloProduto, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                
                return connection.Query<dynamic>(sql, parametros).Any();
            }
        }

        public int InserirProdutoAbacos(ProdutoAbacosViewModel produto)
        {
            const string sql = @"INSERT INTO IntegracoesVtex_.dbo.ProdutoAbacos (
                                      ProtocoloProduto
                                     ,CodigoProduto
                                     ,CodigoProdutoPai
                                     ,NomeProduto
                                     ,CodigoMarca
                                     ,CodigoFabricante
                                     ,Peso
                                     ,Largura
                                     ,Altura
                                     ,UnidadeMedidaNome
                                     ,ProdutoKit
                                     ,CodigoProdutoPaiAbacos
                                 ) VALUES (
                                      @ProtocoloProduto
                                     ,@CodigoProduto
                                     ,@CodigoProdutoPai
                                     ,@NomeProduto
                                     ,@CodigoMarca
                                     ,@CodigoFabricante
                                     ,@Peso
                                     ,@Largura
                                     ,@Altura
                                     ,@UnidadeMedidaNome
                                     ,@ProdutoKit
                                     ,@CodigoProdutoPaiAbacos
                                 ); ";

            var connection = new SqlConnection(_connectionString);
            {
                var parametros = new DynamicParameters();
                parametros.Add("ProtocoloProduto", produto.ProtocoloProduto, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoProduto", produto.CodigoProduto, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoProdutoPai", produto.CodigoProdutoPai, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("NomeProduto", produto.NomeProduto, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoMarca", produto.CodigoMarca, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoFabricante", produto.CodigoFabricante, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("Peso", produto.Peso, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("Largura", produto.Largura, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("Altura", produto.Altura, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("UnidadeMedidaNome", produto.UnidadeMedidaNome, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("ProdutoKit", produto.ProdutoKit, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoProdutoPaiAbacos", produto.CodigoProdutoPaiAbacos, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int InserirCategoriasDoSite(LinhaCategoriasDoSiteViewModel categoria, string codigoProduto)
        {
            const string sql = @"INSERT INTO IntegracoesVtex_.dbo.CategoriasDoSite (
                                      CodigoCategoria
                                     ,CodigoCategoriaPai
                                     ,CodigoExternoCategoria
                                     ,IdProdutoAbacos
                                 ) 
                                 SELECT @CodigoCategoria
                                       ,@CodigoCategoriaPai
                                       ,@CodigoExternoCategoria
                                       ,IdProdutoAbacos
                                   FROM IntegracoesVtex_.dbo.ProdutoAbacos
                                  WHERE CodigoProduto = @CodigoProduto
                                    AND NOT EXISTS(SELECT 1 
                                                     FROM IntegracoesVtex_.dbo.CategoriasDoSite 
                                                    WHERE CodigoCategoria = @CodigoCategoria
                                                      AND CategoriasDoSite.IdProdutoAbacos = ProdutoAbacos.IdProdutoAbacos); ";

            var connection = new SqlConnection(_connectionString);
            {
                var parametros = new DynamicParameters();
                parametros.Add("CodigoCategoria", categoria.CodigoCategoria, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoCategoriaPai", categoria.CodigoCategoriaPai, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoExternoCategoria", categoria.CodigoExternoCategoria, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoProduto", codigoProduto, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                
                return connection.Execute(sql, parametros);
            }
        }

        public int InserirComponentesKit(LinhaComponentesKitViewModel componente, string codigoProduto)
        {
            const string sql = @"INSERT INTO IntegracoesVtex_.dbo.ComponentesKit(
                                     CodigoProdutoComponente
                                    ,Quantidade
                                    ,PrecoPromocional
                                    ,IdProdutoAbacos
                                 )
                                 SELECT @CodigoProdutoComponente
                                       ,@Quantidade
                                       ,@PrecoPromocional
                                       ,IdProdutoAbacos
                                   FROM IntegracoesVtex_.dbo.ProdutoAbacos
                                  WHERE CodigoProduto = @CodigoProduto
                                    AND NOT EXISTS(SELECT 1 FROM IntegracoesVtex_.dbo.ComponentesKit
                                                    WHERE CodigoProdutoComponente = @CodigoProdutoComponente
                                                      AND ComponentesKit.IdProdutoAbacos = ProdutoAbacos.IdProdutoAbacos); ";
            
            var connection = new SqlConnection(_connectionString);
            {
                var parametros = new DynamicParameters();
                parametros.Add("CodigoProdutoComponente", componente.CodigoProdutoComponente, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("Quantidade", componente.Quantidade, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("PrecoPromocional", componente.PrecoPromocional, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoProduto", codigoProduto, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> ListaPreco(int? codigoLista)
        {
            const string sql = @"
                SELECT LISP_COD, LISP_NOM
                  FROM ABACOS.dbo.TCOM_LISPRE (NOLOCK)
                 WHERE LISP_COD = @lista;
            ";

            var connection = new SqlConnection(_connectionString);
            {
                var parametros = new DynamicParameters();
                parametros.Add("lista", codigoLista, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parametros);
            }
        }

        public string ConverterListaPrecos(int? codigoLista)
        {
            var resultado = this.ListaPreco(codigoLista);

            return resultado.FirstOrDefault()?.LISP_NOM.ToString();
        }

        public IEnumerable<dynamic> ListaPrecoListar()
        {
            const string sql = @"
                SELECT LISP_COD, LISP_NOM
                  FROM ABACOS.dbo.TCOM_LISPRE (NOLOCK)
                 WHERE LISP_DAT_FIM >= GETDATE();
            ";

            var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<TCOM_PROLIS>> ListaPrecoPorDKPai(string produtoPai)
        {
            const string sql = @"
                SELECT TCOM_PROLIS.PROL_COD
                      ,TCOM_PROLIS.LISP_COD
                      ,TCOM_PROLIS.PROL_DAT_INIPRO
                      ,TCOM_PROLIS.PROL_DAT_FIMPRO
                      ,TCOM_PROLIS.PROL_VAL_PRE
                      ,TCOM_PROLIS.PROL_VAL_PREPRO
                      ,TCOM_PROLIS.PROL_DAT_CAD
                      ,TCOM_PROLIS.PROL_DAT_ALT
                      ,TCOM_PROLIS.USUS_COD
                      ,TCOM_PROLIS.PROL_PER
                      ,TCOM_PROLIS.PROL_DAT_INIPRO
                      ,TCOM_PROLIS.PROL_DAT_FIMPRO
                      ,TCOM_PROSER.PROS_COD
                      ,TCOM_PROSER.PROS_EXT_COD
                      ,TCOM_PROSER.PROS_NOM
                      ,TCOM_PROSER.PROS_CFB
                      ,TCOM_FAMPRO.FAMP_COD
                      ,TCOM_FAMPRO.FAMP_NOM
                      ,TCOM_SUBPRO.SUBP_COD
                      ,TCOM_SUBPRO.SUBP_NOM
                      ,TCOM_GRUPRO.GRUP_COD
                      ,TCOM_GRUPRO.GRUP_NOM
                      ,TCOM_MARPRO.MARP_COD
                      ,TCOM_MARPRO.MARP_NOM
                      ,TCOM_CLAPRO.CLAP_COD
                      ,TCOM_CLAPRO.CLAP_NOM
                      ,TCOM_LISPRE.LISP_COD
                      ,TCOM_LISPRE.LISP_NOM
                      ,TGEN_UNIMON.UNIM_COD
                      ,TGEN_UNIMON.UNIM_NOM
                      ,TACE_USUSIS.USUS_COD
                      ,TACE_USUSIS.USUS_NOM
                      ,PRCC_VAL_PRETAB = ISNULL(TCOM_PRCCOM.PRCC_VAL_PRETAB, TCOM_PROLIS.PROL_VAL_PRE)
                      ,SALP_COD
                      ,SALP_VAL_CUSBRU
                      ,ROUND(ISNULL(CASE TCOM_PROSER.pros_chr_sn_com
                                    WHEN 'S' THEN ABACOS.dbo.Fest_s_custotkit(TCOM_PROSER.pros_cod, 8,
                                                  'B', 'N')
                                    ELSE TEST_SALPRO.SALP_VAL_CUSBRU
                       END, 0), 2)                       AS CustoBruto
                  FROM ABACOS.dbo.TCOM_PROLIS (NOLOCK) 
                 INNER JOIN ABACOS.dbo.TCOM_PROSER (NOLOCK) ON (TCOM_PROLIS.PROS_COD = TCOM_PROSER.PROS_COD)
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
                 WHERE (EXISTS (SELECT 1 
                                  FROM ABACOS.dbo.TCOM_LISUNI X (NOLOCK) 
                                 INNER JOIN ABACOS.dbo.TGEN_UNINEG Y (NOLOCK) ON (X.UNIN_COD = Y.UNIN_COD)
                                 WHERE (X.LISP_COD=TCOM_PROLIS.LISP_COD)
                                   AND (Y.EMPR_COD=1)))
                   AND ((NOT EXISTS (SELECT 1 
                                       FROM ABACOS.dbo.TCOM_USULIS X (NOLOCK) 
                                      INNER JOIN ABACOS.dbo.TACE_USUSIS Y (NOLOCK) ON (X.USUS_COD = Y.USUS_COD)
                                      WHERE (LISP_COD=TCOM_PROLIS.LISP_COD) AND (Y.USUS_DAT_FIM IS NULL))) OR
                        (EXISTS (SELECT 1 FROM ABACOS.dbo.TCOM_USULIS (NOLOCK) WHERE LISP_COD=TCOM_PROLIS.LISP_COD AND USUS_COD=1476)))
                
                   AND (TCOM_PROLIS.LISP_COD = 2) 
                
                   AND ((SELECT PROS_EXT_COD FROM ABACOS.dbo.TCOM_PROSER AS Produto WHERE PROS_COD=TCOM_PROSER.PROS_COD_PAI)=@produtoPai) 
                
                   AND (TCOM_PROSER.PROS_DAT_FIM IS NULL) 
				   
                 ORDER BY TCOM_PROLIS.PROS_COD;
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("produtoPai", produtoPai, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                var listaPrecos = await connection.QueryAsync<TCOM_PROLIS>(
                    sql: sql,
                    types: new[]
                    {
                        typeof(TCOM_PROLIS),
                        typeof(TCOM_PROSER),
                        typeof(TCOM_FAMPRO),
                        typeof(TCOM_SUBPRO),
                        typeof(TCOM_GRUPRO),
                        typeof(TCOM_MARPRO),
                        typeof(TCOM_CLAPRO),
                        typeof(TCOM_LISPRE),
                        typeof(TGEN_UNIMON),
                        typeof(TACE_USUSIS),
                        typeof(TEST_SALPRO),
                    },
                    map: obj =>
                    {
                        TCOM_PROLIS produtoLista = obj[0] as TCOM_PROLIS;
                        TCOM_PROSER produto = obj[1] as TCOM_PROSER;
                        TCOM_FAMPRO familia = obj[2] as TCOM_FAMPRO;
                        TCOM_SUBPRO subgrupo = obj[3] as TCOM_SUBPRO;
                        TCOM_GRUPRO grupo = obj[4] as TCOM_GRUPRO;
                        TCOM_MARPRO marca = obj[5] as TCOM_MARPRO;
                        TCOM_CLAPRO classe = obj[6] as TCOM_CLAPRO;
                        TCOM_LISPRE listaPreco = obj[7] as TCOM_LISPRE;
                        TGEN_UNIMON unidadeMonetaria = obj[8] as TGEN_UNIMON;
                        TACE_USUSIS usuario = obj[9] as TACE_USUSIS;
                        TEST_SALPRO saldoProduto = obj[10] as TEST_SALPRO;

                        produtoLista.PROS_COD = produto.PROS_COD;
                        produtoLista.TCOM_PROSER = produto;
                        if (produtoLista.TCOM_PROSER != null)
                        {
                            produtoLista.TCOM_PROSER.FAMP_COD = familia.FAMP_COD;
                            produtoLista.TCOM_PROSER.TCOM_FAMPRO = familia;

                            produtoLista.TCOM_PROSER.SUBP_COD = subgrupo.SUBP_COD;
                            produtoLista.TCOM_PROSER.TCOM_SUBPRO = subgrupo;

                            if (produtoLista.TCOM_PROSER.TCOM_SUBPRO != null)
                            {
                                produtoLista.TCOM_PROSER.TCOM_SUBPRO.GRUP_COD = grupo.GRUP_COD;
                                produtoLista.TCOM_PROSER.TCOM_SUBPRO.TCOM_GRUPRO = grupo;
                            }

                            produtoLista.TCOM_PROSER.MARP_COD = marca.MARP_COD;
                            produtoLista.TCOM_PROSER.TCOM_MARPRO = marca;

                            produtoLista.TCOM_PROSER.CLAP_COD = classe.CLAP_COD;
                            produtoLista.TCOM_PROSER.TCOM_CLAPRO = classe;
                            if (saldoProduto != null)
                            {
                                if (produtoLista.TCOM_PROSER.TEST_SALPRO == null)
                                {
                                    produtoLista.TCOM_PROSER.TEST_SALPRO = new List<TEST_SALPRO>();
                                }                                
                                produtoLista.TCOM_PROSER.TEST_SALPRO.Add(saldoProduto);
                            }
                            produtoLista.TACE_USUSIS = usuario;
                        }
                        listaPreco.TGEN_UNIMON = unidadeMonetaria;
                        produtoLista.TCOM_LISPRE = listaPreco;

                        return produtoLista;
                    },
                    param: parametros,
                    splitOn: "PROL_COD, PROS_COD, FAMP_COD, SUBP_COD, GRUP_COD, MARP_COD, CLAP_COD, LISP_COD, UNIM_COD, USUS_COD, SALP_COD"
                );

                foreach (TCOM_PROLIS preco in listaPrecos)
                {
                    preco.TCOM_PROSER
                        .listaComponentes = (IList<TCOM_COMPRO>)await ComponentesKit(preco.TCOM_PROSER.PROS_EXT_COD);
                }

                return listaPrecos;
            }
        }

        public async Task<IEnumerable<TCOM_COMPRO>> ComponentesKit(string codigoExternoKit)
        {
            const string sql = @"
                SELECT TCOM_COMPRO.COMP_COD
                      ,TCOM_COMPRO.PROS_COD
                      ,TCOM_COMPRO.COMP_COD_PRO
                      ,TCOM_COMPRO.COMP_QTF
                      ,TCOM_PROSER.PROS_COD
                      ,TCOM_PROSER.PROS_EXT_COD
                      ,TCOM_PROSER.PROS_NOM
                      ,TCOM_PROSER.PROS_CFB
                      ,ProdutoComponente.PROS_COD
                      ,ProdutoComponente.PROS_EXT_COD
                      ,ProdutoComponente.PROS_NOM
                      ,ProdutoComponente.PROS_CFB
                      ,PrecoComponente.PROL_COD
                      ,PrecoComponente.PROL_VAL_PREPRO
                  FROM ABACOS.DBO.TCOM_COMPRO WITH (NOLOCK) 
                 INNER JOIN ABACOS.dbo.TCOM_PROSER WITH (NOLOCK)
                    ON TCOM_PROSER.PROS_COD = TCOM_COMPRO.PROS_COD
                  LEFT JOIN ABACOS.DBO.TCOM_PROSER AS ProdutoComponente WITH (NOLOCK) 
                    ON ProdutoComponente.PROS_COD = TCOM_COMPRO.COMP_COD_PRO
                  LEFT JOIN ABACOS.dbo.TCOM_PROLIS AS PrecoComponente WITH (NOLOCK) 
                    ON ProdutoComponente.PROS_COD = PrecoComponente.PROS_COD 
                   AND PrecoComponente.LISP_COD = 2
                 WHERE TCOM_PROSER.PROS_EXT_COD = @codigoExternoKit;
            ";

            using var connetion = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("codigoExternoKit", codigoExternoKit, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return await connetion.QueryAsync<TCOM_COMPRO>(
                    sql: sql,
                    types: new[]
                    {
                        typeof(TCOM_COMPRO),
                        typeof(TCOM_PROSER),
                        typeof(TCOM_PROSER),
                        typeof(TCOM_PROLIS),
                    },
                    map: obj =>
                    {
                        TCOM_COMPRO componente = obj[0] as TCOM_COMPRO;
                        TCOM_PROSER produto = obj[1] as TCOM_PROSER;
                        TCOM_PROSER produtoComponente = obj[2] as TCOM_PROSER;
                        TCOM_PROLIS listaPreco = obj[3] as TCOM_PROLIS;

                        componente.TCOM_PROSER = produto;
                        componente.ProdutoComponente = produtoComponente;
                        componente.ProdutoComponente.TCOM_PROLIS = new List<TCOM_PROLIS>();
                        componente.ProdutoComponente.TCOM_PROLIS.Add(listaPreco);

                        return componente;
                    },
                    param: parametros,
                    splitOn: "COMP_COD, PROS_COD, PROS_COD, PROL_COD"
                );
            }
        }

        public async Task<IEnumerable<dynamic>> BuscaRelatorioEstoqueMinimo(int filtro)
        {
            const string sql = @"
                SELECT
 	                 PR.PROS_EXT_COD AS DK
	                ,REPLACE(ISNULL(RTRIM(CONVERT(VARCHAR(250), PR.PROS_DET)), RTRIM(PR.PROS_NOM)),CHAR(13) + Char(10) ,' ') AS DESCRICAO
	                ,ISNULL(S1.SALP_QTF_DIS, 0) AS DISP_CONNECT
	                ,ISNULL(S2.SALP_QTF_DIS, 0) AS VIRTUAL_00K
                    ,COUNT(1) OVER() AS TOTAL
                FROM	
	                ABACOS.DBO.TCOM_PROSER AS PR WITH (NOLOCK)
	                LEFT JOIN ABACOS.DBO.TEST_SALPRO AS S1 WITH (NOLOCK) ON S1.PROS_COD = PR.PROS_COD AND S1.ALMO_COD = 8
	                LEFT JOIN ABACOS.DBO.TEST_SALPRO AS S2 WITH (NOLOCK) ON S2.PROS_COD = PR.PROS_COD AND S2.ALMO_COD = 21
	
                WHERE (@condicao = 0 and (S1.SALP_QTF_DIS < 1)) 
                or (@condicao = 1 and (S1.SALP_QTF_DIS = 1))
                or (@condicao = 2 and (S1.SALP_QTF_DIS > 1 AND S1.SALP_QTF_DIS <= 5))
            ";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("condicao", filtro, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return await connection.QueryAsync<dynamic>(sql, parametros);
            }
        }

        public async Task<IEnumerable<dynamic>> BuscaRelatorioProdutosMaisVendidos(int top, string dataInicial, string dataFinal)
        {
            string sql = @"
                SELECT TOP " + top.ToString() + @" TCOM_PROSER.PROS_COD, TCOM_PROSER.PROS_NOM, count(TCOM_PEDSAI.PEDS_EXT_COD) as total_pedidos 
                FROM TCOM_PEDSAI
                INNER JOIN ABACOS.dbo.TCOM_ITEPDS
                   ON TCOM_ITEPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                   INNER JOIN ABACOS.dbo.TCOM_PROSER
                   ON TCOM_PROSER.PROS_COD = TCOM_ITEPDS.PROS_COD
                WHERE TCOM_PEDSAI.PEDS_DAT_MOV BETWEEN @dataInicial AND @dataFinal
                group by TCOM_PROSER.PROS_COD, TCOM_PROSER.PROS_NOM
                order by total_pedidos desc
            ";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("dataInicial", dataInicial, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("dataFinal", dataFinal, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return await connection.QueryAsync<dynamic>(sql, parametros);
            }
        }


    }
}
