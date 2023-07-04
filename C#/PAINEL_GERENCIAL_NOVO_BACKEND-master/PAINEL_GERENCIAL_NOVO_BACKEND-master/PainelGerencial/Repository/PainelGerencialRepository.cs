﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using PainelGerencial.Domain;

namespace PainelGerencial.Repository
{
    public class PainelGerencialRepository : IPainelGerencialRepository
    {
        private readonly string _connectionString;
        private readonly string _connectionStringMysql;
        private readonly string _connectionStringMysqlPainelGerencial;
        private readonly string _connectionStringMysqlNucci;
        private readonly string _connectionStringSigeco;
        private readonly int[] PEDIDO_NAO_MOSTRAR = { 199438, 653056, 691747, 706006, 709958, 719060, 719526, 753018, 758062, 765687, 768728, 792048, 827608, 851971, 853820, 863047, 863245, 870359, 872091, 882575, 883490, 890822, 904053, 906489, 912029, 965254, 967673, 984654, 985568, 987737, 1147074, 1148688, 1179683, 1194062, 1219866, 1225756, 1227122, 1229241, 1231471, 1238243, 1238246, 1238247, 1258837, 1265217, 1311947, 1320375, 1343937, 1345645, 1396785, 1414236, 1437858, 1463799, 1464412, 1473721, 1478048, 1501334, 1604272, 1609596, 1737464, 1759443, 1778071, 1801806, 1825300, 1829136, 1829145, 1850571, 1866655, 1873481, 1922549, 1925433, 1931633, 1931808, 1936334, 1947779, 1951444, 1951759, 1957849, 1967218, 1969426, 1972865, 1973434, 1973502, 1973752, 1975622, 1976244, 1976245, 1977642, 1978545, 1978844, 1987448, 1989250, 1991830, 1993310, 1994570, 1996100, 2000701, 2000972, 2003305, 2004089, 2005476, 2006310, 2008064, 2009138, 2009582, 2009589, 2012228, 2013115, 2014064, 2015082, 2019094, 2019610, 2021362, 2022583, 2030394, 2031271, 2033539, 2035491, 2043326, 2047376, 2048390, 2058046, 2058968, 2059864, 2062184, 2062680, 2063545, 2068483, 2077628, 2078493, 2079783, 2082520, 2082567, 2082646, 2083859, 2084652, 2084653, 2085242, 2085708, 2085761, 2085948, 2086187, 2086230, 2086630, 2086640, 2086732, 2086764, 2086848, 2087258, 2090839, 2090877, 2099200, 2100657, 2102109, 2108495, 2121719, 2122504, 2126612, 2151448, 2162560, 2212273, 2216919, 2289178, 2329688, 2419923, 2441019, 2480349, 2468235, 2535829, 2572359, 2599850, 2607882, 2644570, 2643631, 2575014, 2616556, 2631648, 2619737, 2641058, 2644275, 2663757, 2699484, 2721802, 2735635, 2741687, 2753870, 2754797, 2759249, 2785701, 2804128, 2805228, 2806444, 2806497, 2807067, 2807713, 2808505, 2811055, 2811684, 2827537, 2829649, 2829961, 2868080, 2880660, 2884620, 2902912, 2916340, 2930976, 2942711, 2959904, 2980634, 2981481, 2981589, 3043446, 3089343, 3099430, 3159516, 3211229, 3204922, 3337372 };
        private readonly string SALT_SENHA = "b:i7/pvwFYUn(U?%{E4u?)-o<m%b6sc/2BIWg4s:&|^VEe`Bm[hw]ynlT+bJk(D&";

        public PainelGerencialRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PainelGerencialServer");
            _connectionStringMysql = configuration.GetConnectionString("mysql");
            _connectionStringMysqlPainelGerencial = configuration.GetConnectionString("painelgerencial");
            _connectionStringSigeco = configuration.GetConnectionString("Sigeco");
            _connectionStringMysqlNucci = configuration.GetConnectionString("Nucci");
        }

