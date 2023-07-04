using Dapper;
using Microsoft.Extensions.Configuration;
using PainelGerencial.Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public class DropshippingRepository : IDropshippingRepository
    {
        private readonly string _connectionString;

        public DropshippingRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PainelGerencialServer");
        }

        // Altera o pedido de Pessoa jurídica(CNPJ) para Pessoa Física(CPF)
        public int AlteraPedidoCnpjCpf(string pedidoCodigoExterno)
        {
            const string sql = @"EXEC Dropshipping.dbo.SP_AlteraPedidoCnpjCpf 
                                      @PEDS_EXT_COD = @pedidoCodigoExterno
                                     ,@debug = 0; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedidoCodigoExterno", pedidoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        // Consulta o pedido dropshipping
        public async Task<IEnumerable<Pedido_Model>> PedidoDropshipping(string pedidoCodigoExterno)
        {
            const string sql = @"SELECT Pedidos.CodigoExterno
                                       ,Pedidos.PedidoConnect
                                       ,Pedidos.EmailEnviado
                                       ,Pedidos.DataConfirmacao
                                       ,Pedidos.DataAtualizacaoRegistro
                                       ,Pedidos.DataCriacaoRegistro
                                       ,PedidoStatus.codigo
                                       ,PedidoStatus.nome
                                       ,PedidoStatus.Descricao
                                   FROM Dropshipping.dbo.Pedidos
                                  INNER JOIN Dropshipping.dbo.PedidoStatus
                                     ON PedidoStatus.codigo = Pedidos.PedidoStatusCodigo
                                  WHERE CodigoExterno = SUBSTRING(@pedido, 1, CHARINDEX('-', @pedido, 1)-1); ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedido", pedidoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return await connection.QueryAsync<Pedido_Model, PedidoStatus_Model, Pedido_Model>(
                    sql: sql, 
                    map: (pedido, status) =>
                    {
                        pedido.pedidoStatus = status;
                        return pedido;
                    },
                    param: parametros,
                    splitOn: "CodigoExterno,codigo");
            }
        }

        public IEnumerable<NotaFiscal_Model> NotaFiscal(string pedidoCodigoExterno)
        {
            const string sql = @"SELECT PedidoFornecedorCodigo
                                       ,Numero
                                       ,Rastreio
                                       ,Chave
                                       ,DataEmissao
                                       ,DataConfirmacao
                                       ,DataAtualizacaoRegistro
                                       ,DataCriacaoRegistro
                                       ,Enviado
                                   FROM Dropshipping.dbo.Notasfiscais
                                  WHERE PedidoFornecedorCodigo = @pedido; ";
            
            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedido", pedidoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query<NotaFiscal_Model>(sql, parametros);
            }
        }

        public async Task<IEnumerable<PedidoFornecedor_Model>> PedidoFornecedores(string pedidoCodigoExterno)
        {
            const string sql = @"SELECT PedidoFornecedores.PedidoFornecedorCodigo
                                       ,PedidoFornecedores.PedidoCodigo
                                       ,PedidoFornecedores.FornecedorCodigo
                                       ,PedidoFornecedores.DataConfirmacao
                                       ,PedidoFornecedores.DataAtualizacaoRegistro
                                       ,PedidoFornecedores.DataCriacaoRegistro
                                       ,PedidoFornecedores.CodigoErpFornecedor
                                       ,PedidoFornecedores.PedidoStatusCodigo
                                       ,PedidoStatus.Codigo
                                       ,PedidoStatus.Nome
                                       ,PedidoStatus.Descricao
                                 FROM Dropshipping.dbo.PedidoFornecedores
                                INNER JOIN Dropshipping.dbo.PedidoStatus
                                   ON PedidoStatus.Codigo = PedidoFornecedores.PedidoStatusCodigo
                                WHERE PedidoFornecedorCodigo = @pedido; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedido", pedidoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return await connection.QueryAsync<PedidoFornecedor_Model, PedidoStatus_Model, PedidoFornecedor_Model>(
                    sql: sql,
                    map: (pedidoFornecedor, status) =>
                    {
                        pedidoFornecedor.pedidoStatus = status;
                        return pedidoFornecedor;
                    },
                    param: parametros,
                    splitOn: "Codigo"
                );
            }
        }

        public int PedidoFornecedoresAlteraStatus(string codigoExterno, int status)
        {
            const string sql = @"UPDATE Dropshipping.dbo.PedidoFornecedores 
                                    SET PedidoStatusCodigo = @status
                                  WHERE PedidoFornecedorCodigo = @pedido; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedido", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("status", status, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public IEnumerable<dynamic> PedidosFaturadosSemAndamento()
        {
            const string sql = @"SELECT DISTINCT TCOM_PEDSAI.PEDS_EXT_COD, PedidoFornecedores.PedidoStatusCodigo, PedidoStatus.Nome
                                   FROM ABACOS.dbo.TCOM_PEDSAI (NOLOCK)
                                  INNER JOIN Dropshipping.dbo.PedidoFornecedores (NOLOCK)
                                     ON PedidoFornecedorCodigo = TCOM_PEDSAI.PEDS_EXT_COD
                                    AND PedidoStatusCodigo < 5
                                    AND DataCriacaoRegistro >= DATEADD(DAY, -70, getdate())
                                  INNER JOIN Dropshipping.dbo.PedidoStatus
                                     ON PedidoStatus.Codigo = PedidoFornecedores.PedidoStatusCodigo
                                  WHERE TCOM_PEDSAI.STAP_COD IN (11, 27); ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<dynamic>(sql);
            }
        }

        public IEnumerable<PedidoFornecedor_Model> PedidosSemNotaFiscal()
        {
            const string sql = @"SELECT PedidoFornecedorCodigo
                                       ,PedidoCodigo
                                       ,DataConfirmacao
                                   FROM Dropshipping.dbo.PedidoFornecedores
                                  WHERE PedidoStatusCodigo BETWEEN 4 AND 6
                                    AND PedidoFornecedorCodigo LIKE '%-GLB'
                                    AND NOT EXISTS(SELECT 1 FROM Dropshipping.dbo.Notasfiscais
                                                    WHERE Notasfiscais.PedidoFornecedorCodigo = PedidoFornecedores.PedidoFornecedorCodigo); ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Query<PedidoFornecedor_Model>(sql);
            }
        }

        public int VoltaStatusPedidosSemNotaFiscal()
        {
            const string sql = @"UPDATE Dropshipping.dbo.PedidoFornecedores 
                                    SET PedidoStatusCodigo = 3
                                  WHERE PedidoStatusCodigo BETWEEN 4 AND 6
                                    AND PedidoFornecedorCodigo LIKE '%-GLB'
                                    AND NOT EXISTS(SELECT 1 FROM Dropshipping.dbo.Notasfiscais
                                                    WHERE Notasfiscais.PedidoFornecedorCodigo = PedidoFornecedores.PedidoFornecedorCodigo); ";

            using var connection = new SqlConnection(_connectionString);
            {
                return connection.Execute(sql);
            }
        }

        public bool ExistePedido(string codigoExterno)
        {
            const string sql = @"SELECT 1
                                   FROM Dropshipping.dbo.Pedidos
                                  WHERE CodigoExterno = @codigoExterno; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("codigoExterno", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query(sql, parametros).Any();
            }
        }

        public bool ExistePedidoFornecedores(string codigoExterno)
        {
            const string sql = @"SELECT 1
                                   FROM Dropshipping.dbo.PedidoFornecedores
                                  WHERE PedidoFornecedorCodigo = @codigoExterno; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("codigoExterno", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query(sql, parametros).Any();
            }
        }

        public int InserePedido(Pedido_Model pedido)
        {
            const string sql = @"INSERT INTO Dropshipping.dbo.Pedidos (
                                        CodigoExterno
                                       ,PedidoStatusCodigo
                                       ,PedidoConnect
                                       ,EmailEnviado
                                       ,DataAtualizacaoRegistro
                                       ,DataCriacaoRegistro
                                 ) VALUES (
                                        @CodigoExterno
                                       ,@PedidoStatusCodigo
                                       ,@PedidoConnect
                                       ,@EmailEnviado
                                       ,@DataAtualizacaoRegistro
                                       ,@DataCriacaoRegistro
                                 );";
            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("CodigoExterno", pedido.CodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("PedidoStatusCodigo", pedido.PedidoStatusCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("PedidoConnect", pedido.PedidoConnect, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("EmailEnviado", pedido.EmailEnviado, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("DataAtualizacaoRegistro", pedido.DataAtualizacaoRegistro, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("DataCriacaoRegistro", pedido.DataCriacaoRegistro, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int InserePedidoFornecedores(PedidoFornecedor_Model pedidoFornecedor)
        {
            const string sql = @"INSERT INTO Dropshipping.dbo.PedidoFornecedores (
                                        PedidoFornecedorCodigo
                                       ,PedidoCodigo
                                       ,FornecedorCodigo
                                       ,PedidoStatusCodigo
                                       ,DataAtualizacaoRegistro
                                       ,DataCriacaoRegistro
                                       ,CodigoErpFornecedor
                                 ) VALUES (
                                        @PedidoFornecedorCodigo
                                       ,@PedidoCodigo
                                       ,@FornecedorCodigo
                                       ,@PedidoStatusCodigo
                                       ,@DataAtualizacaoRegistro
                                       ,@DataCriacaoRegistro
                                       ,@CodigoErpFornecedor
                                 );";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("PedidoFornecedorCodigo", pedidoFornecedor.PedidoFornecedorCodigo, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("PedidoCodigo", pedidoFornecedor.PedidoCodigo, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("FornecedorCodigo", pedidoFornecedor.FornecedorCodigo, System.Data.DbType.Guid, System.Data.ParameterDirection.Input);
                parametros.Add("DataConfirmacao", pedidoFornecedor.PedidoStatusCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("PedidoStatusCodigo", pedidoFornecedor.PedidoStatusCodigo, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("DataAtualizacaoRegistro", pedidoFornecedor.DataAtualizacaoRegistro, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("DataCriacaoRegistro", pedidoFornecedor.DataCriacaoRegistro, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoErpFornecedor", pedidoFornecedor.CodigoErpFornecedor, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }        

        public int InserePedidoFornecedorProdutos(string pedidoCodigoExterno)
        {
            const string sql = @"DECLARE @ped_int INT;

                                 -- Codigo interno do pedido no Abacos
                                 SELECT @ped_int = PEDS_COD 
                                 FROM abacos.dbo.TCOM_PEDSAI 
                                 WHERE PEDS_EXT_COD = @pedidoCodigoExterno 
                                 AND UNIN_COD = 5;

                                 INSERT INTO Dropshipping.dbo.PedidoFornecedorProdutos(PedidoFornecedorCodigo, ProdutoCodigo, Quantidade, PrecoUnitarioVenda, PrecoUnitarioCusto, DataCriacaoRegistro, DataAtualizacaoRegistro)
                                 SELECT @pedidoCodigoExterno, TCOM_PROSER.PROS_EXT_COD, SUM(TCOM_ITEPDS.ITEP_QTF_PED), MAX(ITEP_VAL_PREBRU) * 100, MAX(ITEP_VAL_CUSUNI) * 100, GETDATE(), GETDATE()
                                   FROM ABACOS.dbo.TCOM_ITEPDS 
                                  INNER JOIN abacos.dbo.TCOM_PROSER
                                     ON TCOM_PROSER.PROS_COD = TCOM_ITEPDS.PROS_COD
                                  WHERE PEDS_COD = @ped_int
                                    AND NOT EXISTS (SELECT 1 FROM Dropshipping.dbo.PedidoFornecedorProdutos 
                                                     WHERE PedidoFornecedorCodigo = @pedidoCodigoExterno)
                                  GROUP BY TCOM_PROSER.PROS_EXT_COD;
                                ";
            using SqlConnection connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedidoCodigoExterno", pedidoCodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

    }
}