        public IEnumerable<dynamic> PedidoMaisAntigoPreAnalise()
        {
            string sql = @"SELECT TOP 1
                                  TCOM_PEDSAI.PEDS_COD,
                                  CONVERT(VARCHAR(10), TCOM_PEDSAI.PEDS_DAT_CAD, 103) AS data_antiga
                           FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
                           INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                           INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                           WHERE TCOM_PEDSAI.STAP_COD = 22
                           	 AND TCOM_PEDSAI.COME_COD != 130
                           	 AND TCOM_COMERC.COMG_COD = 1
                             AND TCOM_PEDSAI.ORIP_COD IN (100001, 100005, 100006)
                           ORDER BY TCOM_PEDSAI.PEDS_DAT_CAD ASC; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection
                    .Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> PreAnalise()
        {
            string sql = @"SELECT COUNT(*)  AS quantidade,
								  SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) AS valor_total,
								  SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) / COUNT(1) AS ticket_medio,
                                  (SELECT TOP 1
                                          --TCOM_PEDSAI.PEDS_COD,
                                          CONVERT(VARCHAR(10), TCOM_PEDSAI.PEDS_DAT_CAD, 103) AS data
                                   FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
                                   INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                                   INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                                   WHERE TCOM_PEDSAI.STAP_COD = 22
                                   	 AND TCOM_PEDSAI.COME_COD != 130
                                   	 AND TCOM_COMERC.COMG_COD = 1
                                   ORDER BY TCOM_PEDSAI.PEDS_DAT_CAD ASC) AS pedido_mais_antigo
						   FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
						   INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
						   INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
						   WHERE TCOM_PEDSAI.STAP_COD = 22
						     AND TCOM_COMERC.COMG_COD = 1
							 AND TCOM_PEDSAI.COME_COD NOT IN(79, 130, 160)
							 AND TCOM_PEDSAI.PEDS_COD NOT IN (SELECT codigo FROM TMP_PED_NAOMOSTRAR) ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection
                    .Query<dynamic>(sql);

                
            }
        }

        public IEnumerable<dynamic> AFaturar()
        {
            string sql = @"SELECT COUNT(*)  AS quantidade,
                           	      SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) AS valor_total,
                           	      SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) / COUNT(1) AS ticket_medio
                           FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
                           INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                           INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                           WHERE TCOM_PEDSAI.STAP_COD = 13
                           	 AND TCOM_COMERC.COMG_COD = 1
                           	 AND TCOM_PEDSAI.COME_COD NOT IN(79)
                           	 AND TCOM_PEDSAI.PEDS_COD NOT IN(SELECT codigo FROM TMP_PED_NAOMOSTRAR); ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection
                    .Query<dynamic>(sql);

                
            }
        }

        public IEnumerable<dynamic> PedidoMaisAntigoFaturamentoAFaturar()
        {
            string sql = @"SELECT TOP 1
                                  TCOM_PEDSAI.PEDS_COD,
                                  CONVERT(VARCHAR(10), TCOM_PEDSAI.PEDS_DAT_CAD, 103) AS data
                           FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
                           INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                           INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                           WHERE TCOM_PEDSAI.STAP_COD = 13
                             AND TCOM_COMERC.COMG_COD = 1
                           ORDER BY TCOM_PEDSAI.PEDS_DAT_CAD ASC; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection
                    .Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> Faturados()
        {
            string sql = @"SELECT COUNT(1)  AS quantidade,
                           	      COALESCE(SUM(NOTF_VAL_NOT), 0) AS valor_total,
                           	      COALESCE(SUM(NOTF_VAL_NOT)/COUNT(1), 0) AS ticket_medio,
                                  MIN(TGEN_NOTFIS.NOTF_DAT_CAD) AS pedido_mais_antigo
                           FROM TGEN_NOTFIS
                                   JOIN (
                                           SELECT
                                                   TGEN_ITENOT.NOTF_COD,
                                                   SUM(TGEN_ITENOT.ITEN_QTF_NFF * TGEN_ITENOT.ITEN_VAL_CUSESTLIQ) as TOT
                                           FROM
                                                   TGEN_ITENOT WITH(NOLOCK)
                                               GROUP BY NOTF_COD
                                           )
                           TAB ON TAB.NOTF_COD = TGEN_NOTFIS.NOTF_COD
                           JOIN TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TGEN_NOTFIS.COME_COD
                           WHERE
                               TGEN_NOTFIS.NOTF_DAT_CAN IS NULL AND
                               TCOM_COMERC.COMG_COD = 1 AND
                               TCOM_COMERC.COME_COD <> 152 AND
                               CAST(TGEN_NOTFIS.NOTF_DAT_EMI AS DATE) = CAST(GETDATE() AS DATE); ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection
                    .Query<dynamic>(sql);                
            }
        }

        public IEnumerable<dynamic> PedidoMaisAntigoFaturamentoPendentesVendas()
        {
            string sql = @"SELECT	TOP 1
									TCOM_PEDSAI.PEDS_COD,
									CONVERT(VARCHAR(10), TCOM_PEDSAI.PEDS_DAT_CAD, 103) AS data
								FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
								INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
								INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
							WHERE
								TCOM_PEDSAI.STAP_COD IN (8, 9)
								AND TCOM_COMERC.COMG_COD = 1
							ORDER BY TCOM_PEDSAI.PEDS_DAT_CAD ASC; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection
                    .Query<dynamic>(sql);

                
            }
        }

        public IEnumerable<dynamic> PendentesVendas()
        {
            string sql = @"SELECT
	                            Pedidos.PEDS_EXT_COD, 
	                            ItemPedido.PEDS_COD, 
	                            ItemPedido.ITEP_VAL_PREBRU AS valor_produtos,
	                            ISNULL((SELECT
				                            ISNULL(Comercial.CPLP_VAL_TOT, 0)
			                            FROM 
				                            ABACOS.dbo.TCOM_PEDSAI AS Pedido WITH(NOLOCK)
				                            INNER JOIN ABACOS.dbo.TCOM_CPLPDS AS Comercial WITH(NOLOCK) ON Comercial.PEDS_COD = Pedido.PEDS_COD
				                            INNER JOIN ABACOS.dbo.TCOM_COMERC AS Comercializacao WITH(NOLOCK) ON Comercializacao.COME_COD = Pedido.COME_COD
			                            WHERE
				                            Pedido.STAP_COD NOT IN (11, 12)
				                            AND Pedido.STAP_COD IN (8, 9)
				                            AND Comercializacao.COMG_COD = 1
				                            AND Pedido.UNIN_COD = 5
				                            AND Pedido.PEDS_EXT_COD = Pedidos.PEDS_EXT_COD
				                            AND Pedido.PEDS_DAT_CAN IS NULL)
				                            ,Comercial.CPLP_VAL_TOT) AS valor_vendas,
	                            Pedidos.PEDS_DAT_CAD,
	                            CASE
		                            WHEN ClasseProduto.CLAP_NOM LIKE '%DROP%' THEN 'DROP'
		                            WHEN ClasseProduto.CLAP_NOM LIKE '%CROSS%' THEN 'CROSS'
		                            ELSE 'NORMAL'
	                            END AS dropshipping,
	                            ClasseProduto.CLAP_NOM,
	                            ItemPedido.ITEP_COD,
	                            Produto.PROS_EXT_COD,
	                            Comercial.CPLP_VAL_ENCFIX,
	                            MarcaProduto.MARP_NOM,
                                COUNT(1) OVER() AS Total

                            FROM ABACOS.dbo.TCOM_PEDSAI AS Pedidos WITH(NOLOCK)
                                 INNER JOIN ABACOS.dbo.TCOM_ITEPDS AS ItemPedido WITH(NOLOCK) ON ItemPedido.PEDS_COD = Pedidos.PEDS_COD
	                             INNER JOIN ABACOS.dbo.TCOM_COMERC AS Comercializacao WITH(NOLOCK) ON Comercializacao.COME_COD = Pedidos.COME_COD
	                             INNER JOIN ABACOS.dbo.TCOM_CPLPDS AS Comercial WITH(NOLOCK) ON Comercial.PEDS_COD = Pedidos.PEDS_COD
	                             INNER JOIN ABACOS.dbo.TCOM_PROSER AS Produto WITH(NOLOCK) ON Produto.PROS_COD = ItemPedido.PROS_COD
	                             INNER JOIN ABACOS.dbo.TCOM_CLAPRO AS ClasseProduto WITH(NOLOCK) ON ClasseProduto.CLAP_COD = Produto.CLAP_COD
	                             LEFT JOIN ABACOS.dbo.TCOM_MARPRO AS MarcaProduto WITH(NOLOCK) ON Produto.MARP_COD = MarcaProduto.MARP_COD

                            WHERE 
	                            Pedidos.UNIN_COD = 1
	                            AND Pedidos.STAP_COD IN (8, 9)
	                            AND Pedidos.STAP_COD NOT IN (11, 12)
	                            AND Pedidos.PEDS_DAT_CAN IS NULL
	                            AND Pedidos.PEDS_DAT_CAD > '2022-02-05'
	                            AND ItemPedido.ITEP_QTF_PEN > 0
	
                            ORDER BY 
	                            Pedidos.PEDS_DAT_CAD";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection
                    .Query<dynamic>(sql);

                
            }
        }

        

        public IEnumerable<dynamic> FaturamentoHoje()
        {
            string sql = @"SELECT COUNT(1) AS quantidade, 
                                  SUM(NOTF_VAL_MER) AS val_mercadorias,
                                  SUM(NOTF_VAL_FRE) AS valor_frete,
                                  SUM(NOTF_VAL_ENC) AS valor_encargos, 
                                  SUM(NOTF_VAL_NOT) AS valor_total,
                                  SUM(TAB.TOT) AS custo_liquido_mercadorias,
                                  SUM(NOTF_VAL_ICM)  AS valor_icms, (0) AS custo_frete,
                                  (SUM(NOTF_VAL_NOT) - SUM(NOTF_VAL_ICM) - SUM(TAB.TOT) - 0) AS valor_margem,
                                  100 * ((SUM(NOTF_VAL_NOT) - SUM(NOTF_VAL_ICM) - SUM(TAB.TOT)) /
                                         SUM(NOTF_VAL_NOT)) AS percentual_margem,
                                  SUM(NOTF_VAL_NOT) / COUNT(1) AS ticket_medio,
                                  MIN(TGEN_NOTFIS.NOTF_DAT_CAD) AS pedido_mais_antigo
                           FROM ABACOS.dbo.TGEN_NOTFIS (NOLOCK)
                           CROSS APPLY
                           (
                           	SELECT
                           	TGEN_ITENOT.NOTF_COD,
                           	SUM(TGEN_ITENOT.ITEN_QTF_NFF *
                           		TGEN_ITENOT.ITEN_VAL_CUSESTLIQ) as TOT
                           		FROM
                           		ABACOS.dbo.TGEN_ITENOT (NOLOCK)
                           		INNER JOIN ABACOS.dbo.TCOM_COMERC (NOLOCK) ON TCOM_COMERC.COME_COD = TGEN_NOTFIS.COME_COD
                           		WHERE
                           		TGEN_ITENOT.NOTF_COD = TGEN_NOTFIS.NOTF_COD
                           		AND
                           		TCOM_COMERC.COMG_COD = 1
                           		GROUP BY TGEN_ITENOT.NOTF_COD
                           	) TAB
                           	INNER JOIN ABACOS.dbo.TCOM_COMERC (NOLOCK) ON TCOM_COMERC.COME_COD = TGEN_NOTFIS.COME_COD
                           	WHERE TGEN_NOTFIS.NOTF_DAT_EMI BETWEEN convert(datetime2, @data+' 00:00:00') AND convert(datetime2, @data+' 23:59:59')
                           	AND TGEN_NOTFIS.NOTF_DAT_CAN IS NULL
                           	AND TCOM_COMERC.COMG_COD = 1
                            AND TCOM_COMERC.COME_COD <> 152; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DateTime data = DateTime.UtcNow;

                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@data", data.ToString("yyyy-MM-dd"), DbType.String, ParameterDirection.Input);

                return connection.Query<dynamic>(sql,
                                                           parameter,
                                                           commandType: CommandType.Text);

                
            }
        }

        public IEnumerable<dynamic> FaturamentoOntem()
        {
            string sql = @"SELECT
	                            COUNT(1)  AS quantidade,
	                            SUM(NOTF_VAL_NOT) AS valor_total,
	                            SUM(NOTF_VAL_NOT) / COUNT(1) AS ticket_medio
                           FROM ABACOS.dbo.TGEN_NOTFIS WITH(NOLOCK)
                           INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TGEN_NOTFIS.COME_COD
                           WHERE
	                           TGEN_NOTFIS.NOTF_DAT_EMI BETWEEN convert(datetime2, @data+' 00:00:00') AND convert(datetime2, @data+' 23:59:59')
	                           AND TGEN_NOTFIS.NOTF_DAT_CAN IS NULL
	                           AND TCOM_COMERC.COMG_COD = 1
                               AND TCOM_COMERC.COME_COD <> 152; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DateTime dataAtual = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day).AddDays(-1);
                
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@data", dataAtual, DbType.DateTime, ParameterDirection.Input);

                return connection
                    .Query<dynamic>(sql,
                                    parameter,
                                    commandType: CommandType.Text);

                
            }
        }

        public IEnumerable<dynamic> FaturamentoUltimos7Dias()
        {
            string sql = @"SELECT COUNT(DISTINCT FA.NOTF_COD) quantidade
                           , COALESCE(SUM(FA.ITEN_VAL_BRUTOT), 0) val_mercadorias
                           , COALESCE(SUM(FA.IMPN_VAL_FRE), 0) valor_frete
                           , COALESCE(SUM(IMPN_VAL_ENC), 0) AS valor_encargos
                           , COALESCE(SUM(IMPN_VAL_NOT), 0) AS valor_total
                           , COALESCE(SUM(ITEN_VAL_CUSESTLIQ * ITEN_QTF_FIS), 0) AS custo_liquido_mercadorias
                           , COALESCE(SUM(IMPN_VAL_ICM), 0) AS valor_icms
                           , (0) AS custo_frete
                           , COALESCE((SUM(IMPN_VAL_NOT) - SUM(IMPN_VAL_ICM) - SUM(ITEN_VAL_CUSESTLIQ * ITEN_QTF_FIS) - 0), 0) AS valor_margem
                           , COALESCE(100 * ((SUM(IMPN_VAL_NOT) - SUM(IMPN_VAL_ICM) - SUM(ITEN_VAL_CUSESTLIQ * ITEN_QTF_FIS)) / SUM(IMPN_VAL_NOT)), 0) AS percentual_margem
                           , COALESCE(SUM(IMPN_VAL_NOT) / COUNT(DISTINCT FA.NOTF_COD), 0) AS ticket_medio
                           FROM DATAWAREHOUSE.dbo.DWFAT FA (NOLOCK)
                           INNER JOIN DATAWAREHOUSE.dbo.DWCOMERC CO (NOLOCK) ON CO.COME_COD = FA.COME_COD
                           WHERE FA.NOTF_DAT_EMI BETWEEN convert(datetime2, @data_inicial) AND convert(datetime2, @data_final)
                           AND CO.COMG_COD = 1
                           AND CO.COME_COD <> 152; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DateTime data_inicial = new DateTime(DateTime.UtcNow.Year, 
                                                     DateTime.UtcNow.Month, 
                                                     DateTime.UtcNow.Day).AddDays(-6);
                DateTime data_final = new DateTime(DateTime.UtcNow.Year, 
                                                   DateTime.UtcNow.Month, 
                                                   DateTime.UtcNow.Day);

                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@data_inicial", data_inicial.ToString("yyyy-MM-dd"), DbType.String, ParameterDirection.Input);
                parameter.Add("@data_final", data_final.ToString("yyyy-MM-dd"), DbType.String, ParameterDirection.Input);

                return connection.Query<dynamic>(sql,
                                                           parameter,
                                                           commandType: CommandType.Text);

                
            }
        }

        public IEnumerable<dynamic> FaturamentoMes()
        {
            string sql = @"SELECT CAST(COUNT(DISTINCT FA.NOTF_COD) AS VARCHAR(20)) quantidade
                                , COALESCE(SUM(FA.ITEN_VAL_BRUTOT), 0) val_mercadorias
                                , COALESCE(SUM(FA.IMPN_VAL_FRE), 0) valor_frete
                                , COALESCE(SUM(IMPN_VAL_ENC), 0) AS valor_encargos
                                , COALESCE(SUM(IMPN_VAL_NOT), 0) AS valor_total
                                , COALESCE(SUM(ITEN_VAL_CUSESTLIQ * ITEN_QTF_FIS), 0) AS custo_liquido_mercadorias
                                , COALESCE(SUM(IMPN_VAL_ICM), 0) AS valor_icms, (0) AS custo_frete
                                , COALESCE((SUM(IMPN_VAL_NOT) - SUM(IMPN_VAL_ICM) - SUM(ITEN_VAL_CUSESTLIQ * ITEN_QTF_FIS) - 0), 0) AS valor_margem
                                , COALESCE(100 * ((SUM(IMPN_VAL_NOT) - SUM(IMPN_VAL_ICM) - SUM(ITEN_VAL_CUSESTLIQ * ITEN_QTF_FIS)) / SUM(IMPN_VAL_NOT)), 0) AS percentual_margem
                                , COALESCE(SUM(IMPN_VAL_NOT) / COUNT(DISTINCT FA.NOTF_COD), 0) AS ticket_medio
                           FROM DATAWAREHOUSE.dbo.DWFAT FA (NOLOCK)
                           INNER JOIN DATAWAREHOUSE.dbo.DWCOMERC CO (NOLOCK) ON CO.COME_COD = FA.COME_COD
                           WHERE FA.NOTF_DAT_EMI BETWEEN convert(datetime2, @data_inicial) AND convert(datetime2,@data_final)
                           AND CO.COMG_COD = 1
                           AND CO.COME_COD <> 152; ";

            using var connection = new SqlConnection(_connectionString);
            {
                string data_inicial = DateTime.UtcNow.ToString("yyyy-MM-01");
                string data_final = DateTime.UtcNow.ToString("yyyy-MM-dd");

                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@data_inicial", data_inicial, DbType.String, ParameterDirection.Input);
                parameter.Add("@data_final", data_final, DbType.String, ParameterDirection.Input);

                return connection.Query<dynamic>(sql,parameter,commandType: CommandType.Text);

                
            }
        }

        public IEnumerable<dynamic> PedidosAProcessarPelaAPI()
        {
            string sql = @"SELECT 
                                COUNT(1) quantidade
                               , CONVERT(VARCHAR(12), MIN(DataLog), 105) DtMaisAntiga
                           FROM [ABACOS].[rpl].[Pedido] (NOLOCK)
                           WHERE DataConfirmacao IS NULL AND CodigoInterface = 34
                           GROUP BY Acao, CodigoInterface ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);

         
            }
        }

        public IEnumerable<dynamic> PedidosACorrigirEstoque()
        {
            string sql = @"SELECT
                               COUNT(*) AS quantidade,
                               SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) AS valor_total,
                               SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) / COUNT(1) AS ticket_medio
                           FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
                           INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                           INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                           WHERE
                               TCOM_PEDSAI.STAP_COD = 9
                               AND TCOM_PEDSAI.UNIN_COD = 5; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> PedidoMaisAntigoACorrigirEstoque()
        {
            string sql = @"SELECT	TOP 1
                               TCOM_PEDSAI.PEDS_COD,
                               CONVERT(VARCHAR(10), TCOM_PEDSAI.PEDS_DAT_CAD, 103) AS data
                           FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
                           INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                           INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                           WHERE
                               TCOM_PEDSAI.STAP_COD = 9
                               AND TCOM_PEDSAI.UNIN_COD = 5
                           ORDER BY TCOM_PEDSAI.PEDS_DAT_CAD ASC; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        // Não utilizados
        /*
        public IEnumerable<dynamic> FaturamentoPorHora()
        {
            string sql = @"SELECT
								DATEPART(hour, NOTF_DAT_EMI) AS hora,
								SUM(NOTF_VAL_NOT) AS valor,
								COUNT(1) as quantidade
							FROM ABACOS.dbo.TGEN_NOTFIS
							INNER JOIN ABACOS.dbo.TCOM_COMERC ON TCOM_COMERC.COME_COD = TGEN_NOTFIS.COME_COD
							WHERE
								CAST(TGEN_NOTFIS.NOTF_DAT_EMI AS DATE) = CAST(GETDATE() AS DATE)
								AND TCOM_COMERC.COMG_COD = 1
								AND NOTF_DAT_CAN IS NULL
							GROUP BY DATEPART(hour, NOTF_DAT_EMI)
							ORDER BY hora ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);

                
            }
        }

        public IEnumerable<dynamic> PedidosAguardandoSeparacao()
        {
            string sql = @"
							(
								SELECT
									COUNT(*) AS quantidade,
									'aguardando_separacao' AS status
								FROM ABACOS.dbo.TCOM_PEDSAI
								INNER JOIN ABACOS.dbo.TCOM_COMERC ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
								WHERE
									TCOM_PEDSAI.STAP_COD = 13
									AND TCOM_COMERC.COMG_COD = 5
									AND TCOM_PEDSAI.PEDS_COD NOT IN (SELECT TGEN_RPTIMP.RPTI_COD_RLC FROM ABACOS.dbo.TGEN_RPTIMP)
							)
							UNION
							(
								SELECT
									COUNT(*) AS quantidade,
									'em_separacao' AS status
								FROM ABACOS.dbo.TCOM_PEDSAI
								INNEr JOIN ABACOS.dbo.TCOM_COMERC ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
								WHERE
									TCOM_PEDSAI.STAP_COD = 13
									AND TCOM_COMERC.COMG_COD = 5
									AND TCOM_PEDSAI.PEDS_COD IN (SELECT TGEN_RPTIMP.RPTI_COD_RLC FROM ABACOS.dbo.TGEN_RPTIMP)
							)
							 ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);

                
            }
        }

        public IEnumerable<dynamic> PerfilDosPedidosFaturados()
        {
            string sql = @"SELECT
								SUM(TGEN_ITENOT.ITEN_QTF_FIS) as linhas
							FROM ABACOS.dbo.TGEN_NOTFIS
							INNER JOIN ABACOS.dbo.TGEN_ITENOT ON TGEN_NOTFIS.NOTF_COD = TGEN_ITENOT.NOTF_COD
							INNER JOIN ABACOS.dbo.TCOM_PEDSAI ON TCOM_PEDSAI.PEDS_COD = TGEN_NOTFIS.PEDS_COD
							INNER JOIN ABACOS.dbo.TCOM_COMERC ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
							WHERE
								CAST(TGEN_NOTFIS.NOTF_DAT_EMI AS DATE) = CAST(GETDATE() AS DATE)
								AND TCOM_COMERC.COMG_COD = 1
								AND TGEN_NOTFIS.NOTF_DAT_CAN IS NULL
							GROUP BY TGEN_NOTFIS.NOTF_COD ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);

                
            }
        }

        public IEnumerable<dynamic> PerfilDosPedidosAFaturar()
        {
            string sql = @"SELECT SUM(TCOM_ITEPDS.ITEP_QTF_PED) as linhas
                           FROM ABACOS.dbo.TCOM_PEDSAI
                           INNER JOIN ABACOS.dbo.TCOM_ITEPDS ON TCOM_PEDSAI.PEDS_COD = TCOM_ITEPDS.PEDS_COD
                           INNER JOIN ABACOS.dbo.TCOM_COMERC ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                           WHERE TCOM_PEDSAI.STAP_COD = 13
                           	AND TCOM_COMERC.COMG_COD = 1
                           	AND TCOM_PEDSAI.PEDS_DAT_CAN IS NULL
                           GROUP BY TCOM_PEDSAI.PEDS_COD ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);

                
            }
        }
        */

        public IEnumerable<dynamic> PendentesOutrasSaidas()
        {
            string sql = @"SELECT COUNT(*) AS quantidade,
                                  SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) AS valor_total,
                                  SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) / COUNT(1) AS ticket_medio
                           FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
                           INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                           INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                           WHERE TCOM_PEDSAI.STAP_COD IN (8, 9)
                           	 AND TCOM_COMERC.COMG_COD = 5
                           	 AND TCOM_PEDSAI.COME_COD NOT IN(79)
                           	 AND TCOM_PEDSAI.PEDS_COD NOT IN(SELECT codigo FROM TMP_PED_NAOMOSTRAR) ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);

                
            }
        }

        public IEnumerable<dynamic> PendentesDevolucoes()
        {
            string sql = @"SELECT
                               COUNT(*) AS quantidade,
                               SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) AS valor_total,
                               SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) / COUNT(1) AS ticket_medio
                           FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
                           INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                           INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                           WHERE
                               TCOM_PEDSAI.STAP_COD IN (8, 9)
                               AND TCOM_COMERC.COMG_COD = 2
                               AND TCOM_PEDSAI.COME_COD NOT IN(79)
                               AND TCOM_PEDSAI.PEDS_COD NOT IN(SELECT codigo FROM TMP_PED_NAOMOSTRAR); ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);

                
            }
        }

        
        public static string GerarHashMd5(string input)
        {
            MD5 md5Hash = MD5.Create();
            // Converter a String para array de bytes, que é como a biblioteca trabalha.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Cria-se um StringBuilder para recompôr a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop para formatar cada byte como uma String em hexadecimal
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public IEnumerable<dynamic> UsuariosLogin(string email, string senha)
        {
            string sql = @"SELECT TOP 1 id,
                                  nome,
                                  email,
                                  id_setor,
                                  id_grupo
                           FROM usuario
                           WHERE email = @email
                             AND senha = @senha
                             AND ativo = 'S'
                           ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@email", email, DbType.String, ParameterDirection.Input);
                parameter.Add("@senha", GerarHashMd5(SALT_SENHA+senha), DbType.String, ParameterDirection.Input);

                return connection.Query<dynamic>(sql, parameter, commandType: CommandType.Text);
            }
        }

        public bool UsuariosLogout(int id)
        {
            string sql = @"UPDATE usuario_sessao SET sessao = 0 WHERE usuario = @id; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@id", id, DbType.String, ParameterDirection.Input);

                return true;
            }
        }

        public bool UsuariosVerificaSessao(int id, string ip)
        {
            if (id == 0)
            {
                return false;
            }
            var agora = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            
            string sql = @"SELECT id 
                             FROM usuario_sessao 
                            WHERE usuario = @id 
                              AND data_sessao > @agora 
                              AND sessao = 1 
                              AND ip = @ip ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@agora", agora, DbType.String, ParameterDirection.Input);
                parameter.Add("@id", id, DbType.Int32, ParameterDirection.Input);
                parameter.Add("@ip", ip, DbType.String, ParameterDirection.Input);

                var query = connection.Query<dynamic>(sql, parameter, commandType: CommandType.Text);
                
                return (query.AsList().Count > 0);
            }
        }

        public IEnumerable<dynamic> UsuariosListar()
        {
            string sql = @"SELECT
                                  usuario.id,
                                  usuario.nome,
                                  usuario.email,
                                  usuario.ativo,
                                  setor_sugestoesperdas.nome AS setor,
                                  setor_sugestoesperdas.id AS id_setor,
                                  grupo.nome AS grupo,
                                  grupo.id AS id_grupo
                           FROM usuario
                           INNER JOIN grupo ON grupo.id = usuario.id_grupo
                           INNER JOIN setor_sugestoesperdas ON setor_sugestoesperdas.id = usuario.id_setor
                           ORDER BY usuario.nome; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<Usuario>(sql);
            }
        }

        public IEnumerable<dynamic> UsuariosDados(int id)
        {
            string sql = @"SELECT id, nome, email, id_setor, id_grupo, ativo FROM usuario WHERE id = @id; ";

            DynamicParameters parameter = new DynamicParameters();
            parameter.Add("@id", id, DbType.Int32, ParameterDirection.Input);

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql, parameter, commandType: CommandType.Text);
            }
        }

        public IEnumerable<dynamic> UsuariosGrupos()
        {
            string sql = @"SELECT id, nome FROM grupo ORDER BY nome ASC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> UsuariosSetores()
        {
            string sql = @"SELECT id, nome FROM setor_sugestoesperdas ORDER BY nome ASC; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<Usuario> UsuarioPorEmail(string email)
        {
            string sql = @"SELECT id, nome, email, id_setor, id_grupo, ativo 
                             FROM usuario 
                            WHERE email = @email; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("email", email, DbType.String, ParameterDirection.Input);

                return connection.Query<Usuario>(sql, parametros);
            }
        }

        public int UsuariosCadastrar(Usuario usuario)
        {
            usuario.senha = GerarHashMd5(SALT_SENHA + usuario.senha);
            string sql = "";
            var existe = UsuarioPorEmail(usuario.email);
            if (!existe.Any())
            {
                sql = @"INSERT INTO usuario(nome, email, id_setor, id_grupo, ativo, senha) 
                        VALUES (@nome, @email, @id_setor, @id_grupo, @ativo, @senha); ";
            } else
            {
                sql = @"UPDATE usuario SET nome = @nome, email = @email, id_setor = @id_setor, id_grupo = @id_grupo, ativo = @ativo, senha = @senha WHERE id = @id; ";
            }

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("nome", usuario.nome, DbType.String, ParameterDirection.Input);
                parametros.Add("email", usuario.email, DbType.String, ParameterDirection.Input);
                parametros.Add("id_setor", usuario.id_setor, DbType.Int64, ParameterDirection.Input);
                parametros.Add("id_grupo", usuario.id_grupo, DbType.Int64, ParameterDirection.Input);
                parametros.Add("ativo", usuario.ativo, DbType.String, ParameterDirection.Input);
                parametros.Add("senha", usuario.senha, DbType.String, ParameterDirection.Input);
                parametros.Add("id", usuario.id, DbType.Int64, ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public bool UsuariosExcluir(int id)
        {
            string sql = "DELETE FROM usuario WHERE id = @id; ";

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@id", id, DbType.Int32, ParameterDirection.Input);

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                connection.Execute(sql, parameters, commandType: CommandType.Text);
                return true;
            }
        }


        public IEnumerable<Relatorio_Model> FaturamentoPorHora()
        {
            string sql = @"SELECT
								DATEPART(hour, NOTF_DAT_EMI) AS hora,
								FORMAT(SUM(NOTF_VAL_NOT), 'C', 'pt-br') AS valor,
								COUNT(1) as quantidade
							FROM ABACOS.dbo.TGEN_NOTFIS
							INNER JOIN ABACOS.dbo.TCOM_COMERC ON TCOM_COMERC.COME_COD = TGEN_NOTFIS.COME_COD
							WHERE
								CAST(TGEN_NOTFIS.NOTF_DAT_EMI AS DATE) = CAST(GETDATE() AS DATE)
								AND TCOM_COMERC.COMG_COD = 1
								AND NOTF_DAT_CAN IS NULL
                                AND TCOM_COMERC.COME_COD <> 152
							GROUP BY DATEPART(hour, NOTF_DAT_EMI)
							ORDER BY hora ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<Relatorio_Model>(sql);
            }
        }

        public IEnumerable<dynamic> FaturamentoDespachados()
        {
            string sql = @"SELECT
								COUNT(1) AS quantidade,
								COALESCE(SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX), 0) AS valor_total,
								COALESCE(SUM(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX) / COUNT(1), 0) AS ticket_medio
							FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
							INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
							INNER JOIN ABACOS.dbo.TCOM_LOGPDS WITH(NOLOCK) ON TCOM_LOGPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
							INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
							WHERE
								CAST(TCOM_LOGPDS.LOGP_DAT_CAD AS DATE) = CAST(GETDATE() AS DATE)
								AND TCOM_LOGPDS.STAP_COD = 27
								AND TCOM_COMERC.COMG_COD = 1 ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> FaturamentoNaoDespachados()
        {
            string sql = @"SELECT COALESCE(TCOM_CPLPDS.CPLP_VAL_TOT-TCOM_CPLPDS.CPLP_VAL_ENCFIX, 0) AS valor_total
                                 ,TCOM_PEDSAI.PEDS_EXT_COD
                                 ,TCOM_PEDSAI.PEDS_COD
                                 ,TCOM_CPLPDS.CPLP_VAL_ENCFIX
                                 ,TCOM_PEDSAI.PEDS_DAT_CAD
                                 ,TCOM_PEDSAI.UNIN_COD
                           FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
                           INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
                           INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                           WHERE TCOM_PEDSAI.STAP_COD = 11
                           	 AND TCOM_COMERC.COMG_COD = 1
                           	 AND TCOM_PEDSAI.COME_COD NOT IN(79)
                           	 AND CONVERT(DATE, PEDS_DAT_ALT) >= CONVERT(DATE, GETDATE()-360)
                           	 AND TCOM_PEDSAI.PEDS_COD NOT IN(SELECT codigo FROM TMP_PED_NAOMOSTRAR); ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> PedidosAguardandoSeparacao()
        {
            string sql = @"
							(
								SELECT
									COUNT(*) AS quantidade,
									'aguardando_separacao' AS status
								FROM ABACOS.dbo.TCOM_PEDSAI
								INNER JOIN ABACOS.dbo.TCOM_COMERC ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
								WHERE
									TCOM_PEDSAI.STAP_COD = 13
									AND TCOM_COMERC.COMG_COD = 5
									AND TCOM_PEDSAI.PEDS_COD NOT IN (SELECT TGEN_RPTIMP.RPTI_COD_RLC FROM ABACOS.dbo.TGEN_RPTIMP)
							)
							UNION
							(
								SELECT
									COUNT(*) AS quantidade,
									'em_separacao' AS status
								FROM ABACOS.dbo.TCOM_PEDSAI
								INNEr JOIN ABACOS.dbo.TCOM_COMERC ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
								WHERE
									TCOM_PEDSAI.STAP_COD = 13
									AND TCOM_COMERC.COMG_COD = 5
									AND TCOM_PEDSAI.PEDS_COD IN (SELECT TGEN_RPTIMP.RPTI_COD_RLC FROM ABACOS.dbo.TGEN_RPTIMP)
							); ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);                
            }
        }

        public IEnumerable<dynamic> PerfilDosPedidosFaturados()
        {
            string sql = @"SELECT
								SUM(TGEN_ITENOT.ITEN_QTF_FIS) as linhas
							FROM ABACOS.dbo.TGEN_NOTFIS
							INNER JOIN ABACOS.dbo.TGEN_ITENOT ON TGEN_NOTFIS.NOTF_COD = TGEN_ITENOT.NOTF_COD
							INNER JOIN ABACOS.dbo.TCOM_PEDSAI ON TCOM_PEDSAI.PEDS_COD = TGEN_NOTFIS.PEDS_COD
							INNER JOIN ABACOS.dbo.TCOM_COMERC ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
							WHERE
								CAST(TGEN_NOTFIS.NOTF_DAT_EMI AS DATE) = CAST(GETDATE() AS DATE)
								AND TCOM_COMERC.COMG_COD = 1
								AND TGEN_NOTFIS.NOTF_DAT_CAN IS NULL
							GROUP BY TGEN_NOTFIS.NOTF_COD ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> PerfilDosPedidosAFaturar()
        {
            string sql = @"SELECT SUM(TCOM_ITEPDS.ITEP_QTF_PED) as linhas
                           FROM ABACOS.dbo.TCOM_PEDSAI
                           INNER JOIN ABACOS.dbo.TCOM_ITEPDS ON TCOM_PEDSAI.PEDS_COD = TCOM_ITEPDS.PEDS_COD
                           INNER JOIN ABACOS.dbo.TCOM_COMERC ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
                           WHERE TCOM_PEDSAI.STAP_COD = 13
                           	 AND TCOM_COMERC.COMG_COD = 1
                           	 AND TCOM_PEDSAI.PEDS_DAT_CAN IS NULL
                           GROUP BY TCOM_PEDSAI.PEDS_COD; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        // Captacao
        public IEnumerable<dynamic> CaptacaoLojaVirtualIntegracaoVtex()
        {
            string sql = @"SELECT COUNT(distinct PedidoCodigoExterno) AS quantidade
                                 ,COALESCE(SUM(PrecoVenda * Quantidade), 0) AS valor_total
                                 ,CASE COUNT(distinct PedidoCodigoExterno)
                                    WHEN 0 THEN 0
                                  ELSE COALESCE(SUM(PrecoVenda * Quantidade), 0) / COUNT(distinct PedidoCodigoExterno)
                                  END AS ticket_medio
                             FROM IntegracoesVtex_.dbo.PedidoCaptacaoProdutos
                            WHERE CONVERT(DATE,DataCriacaoRegistro) = CONVERT(date, GETDATE()) 
                              AND Vendedor = '1' AND PedidoCodigoExterno LIKE '%cp%' ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> CaptacaoMarketPlaceIntegracaoVtex()
        {
            string sql = @"SELECT COUNT(*) AS quantidade,
                                  ISNULL(SUM(ISNULL(TotalItens, 0)), 0) + ISNULL(SUM(ISNULL(TotalFrete, 0)), 0) AS valor_total,
                                  ISNULL( AVG((ISNULL(TotalItens, 0)) + (ISNULL(TotalFrete, 0))) , 0) AS ticket_medio
                           FROM IntegracoesVtex_.dbo.PedidoCaptacao
                           WHERE CONVERT(DATE,DataPedido) = CONVERT(date, GETDATE()) AND
                                 Origem != 'Vtex'; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> CaptacaoMarketPlaceConnectIntegracaoVtex()
        {
            string sql = @"SELECT
                                  COUNT(DISTINCT PedidoCaptacao.CodigoExterno) AS quantidade
                                 ,ISNULL(SUM(ISNULL(PedidoCaptacaoProdutos.PrecoVenda, 0) * ISNULL(PedidoCaptacaoProdutos.Quantidade, 0)), 0) AS valor_total
								 ,CASE COUNT(DISTINCT PedidoCaptacao.CodigoExterno)
                                    WHEN 0 THEN 0
                                    ELSE ISNULL(SUM(ISNULL(PedidoCaptacaoProdutos.PrecoVenda, 0) * ISNULL(PedidoCaptacaoProdutos.Quantidade, 0)), 0) / COUNT(DISTINCT PedidoCaptacao.CodigoExterno)
                                  END AS ticket_medio
                           FROM IntegracoesVtex_.dbo.PedidoCaptacaoProdutos
                           INNER JOIN IntegracoesVtex_.dbo.PedidoCaptacao ON (PedidoCaptacao.CodigoExterno = PedidoCaptacaoProdutos.PedidoCodigoExterno)
                           WHERE
                                 CONVERT(DATE,DataPedido) = CONVERT(date, GETDATE()) AND
                                 Origem = 'Vtex' and Vendedor != '1'; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> CaptacaoMercadoLivreIntegracaoVtex()
        {
            string sql = @"SELECT
                                  COUNT(*) AS quantidade,
                                  ISNULL(SUM(PEDS_VAL_PED), 0) AS valor_total,
                                  ISNULL(SUM(PEDS_VAL_PED) / COUNT(*), 0) AS ticket_medio
                           FROM ABACOS.dbo.TCOM_PEDSAI
                           WHERE
                                 CAST(TCOM_PEDSAI.PEDS_DAT_MOV AS DATE) = CAST(GETDATE() AS DATE)
                                 AND TCOM_PEDSAI.COME_COD IN (1,12,22,67,79,94,96,124,128,135,162,158, 161)
                                 AND TCOM_PEDSAI.STAP_COD != 12
                                 AND TCOM_PEDSAI.ORIP_COD IN (100001,100003); ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> CaptacaoPedidosDigitadosIntegracaoVtex()
        {
            string sql = @"SELECT
                                  COUNT(*) AS quantidade,
                                  ISNULL(SUM(ISNULL(PEDS_VAL_PED, 0)), 0) AS valor_total,
                                  ISNULL(AVG(ISNULL(PEDS_VAL_PED, 0)), 0) AS ticket_medio
                           FROM ABACOS.dbo.TCOM_PEDSAI
                           WHERE
                                 CONVERT(date,TCOM_PEDSAI.PEDS_DAT_MOV) = CONVERT(date, GETDATE())
                                 AND TCOM_PEDSAI.COME_COD IN (1,12,22,67,79,94,96,124,128,135)
                                 AND TCOM_PEDSAI.STAP_COD != 12
                                 AND TCOM_PEDSAI.ORIP_COD = 1; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<dynamic> FaturamentoCreditoAprovado()
        {
            string sql = @"SELECT
                                  r1.quantidade
                             FROM dbo.resumo_pedido_por_status AS r1
                             INNER JOIN (
                                 SELECT
                                        r2.status,
                                        MAX(r2.dataConsulta) AS ultimaConsulta
                                 FROM dbo.resumo_pedido_por_status AS r2
                                 WHERE
                                       r2.status = 'CAP'
                                 GROUP BY r2.status
                             ) AS ultimosRegistros ON ultimosRegistros.status = r1.status AND ultimosRegistros.ultimaConsulta = r1.dataConsulta; ";

            using var connection = new SqlConnection(_connectionStringSigeco);
            {
                return connection.Query<dynamic>(sql);
            }
        }



        public async Task<dynamic> AbacosInsereCotacao(Cotacao_Model cotacao)
        {
            int codigo = 0;

            using var connection = new SqlConnection(_connectionString);
            {
                connection.Open();
                var transacao = connection.BeginTransaction();

                // Dados do cabeçalho do pedido
                DynamicParameters _param = new DynamicParameters();
                _param.Add("@pACAO", "I", DbType.String, ParameterDirection.Input);
                _param.Add("@pUNIM_COD", 1, DbType.Int32, ParameterDirection.Input);
                _param.Add("@pPEDC_COD", codigo, DbType.Int32, ParameterDirection.Output);
                _param.Add("@pEMPR_COD", cotacao.empresa, DbType.Int32, ParameterDirection.Input);
                _param.Add("@pCLIF_COD", cotacao.cliente, DbType.Int32, ParameterDirection.Input);
                _param.Add("@pPEDC_EXT_COD", cotacao.codigoExterno, DbType.String, ParameterDirection.InputOutput);
                _param.Add("@pPEDC_VAL_PED", cotacao.valor, DbType.Double, ParameterDirection.Input);
                _param.Add("@pPEDC_VAL_CVS", cotacao.conversao, DbType.Double, ParameterDirection.Input);
                _param.Add("@pPEDC_VAL_ENC", cotacao.encargos, DbType.Double, ParameterDirection.Input);
                _param.Add("@pPEDC_VAL_FRE", cotacao.frete, DbType.Double, ParameterDirection.Input);
                _param.Add("@pPEDC_VAL_IMP", cotacao.impostos, DbType.Double, ParameterDirection.Input);
                _param.Add("@pPEDC_VAL_IPI", 0, DbType.Int32, ParameterDirection.Input);
                _param.Add("@pPEDC_OBS", cotacao.obs, DbType.String, ParameterDirection.Input);
                _param.Add("@pPEDC_DES_VEN", cotacao.vendedor, DbType.String, ParameterDirection.Input);
                _param.Add("@pUSUS_COD", 3, DbType.Int32, ParameterDirection.Input); // Usuario
                _param.Add("@pCENC_COD", cotacao.centro_custo, DbType.Int32, ParameterDirection.Input);
                // 108=Mercadoria para Revenda
                _param.Add("@pPLAC_COD", 108, DbType.Int32, ParameterDirection.Input); // Código da conta
                // 8=DESPESA VARIAVEL
                _param.Add("@pPLAL_COD", 8, DbType.Int32, ParameterDirection.Input); // Código da conta de lançamento
                _param.Add("@pUNIN_COD", 1, DbType.Int16, ParameterDirection.Input); // Unidade de Negócio
                _param.Add("@pPEDC_CHR_SN_CLAPAD", "N", DbType.String, ParameterDirection.Input);
                _param.Add("@pPEDC_CHR_TIP", "1", DbType.String, ParameterDirection.Input);
                _param.Add("@pTIPC_COD", cotacao.tipoCompra, DbType.Int32, ParameterDirection.Input);
                _param.Add("@pPEDC_VAL_ICMSUB", 0, DbType.Int32, ParameterDirection.Input);


                var query = connection.Execute("ABACOS.dbo.PCPR_C_PEDCOM", param: _param, transaction: transacao, commandType: CommandType.StoredProcedure);

                codigo = _param.Get<int>("@pPEDC_COD");


                // Dados dos itens do pedido
                int itecod = 0;
                foreach(var item in cotacao.itens) { 
                    _param = new DynamicParameters();
                    _param.Add("@pACAO", "I", DbType.String, ParameterDirection.Input);
                    _param.Add("@pITEC_COD", itecod, DbType.Int32, ParameterDirection.InputOutput);
                    _param.Add("@pPEDC_COD", codigo, DbType.Int32, ParameterDirection.Input);
                    _param.Add("@pPROS_COD", item.codigoProduto, DbType.Int32, ParameterDirection.Input);
                    _param.Add("@pITEC_QTF_PED", item.quantidade, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_VAL_PREBRU", item.precoBruto, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_VAL_DESTOT", item.descontoTotal, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_VAL_PRELIQ", item.precoLiquido, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_DET", null, DbType.String, ParameterDirection.Input);
                    _param.Add("@pITEC_ALQ_IPI", 0, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_ALQ_ICM", 0, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_DAT_PREENT", null, DbType.DateTime, ParameterDirection.Input);
                    _param.Add("@pPEDS_EXT_COD", cotacao.codigoExterno, DbType.String, ParameterDirection.Input);
                    _param.Add("@pITEC_VAL_ICM", 0, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_VAL_IPI", 0, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_PER_MVAICMSUB", 0, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_PER_REDICMSUB", 0, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_ALQ_ICMSSUB", 0, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_VAL_ICMSSUB", 0, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_VAL_FRE", 0, DbType.Double, ParameterDirection.Input);
                    _param.Add("@pITEC_VAL_ENC", 0, DbType.Double, ParameterDirection.Input);

                    connection.Execute("ABACOS.dbo.PCPR_C_ITECOM", param: _param, transaction: transacao, commandType: CommandType.StoredProcedure);
                }
                

                transacao.Commit();




                return query;
            }
        }

        public async Task<IEnumerable<dynamic>> PedidosAPIConsulting()
        {
            const string sql = @"SELECT 
                                    COUNT(1) quantidade, MIN(DataLog) DtMaisAntiga, MAX(DataLog) DtMaisNova, 
                                    (SELECT MAX(DataConfirmacao) DataReplicacao 
								            FROM
									            [ABACOS].[rpl].[Pedido] (NOLOCK) 
								            WHERE 
									            DataConfirmacao IS NOT NULL AND CodigoInterface = 34 AND DataConfirmacao < GETDATE()
								            GROUP BY 
									            Acao, CodigoInterface) DtUltimaReplicacao
                                FROM
                                    [ABACOS].[rpl].[Pedido] (NOLOCK) 
                                WHERE 
                                    DataConfirmacao IS NULL AND CodigoInterface = 34 
                                GROUP 
                                    BY Acao, CodigoInterface";

            using var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }

        }

        public async Task<IEnumerable<dynamic>> Cubos()
        {
            const string sql = @"SELECT * 
                                FROM    
                                    [Traces].[dbo].[CheckList_Job_Demorados]  
                                WHERE 
                                    Job_Name = '2_Processamento dos cubos_SSIS'";

            using var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> ReplicacaoDataWarehouse()
        {
            const string sql = @"SELECT * 
                                FROM 
                                    [Traces].[dbo].[CheckList_Job_Demorados] 
                                WHERE 
                                    Job_Name = '1_Atualizar DATAWAREHOUSE'";

            using var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> EnvioIntelipost()
        {
            const string sql = @"SELECT 
                                    TOP 1
                                    descricao 'NOME JOB'
                                    ,format(inicio,'dd/MM/yyyy HH:mm:ss') AS 'INICIO'
                                    ,format(fim,'dd/MM/yyyy HH:mm:ss') AS 'FIM'
                                    ,DATEDIFF(MINUTE,inicio,GETDATE()) 'DIF_MINUTOS'
                                    ,qtenvio 'QT_ENVIADOS'
                                FROM 
                                    DATAWAREHOUSE.dbo.log_tms_inteliepost
                                WHERE 
                                    descricao = 'tms_intelipost_loader_service'
                                ORDER BY 
                                    id_log DESC";

            using var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }


        public async Task<IEnumerable<dynamic>> Integracao00k()
        {
            const string sql = @"SELECT 
                                    Pedido.PEDS_COD AS Codigo, 
                                    Pedido.PEDS_EXT_COD AS CodigoExterno, 
                                    PedidoOrigem.ORIP_COD AS CodigoExterno, 
                                    RTrim(PedidoOrigem.ORIP_NOM) AS Origem,
                                    Pedido.PEDS_DAT_CAD AS DataCadastro, 
                                    Pedido.PEDS_DAT_MOV AS DataMovimentacao,
                                    Pedido.PEDS_DAT_FAT AS DataFaturamento, 
                                    Pedido.PEDS_DAT_SAI AS DataSaida,
                                    Pedido.PEDS_DAT_ALT AS DataAlteracao
                                FROM 
                                    TCOM_PEDSAI Pedido 
                                LEFT JOIN TCOM_ORIPED PedidoOrigem ON (PedidoOrigem.ORIP_COD = Pedido.ORIP_COD)
                                WHERE 
                                    PedidoOrigem.ORIP_COD IN (100001) AND CAST(Pedido.PEDS_DAT_CAD AS DATE) = CAST(GETDATE() AS DATE)
                                ORDER 
                                    BY PEDS_DAT_CAD DESC";

            var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> IntegracaoVtex()
        {
            const string sql = @"SELECT 
                                    Pedido.PEDS_COD AS Codigo, 
                                    Pedido.PEDS_EXT_COD AS CodigoExterno, 
                                    PedidoOrigem.ORIP_COD AS CodigoExterno, 
                                    RTrim(PedidoOrigem.ORIP_NOM) AS Origem,
                                    Pedido.PEDS_DAT_CAD AS DataCadastro, 
                                    Pedido.PEDS_DAT_MOV AS DataMovimentacao,
                                    Pedido.PEDS_DAT_FAT AS DataFaturamento, 
                                    Pedido.PEDS_DAT_SAI AS DataSaida,
                                    Pedido.PEDS_DAT_ALT AS DataAlteracao
                                FROM 
                                    TCOM_PEDSAI Pedido 
                                LEFT JOIN TCOM_ORIPED PedidoOrigem ON (PedidoOrigem.ORIP_COD = Pedido.ORIP_COD)
                                WHERE 
                                    PedidoOrigem.ORIP_COD IN (100000) AND CAST(Pedido.PEDS_DAT_CAD AS DATE) = CAST(GETDATE() AS DATE)
                                ORDER BY 
                                    PEDS_DAT_CAD DESC";

            var conenction = new SqlConnection(_connectionString);
            {
                return await conenction.QueryAsync<dynamic>(sql);
            }
        }
        public async Task<IEnumerable<dynamic>> PedidosIntegradosPorHoraVia00k() 
        {
            const string sql = @"SELECT 
                                    origem = CASE OP.ORIP_COD 
                                    WHEN 
                                        100001 THEN '00K' END,
                                        DATEPART(HOUR, PE.PEDS_DAT_CAD) AS hora,
                                        COUNT(1) total 
                                    FROM 
                                        ABACOS.DBO.TCOM_PEDSAI PE WITH(NOLOCK) 
                                    JOIN 
                                        ABACOS.DBO.TCOM_STAPDS ST WITH(NOLOCK) ON PE.STAP_COD = ST.STAP_COD 
                                    JOIN 
                                        ABACOS.DBO.TCOM_ORIPED OP WITH(NOLOCK) ON OP.ORIP_COD = PE.ORIP_COD WHERE 1=1 
                                        AND PE.ORIP_COD IN(100001) 
                                        AND PE.PEDS_DAT_CAD BETWEEN DATEADD(dd, DATEDIFF(dd, 0, GETDATE()), 0) AND DATEADD(ms ,-3 ,DATEADD(dd, DATEDIFF(dd, 0, GETDATE()) + 1, 0)) 
                                        AND OP.ORIP_COD IS NOT NULL GROUP BY OP.ORIP_COD, DATEPART(HOUR, PE.PEDS_DAT_CAD)";

            var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> PedidosIntegradosPorHoraViaVtex()
        {
            const string sql = @"SELECT 
                                    origem = CASE OP.ORIP_COD 
                                    WHEN 
                                        100000 THEN 'VTEX' END ,DATEPART(HOUR, PE.PEDS_DAT_CAD) AS hora ,COUNT(1) total 
                                FROM 
                                    ABACOS.DBO.TCOM_PEDSAI PE WITH(NOLOCK) 
                                JOIN 
                                    ABACOS.DBO.TCOM_STAPDS ST WITH(NOLOCK) ON PE.STAP_COD = ST.STAP_COD 
                                JOIN 
                                    ABACOS.DBO.TCOM_ORIPED OP WITH(NOLOCK) ON OP.ORIP_COD = PE.ORIP_COD WHERE 1=1 
                                    AND PE.ORIP_COD IN(100000) 
                                    AND PE.PEDS_DAT_CAD BETWEEN DATEADD(dd, DATEDIFF(dd, 0, GETDATE()), 0) AND DATEADD(ms ,-3 ,DATEADD(dd, DATEDIFF(dd, 0, GETDATE()) + 1, 0)) 
                                    AND OP.ORIP_COD IS NOT NULL GROUP BY OP.ORIP_COD, DATEPART(HOUR, PE.PEDS_DAT_CAD)";

            var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioPedidosPorPeriodo(DateTime dataInicial, DateTime dataFinal, int deslocamento, int limite)
        {
            string sql = @"SELECT
	                            TCOM_STAPDS.STAP_NOM AS Status,
	                            PEDS_DAT_CAD AS Data_Compra,
	                            PEDS_COD AS Cod_Interno,
	                            PEDS_EXT_COD AS Cod_Externo,
	                            PEDS_VAL_REA AS Valor_Pedido,
	                            PEDS_DAT_FAT AS Data_Faturamento,
	                            (SELECT 
		                            COUNT(1) 
	                            FROM 
		                            abacos.dbo.tcom_pedsai (NOLOCK) 
		                            INNER JOIN abacos.dbo.TCOM_STAPDS (NOLOCK) ON TCOM_STAPDS.STAP_COD = TCOM_PEDSAI.STAP_COD
	                            WHERE
		                            peds_dat_cad BETWEEN @dataInicial + ' 00:00:00' and @dataFinal + ' 23:59:59') AS Total,
	                            (SELECT
		                            SUM(PEDS_VAL_REA)
	                            FROM
		                            abacos.dbo.tcom_pedsai (NOLOCK)
		                            INNER JOIN abacos.dbo.TCOM_STAPDS (NOLOCK) ON TCOM_STAPDS.STAP_COD = TCOM_PEDSAI.STAP_COD
	                            WHERE
		                            peds_dat_cad BETWEEN @dataInicial + ' 00:00:00' and @dataFinal + ' 23:59:59') as Soma
                            FROM
	                            abacos.dbo.tcom_pedsai (NOLOCK)
	                            INNER JOIN abacos.dbo.TCOM_STAPDS (NOLOCK) ON TCOM_STAPDS.STAP_COD = TCOM_PEDSAI.STAP_COD
                            WHERE
	                            peds_dat_cad BETWEEN @dataInicial + ' 00:00:00' and @dataFinal + ' 23:59:59'
                            GROUP BY
	                            TCOM_STAPDS.STAP_NOM, PEDS_DAT_CAD, PEDS_COD, PEDS_EXT_COD, PEDS_VAL_REA, PEDS_DAT_FAT
                                ORDER BY PEDS_DAT_CAD ASC OFFSET @deslocamento ROWS FETCH NEXT @limite ROWS ONLY; ";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametro = new DynamicParameters();
                parametro.Add("dataInicial", dataInicial, DbType.DateTime, ParameterDirection.Input);
                parametro.Add("dataFinal", dataFinal, DbType.DateTime, ParameterDirection.Input);
                parametro.Add("deslocamento", deslocamento, DbType.Int16, ParameterDirection.Input);
                parametro.Add("limite", limite, DbType.Int32, ParameterDirection.Input);
                return await connection.QueryAsync<dynamic>(sql, parametro);
            }
        }
        public async Task<IEnumerable<dynamic>> RelatorioSaldoPorCodigo(String dk)
        {
            string sql = @"SELECT	
 	                             PR.PROS_EXT_COD AS DK
	                            ,REPLACE(ISNULL(RTRIM(CONVERT(VARCHAR(250), PR.PROS_DET)), RTRIM(PR.PROS_NOM)),CHAR(13) + Char(10) ,' ') AS DESCRICAO
	                            ,ISNULL(S1.SALP_QTF_DIS, 0) AS CONNECT
	                            ,ISNULL(S2.SALP_QTF_DIS, 0) AS VIRTUAL
                             FROM	
	                            ABACOS.DBO.TCOM_PROSER AS PR WITH (NOLOCK)
	                            LEFT JOIN ABACOS.DBO.TEST_SALPRO AS S1 WITH (NOLOCK) ON S1.PROS_COD = PR.PROS_COD AND S1.ALMO_COD = 8
	                            LEFT JOIN ABACOS.DBO.TEST_SALPRO AS S2 WITH (NOLOCK) ON S2.PROS_COD = PR.PROS_COD AND S2.ALMO_COD = 21
                            WHERE	
	                            PR.PROS_EXT_COD = @dk
	                            AND PR.PROS_DAT_FIM IS NULL
	                            AND PR.PROS_CHR_REGFISKIT = 0
	                            AND PR.PROS_COD <> PR.PROS_COD_PAI";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametro = new DynamicParameters();
                parametro.Add("dk", dk, DbType.String, ParameterDirection.Input);
                return await connection.QueryAsync<dynamic>(sql, parametro);
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioSaldoPorSaldo1()
        {
            string sql = @"SELECT
 	                             PR.PROS_EXT_COD AS DK
	                            ,REPLACE(ISNULL(RTRIM(CONVERT(VARCHAR(250), PR.PROS_DET)), RTRIM(PR.PROS_NOM)),CHAR(13) + Char(10) ,' ') AS DESCRICAO
	                            ,S1.SALP_QTF_DIS AS SALDO_TOTAL
                             FROM	
	                            ABACOS.DBO.TCOM_PROSER AS PR WITH (NOLOCK)
	                            LEFT JOIN ABACOS.DBO.TEST_SALPRO AS S1 WITH (NOLOCK) ON S1.PROS_COD = PR.PROS_COD AND S1.ALMO_COD = 8
	                            LEFT JOIN ABACOS.DBO.TEST_SALPRO AS S2 WITH (NOLOCK) ON S2.PROS_COD = PR.PROS_COD AND S2.ALMO_COD = 21
                            WHERE	
	                            PR.PROS_DAT_FIM IS NULL
	                            AND PR.PROS_CHR_REGFISKIT = 0
	                            AND PR.PROS_COD <> PR.PROS_COD_PAI
								AND S1.SALP_QTF_DIS = 1
								AND S2.SALP_QTF_DIS = 0";

            var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioPrecoPorMarketplace(string codigo, int deslocamento, int limite)
        {
            string sql = @"SELECT 
                                TCOM_PROSER.PROS_EXT_COD AS DK, 
                                TCOM_PROSER.PROS_NOM_RED  AS NOME, 
                                TCOM_PROLIS.PROL_VAL_PRE AS Preco_DE, 
                                TCOM_PROLIS.PROL_VAL_PREPRO AS Preco_POR,
                                (SELECT 
                                    COUNT(1)
                                FROM
                                    ABACOS.DBO.TCOM_PROLIS WITH (NOLOCK)
                                WHERE
                                    TCOM_PROLIS.LISP_COD = @codigo
                                    AND TCOM_PROLIS.PROL_VAL_PREPRO > 0
                                    AND TCOM_PROLIS.PROL_VAL_PREPRO != floor(TCOM_PROLIS.PROL_VAL_PREPRO)) as Total
                            FROM 
                                ABACOS.DBO.TCOM_PROLIS WITH (NOLOCK) 
                            INNER JOIN abacos.dbo.TCOM_PROSER ON TCOM_PROSER.PROS_COD = TCOM_PROLIS.PROS_COD
                            WHERE 
                                TCOM_PROLIS.LISP_COD = @codigo
                                AND TCOM_PROLIS.PROL_VAL_PREPRO > 0
                                AND TCOM_PROLIS.PROL_VAL_PREPRO != floor(TCOM_PROLIS.PROL_VAL_PREPRO)
                            ORDER BY TCOM_PROSER.PROS_EXT_COD ASC OFFSET @deslocamento ROWS FETCH NEXT @limite ROWS ONLY;";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametro = new DynamicParameters();
                parametro.Add("codigo", codigo, DbType.String, ParameterDirection.Input);
                parametro.Add("deslocamento", deslocamento, DbType.Int16, ParameterDirection.Input);
                parametro.Add("limite", limite, DbType.Int32, ParameterDirection.Input);
                return await connection.QueryAsync<dynamic>(sql, parametro);
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioPrecoPorCodigo(string dk)
        {
            string sql = @"SELECT 
                                TCOM_LISPRE.LISP_NOM as Lista, TCOM_PROSER.PROS_EXT_COD AS DK, TCOM_PROSER.PROS_NOM  AS NOME, TCOM_PROLIS.PROL_VAL_PRE AS Preco_DE, TCOM_PROLIS.PROL_VAL_PREPRO AS Preco_POR
                            FROM 
                                ABACOS.DBO.TCOM_PROLIS WITH (NOLOCK) 
                            INNER JOIN abacos.dbo.TCOM_PROSER ON TCOM_PROSER.PROS_COD = TCOM_PROLIS.PROS_COD
                            INNER JOIN abacos.dbo.TCOM_LISPRE ON TCOM_LISPRE.LISP_COD = TCOM_PROLIS.LISP_COD
                            WHERE 
                                TCOM_PROSER.PROS_EXT_COD = @dk;";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametro = new DynamicParameters();
                parametro.Add("dk", dk, DbType.String, ParameterDirection.Input);
                return await connection.QueryAsync<dynamic>(sql, parametro);
            }
        }

        public async Task<IEnumerable<dynamic>> CodigoListaDePreco()
        {
            string sql = @"SELECT 
                                LISP_COD, LISP_NOM FROM ABACOS.dbo.TCOM_LISPRE;";

            var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioPendentesVendasPorPeriodo(DateTime dataInicial, DateTime dataFinal, int deslocamento, int limite)
        {
            string sql = @"SELECT 
                                Pedidos.PEDS_EXT_COD,	
                                ItemPedido.PEDS_COD, 
                                ItemPedido.ITEP_VAL_PREBRU AS valor_produtos,
                                ISNULL((SELECT
                                            ISNULL(Comercial.CPLP_VAL_TOT, 0)
                                        FROM 
                                            ABACOS.dbo.TCOM_PEDSAI AS Pedido WITH(NOLOCK)
                                            INNER JOIN ABACOS.dbo.TCOM_CPLPDS AS Comercial WITH(NOLOCK) ON Comercial.PEDS_COD = Pedido.PEDS_COD
                                            INNER JOIN ABACOS.dbo.TCOM_COMERC AS Comercializacao WITH(NOLOCK) ON Comercializacao.COME_COD = Pedido.COME_COD
                                        WHERE
                                            Pedido.STAP_COD NOT IN (11, 12)
                                            AND Pedido.STAP_COD IN (8, 9)
                                            AND Comercializacao.COMG_COD = 1
                                            AND Pedido.UNIN_COD = 5
                                            AND Pedido.PEDS_EXT_COD = Pedidos.PEDS_EXT_COD
                                            AND Pedido.PEDS_DAT_CAN IS NULL)
                                            ,Comercial.CPLP_VAL_TOT) AS valor_vendas,
                                Pedidos.PEDS_DAT_CAD,
                                CASE
                                    WHEN ClasseProduto.CLAP_NOM LIKE '%DROP%' THEN 'DROP'
                                    WHEN ClasseProduto.CLAP_NOM LIKE '%CROSS%' THEN 'CROSS'
                                    ELSE 'NORMAL'
                                END AS dropshipping,
                                ClasseProduto.CLAP_NOM,
                                ItemPedido.ITEP_COD,
                                Produto.PROS_EXT_COD,
                                Comercial.CPLP_VAL_ENCFIX,
                                MarcaProduto.MARP_NOM,
                                COUNT(1) OVER() AS Total

                            FROM ABACOS.dbo.TCOM_PEDSAI AS Pedidos WITH(NOLOCK)
                            INNER JOIN ABACOS.dbo.TCOM_ITEPDS AS ItemPedido WITH(NOLOCK) ON ItemPedido.PEDS_COD = Pedidos.PEDS_COD
                            INNER JOIN ABACOS.dbo.TCOM_COMERC AS Comercializacao WITH(NOLOCK) ON Comercializacao.COME_COD = Pedidos.COME_COD
                            INNER JOIN ABACOS.dbo.TCOM_CPLPDS AS Comercial WITH(NOLOCK) ON Comercial.PEDS_COD = Pedidos.PEDS_COD
                            INNER JOIN ABACOS.dbo.TCOM_PROSER AS Produto WITH(NOLOCK) ON Produto.PROS_COD = ItemPedido.PROS_COD
                            INNER JOIN ABACOS.dbo.TCOM_CLAPRO AS ClasseProduto WITH(NOLOCK) ON ClasseProduto.CLAP_COD = Produto.CLAP_COD
                            LEFT JOIN ABACOS.dbo.TCOM_MARPRO AS MarcaProduto WITH(NOLOCK) ON Produto.MARP_COD = MarcaProduto.MARP_COD

                            WHERE 
                                Pedidos.UNIN_COD = 1
                                AND Pedidos.STAP_COD IN (8, 9)
                                AND Pedidos.STAP_COD NOT IN (11, 12)
                                AND Pedidos.PEDS_DAT_CAN IS NULL
                                AND Pedidos.PEDS_DAT_CAD BETWEEN @dataInicial + ' 00:00:00' AND @dataFinal + ' 23:59:59'
                                AND ItemPedido.ITEP_QTF_PEN > 0	
	
                            ORDER BY 
                                Pedidos.PEDS_DAT_CAD 

                            OFFSET @deslocamento ROWS FETCH NEXT @limite ROWS ONLY";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametro = new DynamicParameters();
                parametro.Add("dataInicial", dataInicial, DbType.DateTime, ParameterDirection.Input);
                parametro.Add("dataFinal", dataFinal, DbType.DateTime, ParameterDirection.Input);
                parametro.Add("deslocamento", deslocamento, DbType.Int16, ParameterDirection.Input);
                parametro.Add("limite", limite, DbType.Int16, ParameterDirection.Input);
                return await connection.QueryAsync<dynamic>(sql, parametro);
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioPendentesVendasData()
        {
             string sql = @"SELECT
                            (SELECT MIN(PEDS_DAT_CAD)
                             FROM ABACOS.dbo.TCOM_PEDSAI Pedido
                                 RIGHT JOIN ABACOS.dbo.TCOM_LOGPDS ON Pedido.PEDS_COD = ABACOS.dbo.TCOM_LOGPDS.PEDS_COD
                             WHERE ABACOS.dbo.TCOM_LOGPDS.STAP_COD IN (9, 8)
                                   AND Pedido.stap_cod IN (9, 8)
                                   AND Pedido.UNIN_COD = '1'
                                   AND Pedido.COME_COD IN (157, 159)) AS PEDS_MAIS_ANTIGO,
                            (SELECT MAX(PEDS_DAT_CAD)
                             FROM ABACOS.dbo.TCOM_PEDSAI Pedido
                                 RIGHT JOIN ABACOS.dbo.TCOM_LOGPDS ON Pedido.PEDS_COD = ABACOS.dbo.TCOM_LOGPDS.PEDS_COD
                             WHERE ABACOS.dbo.TCOM_LOGPDS.STAP_COD IN (9, 8)
                                   AND Pedido.stap_cod IN (9, 8)
                                   AND Pedido.UNIN_COD = '1'
                                   AND Pedido.COME_COD IN (157, 159)) AS PEDS_MAIS_NOVO";

            var connection = new SqlConnection(_connectionString);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioPendentesVendasPorPeriodoGrafico(DateTime dataInicial, DateTime dataFinal)
        {
            string sql = @"SELECT DISTINCT
                                CONVERT(date, Pedido.PEDS_DAT_CAD) AS 'Data',
                                COUNT(DISTINCT Pedido.PEDS_COD) AS 'Total'
                            FROM
                                ABACOS.dbo.TCOM_PEDSAI Pedido
                                RIGHT JOIN ABACOS.dbo.TCOM_LOGPDS ON Pedido.PEDS_COD = ABACOS.dbo.TCOM_LOGPDS.PEDS_COD
                                INNER JOIN ABACOS.DBO.TCOM_CPLPDS ON Pedido.PEDS_COD = ABACOS.dbo.TCOM_CPLPDS.PEDS_COD
                            WHERE
                                ABACOS.dbo.TCOM_LOGPDS.STAP_COD IN (9, 8)
                                AND Pedido.stap_cod IN (9, 8)
                                AND Pedido.UNIN_COD = '1'
                                AND Pedido.COME_COD IN (157, 159)
                                AND Pedido.PEDS_DAT_CAD BETWEEN '2022-02-05' + ' 00:00:00' AND '2023-05-31' + ' 23:59:59'
                            GROUP BY
                                CONVERT(date, Pedido.PEDS_DAT_CAD)
                            ORDER BY
                                CONVERT(date, Pedido.PEDS_DAT_CAD)";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametro = new DynamicParameters();
                parametro.Add("dataInicial", dataInicial, DbType.DateTime, ParameterDirection.Input);
                parametro.Add("dataFinal", dataFinal, DbType.DateTime, ParameterDirection.Input);
                return await connection.QueryAsync<dynamic>(sql, parametro);
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioPendentesVendasPorCodigo(string codigo)
        {
            string sql = @"SELECT * FROM
                            (
	                        SELECT TCOM_PEDSAI.PEDS_EXT_COD
								                        ,TCOM_PEDSAI.PEDS_COD
								                        ,TCOM_ITEPDS.ITEP_COD
								                        ,ABACOS.dbo.TCOM_PROSER.PROS_EXT_COD
								                        ,ISNULL((SELECT ISNULL(Comercial.CPLP_VAL_TOT, 0)
										                        FROM ABACOS.DBO.TCOM_PEDSAI (NOLOCK) AS Pedido
										                        INNER JOIN ABACOS.DBO.TCOM_CPLPDS (NOLOCK) AS Comercial
											                        ON Comercial.PEDS_COD = Pedido.PEDS_COD
										                        INNER JOIN ABACOS.DBO.TCOM_COMERC (NOLOCK) AS Comercializacao
											                        ON Comercializacao.COME_COD = Pedido.COME_COD
										                        WHERE Pedido.STAP_COD NOT IN (11, 12)  /*11=FATURADO, 12=CANCELADO*/
											                        AND Comercializacao.COMG_COD = 1
											                        AND Pedido.UNIN_COD IN (1, 5) /*1=CONNECT PARTS, 5=Filial Fiscal*/
											                        AND Pedido.PEDS_EXT_COD = TCOM_PEDSAI.PEDS_EXT_COD
											                        AND Pedido.PEDS_DAT_CAN IS NULL)
										                        ,TCOM_CPLPDS.CPLP_VAL_TOT) AS valor_total
								                        ,TCOM_PEDSAI.PEDS_DAT_CAD
							                        FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
						                        INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
						                        INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
						                        INNER JOIN ABACOS.DBO.TCOM_ITEPDS WITH (NOLOCK) ON TCOM_ITEPDS.PEDS_COD	= TCOM_PEDSAI.PEDS_COD
							                        AND TCOM_ITEPDS.ITEP_QTF_PEN > 0
						                        INNER JOIN ABACOS.dbo.TCOM_PROSER (NOLOCK) ON TCOM_PROSER.PROS_COD = TCOM_ITEPDS.PROS_COD
						                        INNER JOIN ABACOS.dbo.TCOM_CLAPRO (NOLOCK) ON TCOM_CLAPRO.CLAP_COD = TCOM_PROSER.CLAP_COD
						                        INNER JOIN ABACOS.dbo.TCOM_MARPRO (NOLOCK) ON TCOM_PROSER.MARP_COD = TCOM_MARPRO.MARP_COD
						                        WHERE TCOM_PEDSAI.STAP_COD IN (8, 9)
							                        AND TCOM_PEDSAI.UNIN_COD = 1
							                        AND TCOM_PEDSAI.COME_COD NOT IN (265, 267, 217) /* LOCALIZA, FULL */
							                        AND TCOM_PEDSAI.PEDS_EXT_COD NOT LIKE '%-LOC%'

						                        UNION
                            
						                        SELECT TCOM_PEDSAI.PEDS_EXT_COD
								                        ,TCOM_PEDSAI.PEDS_COD
								                        ,TCOM_ITEPDS.ITEP_COD
								                        ,ABACOS.dbo.TCOM_PROSER.PROS_EXT_COD
								                        ,ISNULL((SELECT ISNULL(Comercial.CPLP_VAL_TOT, 0)
										                        FROM ABACOS.DBO.TCOM_PEDSAI (NOLOCK) AS Pedido
										                        INNER JOIN ABACOS.DBO.TCOM_CPLPDS (NOLOCK) AS Comercial
											                        ON Comercial.PEDS_COD = Pedido.PEDS_COD
										                        INNER JOIN ABACOS.DBO.TCOM_COMERC (NOLOCK) AS Comercializacao
											                        ON Comercializacao.COME_COD = Pedido.COME_COD
										                        WHERE Pedido.STAP_COD NOT IN (11, 12)  /*11=FATURADO, 12=CANCELADO*/
											                        AND Comercializacao.COMG_COD = 1
											                        AND Pedido.UNIN_COD IN (1, 5) /*1=CONNECT PARTS, 5=Filial Fiscal*/
											                        AND Pedido.PEDS_EXT_COD = TCOM_PEDSAI.PEDS_EXT_COD
											                        AND Pedido.PEDS_DAT_CAN IS NULL)
										                        ,TCOM_CPLPDS.CPLP_VAL_TOT) AS valor_total
								                        ,TCOM_PEDSAI.PEDS_DAT_CAD
							                        FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
						                        INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
						                        INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
						                        INNER JOIN ABACOS.DBO.TCOM_ITEPDS WITH (NOLOCK) ON TCOM_ITEPDS.PEDS_COD	= TCOM_PEDSAI.PEDS_COD
							                        AND TCOM_ITEPDS.ITEP_QTF_PEN > 0
						                        INNER JOIN ABACOS.dbo.TCOM_PROSER (NOLOCK) ON TCOM_PROSER.PROS_COD = TCOM_ITEPDS.PROS_COD
						                        INNER JOIN ABACOS.dbo.TCOM_CLAPRO (NOLOCK) ON TCOM_CLAPRO.CLAP_COD = TCOM_PROSER.CLAP_COD
						                        INNER JOIN ABACOS.dbo.TCOM_MARPRO (NOLOCK) ON TCOM_PROSER.MARP_COD = TCOM_MARPRO.MARP_COD
						                        WHERE TCOM_PEDSAI.STAP_COD IN (8, 9)
							                        AND TCOM_PEDSAI.UNIN_COD = 5
							                        AND EXISTS(SELECT 1 FROM ABACOS.dbo.TCOM_PEDSAI AS PMA WITH(NOLOCK)
										                        WHERE PMA.PEDS_EXT_COD   = TCOM_PEDSAI.PEDS_EXT_COD 
										                        AND PMA.UNIN_COD = 1  /*1=CONNECT PARTS*/
										                        AND PMA.STAP_COD IN (11) /*--11=FATURADO*/)
							                        AND TCOM_PEDSAI.COME_COD NOT IN (265, 267) /* LOCALIZA */
							                        AND TCOM_PEDSAI.PEDS_EXT_COD NOT LIKE '%-LOC%'

						                        UNION

						                        -- pedidos digitados que não existem na filial
						                        SELECT TCOM_PEDSAI.PEDS_EXT_COD
								                        ,TCOM_PEDSAI.PEDS_COD
								                        ,TCOM_ITEPDS.ITEP_COD
								                        ,ABACOS.dbo.TCOM_PROSER.PROS_EXT_COD
								                        ,ISNULL((SELECT ISNULL(Comercial.CPLP_VAL_TOT, 0)
										                        FROM ABACOS.DBO.TCOM_PEDSAI (NOLOCK) AS Pedido
										                        INNER JOIN ABACOS.DBO.TCOM_CPLPDS (NOLOCK) AS Comercial
											                        ON Comercial.PEDS_COD = Pedido.PEDS_COD
										                        INNER JOIN ABACOS.DBO.TCOM_COMERC (NOLOCK) AS Comercializacao
											                        ON Comercializacao.COME_COD = Pedido.COME_COD
										                        WHERE Pedido.STAP_COD NOT IN (11, 12)  /*11=FATURADO, 12=CANCELADO*/
											                        AND Comercializacao.COMG_COD = 1
											                        AND Pedido.UNIN_COD IN (1, 5) /*1=CONNECT PARTS, 5=Filial Fiscal*/
											                        AND Pedido.PEDS_EXT_COD = TCOM_PEDSAI.PEDS_EXT_COD
											                        AND Pedido.PEDS_DAT_CAN IS NULL)
										                        ,TCOM_CPLPDS.CPLP_VAL_TOT) AS valor_total
								                        ,TCOM_PEDSAI.PEDS_DAT_CAD
							                        FROM ABACOS.dbo.TCOM_PEDSAI WITH(NOLOCK)
						                        INNER JOIN ABACOS.dbo.TCOM_CPLPDS WITH(NOLOCK) ON TCOM_CPLPDS.PEDS_COD = TCOM_PEDSAI.PEDS_COD
						                        INNER JOIN ABACOS.dbo.TCOM_COMERC WITH(NOLOCK) ON TCOM_COMERC.COME_COD = TCOM_PEDSAI.COME_COD
						                        INNER JOIN ABACOS.DBO.TCOM_ITEPDS WITH (NOLOCK) ON TCOM_ITEPDS.PEDS_COD	= TCOM_PEDSAI.PEDS_COD
						                        INNER JOIN ABACOS.dbo.TCOM_PROSER (NOLOCK) ON TCOM_PROSER.PROS_COD = TCOM_ITEPDS.PROS_COD
						                        INNER JOIN ABACOS.dbo.TCOM_CLAPRO (NOLOCK) ON TCOM_CLAPRO.CLAP_COD = TCOM_PROSER.CLAP_COD
						                        LEFT JOIN ABACOS.dbo.TCOM_MARPRO (NOLOCK) ON TCOM_PROSER.MARP_COD = TCOM_MARPRO.MARP_COD
						                        WHERE TCOM_PEDSAI.STAP_COD IN (8, 9)
							                        AND TCOM_PEDSAI.UNIN_COD NOT IN (1, 2)		--1=CONNECT PARTS, 2=FULL FILIAL 02
							                        AND NOT EXISTS (SELECT 1 FROM ABACOS.DBO.TCOM_PEDSAI AS J WITH (NOLOCK)
											                        WHERE J.PEDS_EXT_COD = TCOM_PEDSAI.PEDS_EXT_COD
											                        AND J.PEDS_COD <> TCOM_PEDSAI.PEDS_COD
											                        AND J.STAP_COD <> 12	--12=CANCELADO
											                        AND EXISTS (SELECT 1 FROM ABACOS.DBO.TCOM_COMERC AS R WITH (NOLOCK) 
															                        WHERE R.COME_COD = J.COME_COD AND R.COMG_COD <> 1))	--1=VENDA
							                        AND TCOM_PEDSAI.COME_COD NOT IN (265, 267) /* LOCALIZA */
							                        AND TCOM_PEDSAI.PEDS_EXT_COD NOT LIKE '%-LOC%'
							                        ) Pendencias

									WHERE PROS_EXT_COD = @codigo";

            var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametro = new DynamicParameters();
                parametro.Add("codigo", codigo, DbType.String, ParameterDirection.Input);
                return await connection.QueryAsync<dynamic>(sql, parametro) ;
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioPrimeiroEnvioNucci()
        {
            string sql = @"SELECT 
                            DISTINCT IDPedido
                            ,CodigoExterno
                            ,Datacad
                            ,DataEnvio
                            ,StatusMensagem
                         FROM 
                            BDNucci_in.dbo.TbLogPedido
                         WHERE 
                            Status = 'OK' AND PedidoEnvio = 0 AND StatusPedido = 0";
            var connection = new SqlConnection(_connectionStringMysqlNucci);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioPrimeiroEnvioComErroNucci()
        {
            string sql = @"SELECT 
                                IDPedido
                                ,CodigoExterno
                                ,Datacad
                                ,MAX(DataEnvio) as DataEnvio
                                ,StatusMensagem
                            FROM 
                                BDNucci_in.dbo.TbLogPedido
                            WHERE 
                                Status = 'ERROR' 
                                AND PedidoEnvio = 0 
                                AND StatusPedido = 0 
                                AND IDPedido NOT IN (
		                            SELECT 
			                            IDPedido
		                            FROM
			                            BDNucci_in.dbo.TbLogPedido
		                            WHERE
			                            Status = 'OK')
                            GROUP BY 
	                            IDPedido
	                            ,CodigoExterno
	                            ,Datacad
	                            ,StatusMensagem";

            var connection = new SqlConnection(_connectionStringMysqlNucci);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }

        public async Task<IEnumerable<dynamic>> RelatorioAguardandoDespachoNucci()
        {
            string sql = @"SELECT 
	                        LogStatusPedido.IDPedido
                            ,LogStatusPedido.CodigoExterno
                            ,LogStatusPedido.DataCad
                            ,MAX(LogStatusPedido.DataEnvio) as DataEnvio
                            ,LogStatusPedido.StatusMensagem 
                        FROM
	                        BDNucci_in.dbo.TbLogPedido LogPedido WITH(NOLOCK)
	                        INNER JOIN BDNucci_in.dbo.TbLogUpdateStatusPedido LogStatusPedido WITH(NOLOCK) ON (LogStatusPedido.IDPedido = LogPedido.IDPedido) 
                        WHERE
	                        LogStatusPedido.Status = 'OK' 
							AND LogStatusPedido.StatusPedido = 90 
							AND LogPedido.Status = 'OK' 
							AND LogPedido.StatusPedido = 90 
							AND LogPedido.PedidoEnvio = 0
                        GROUP BY 
                            LogStatusPedido.IDPedido,
                            LogStatusPedido.CodigoExterno,
                            LogStatusPedido.DataCad,
                            LogStatusPedido.StatusMensagem";

            var connection = new SqlConnection(_connectionStringMysqlNucci);
            {
                return await connection.QueryAsync<dynamic>(sql);
            }
        }
    }
}
