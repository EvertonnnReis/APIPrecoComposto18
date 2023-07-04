using Dapper;
using Domain.Vtex;
using Microsoft.Extensions.Configuration;
using PainelGerencial.Domain;
using PainelGerencial.Domain.Vtex;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System;
using System.Data;

namespace PainelGerencial.Repository
{
    public class VtexRepository : IVtexRepository
    {
        private readonly string _connectionString;
        private readonly string _followSellers;

        public VtexRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PainelGerencialServer");
            _followSellers = configuration.GetConnectionString("FollowSellers");
        }

        public IEnumerable<PedidoFila_Model> GetPedidoFila(string codigoExterno)
        {
            const string sql = @"SELECT IdPedido
                                       ,Estado
                                       ,CodigoVtex
                                       ,Integrado
                                       ,DataAtualizacaoRegistro
                                       ,DataCriacaoRegistro 
                                       ,CodigoExterno
                                   FROM IntegracoesVtex_.dbo.PedidoFila
                                  WHERE CodigoExterno = @CodigoExterno; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("CodigoExterno", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Query<PedidoFila_Model>(sql, parametros);
            }
        }

        public int InserePedidoFila(PedidoFila_Model pedidoFila)
        {
            const string sql = @"INSERT INTO IntegracoesVtex_.dbo.PedidoFila (
                                        Estado
                                       ,CodigoVtex
                                       ,DataCriacaoRegistro 
                                       ,CodigoExterno
                                 ) VALUES (
                                        @Estado
                                       ,@CodigoVtex
                                       ,GETDATE() 
                                       ,@CodigoExterno
                                 ); ";
            var connection = new SqlConnection(_connectionString);
            {
                var parametros = new DynamicParameters();
                parametros.Add("Estado", pedidoFila.Estado, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoVtex", pedidoFila.CodigoVtex, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("CodigoExterno", pedidoFila.CodigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public List<PedidoFila_Model> PedidosProntosParaManuseio()
        {
            const string sql = @"SELECT IdPedido
                                       ,Estado
                                       ,CodigoVtex
                                       ,Integrado
                                       ,DataAtualizacaoRegistro
                                       ,DataCriacaoRegistro 
                                       ,CodigoExterno
                                   FROM IntegracoesVtex_.dbo.PedidoFila
                                  WHERE Integrado IS NULL; ";

            using var connection = new SqlConnection(_connectionString);
            {
                return (List<PedidoFila_Model>)connection.Query<PedidoFila_Model>(sql);
            }
        }

        public int AtualizaPedidoIntegrado(string codigoExterno)
        {
            const string sql = @"UPDATE IntegracoesVtex_.dbo.PedidoFila
                                    SET integrado = GETDATE()
                                       ,DataAtualizacaoRegistro = GETDATE()
                                  WHERE CodigoExterno = @pedido ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("pedido", codigoExterno, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public bool ExisteSkuFila(string RefId)
        {
            const string sql = @"SELECT 1 FROM IntegracoesVtex_.dbo.SkuFila
                                 WHERE RefId = @RefId; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("RefId", RefId, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                var resultado = connection.Query(sql, parametros);
                return resultado.Any();
            }
        }

        public int InsereSkuFila(SkuByRefIdVtexViewModel produto)
        {
            const string sql = @"INSERT INTO IntegracoesVtex_.dbo.SkuFila (
                                     ProductId
                                    ,IsActive
                                    ,ActivateIfPossible
                                    ,Name
                                    ,RefId
                                    ,PackagedHeight
                                    ,PackagedLength
                                    ,PackagedWidth
                                    ,PackagedWeightKg
                                    ,Height
                                    ,Length
                                    ,Width
                                    ,WeightKg
                                    ,CubicWeight
                                    ,IsKit
                                    ,CreationDate
                                    ,RewardValue
                                    ,EstimatedDateArrival
                                    ,ManufacturerCode
                                    ,CommercialConditionId
                                    ,MeasurementUnit
                                    ,UnitMultiplier
                                    ,ModalType
                                    ,KitItensSellApart
                                 ) VALUES (
                                     @ProductId
                                    ,@IsActive
                                    ,@ActivateIfPossible
                                    ,@Name
                                    ,@RefId
                                    ,@PackagedHeight
                                    ,@PackagedLength
                                    ,@PackagedWidth
                                    ,@PackagedWeightKg
                                    ,@Height
                                    ,@Length
                                    ,@Width
                                    ,@WeightKg
                                    ,@CubicWeight
                                    ,@IsKit
                                    ,@CreationDate
                                    ,@RewardValue
                                    ,@EstimatedDateArrival
                                    ,@ManufacturerCode
                                    ,@CommercialConditionId
                                    ,@MeasurementUnit
                                    ,@UnitMultiplier
                                    ,@ModalType
                                    ,@KitItensSellApart
                                 );";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("ProductId", produto.ProductId, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("IsActive", produto.IsActive, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("ActivateIfPossible", (bool)produto.ActivateIfPossible ? 1 : 0, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("Name", produto.Name, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("RefId", produto.RefId, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("PackagedHeight", produto.PackagedHeight, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("PackagedLength", produto.PackagedLength, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("PackagedWidth", produto.PackagedWidth, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("PackagedWeightKg", produto.PackagedWeightKg, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("Height", produto.Height, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("Length", produto.Length, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("Width", produto.Width, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("WeightKg", produto.WeightKg, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("CubicWeight", produto.CubicWeight, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("IsKit", produto.IsKit, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                parametros.Add("CreationDate", produto.CreationDate, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("RewardValue", produto.RewardValue, System.Data.DbType.Decimal, System.Data.ParameterDirection.Input);
                parametros.Add("EstimatedDateArrival", produto.EstimatedDateArrival, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("ManufacturerCode", produto.ManufacturerCode, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("CommercialConditionId", produto.CommercialConditionId, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("MeasurementUnit", produto.MeasurementUnit, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("UnitMultiplier", produto.UnitMultiplier, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("ModalType", produto.ModalType, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("KitItensSellApart", produto.KitItensSellApart, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public int AtualizaSkuFila(SkuByRefIdVtexViewModel produto)
        {
            const string sql = @"UPDATE IntegracoesVtex_.dbo.SkuFila
                                    SET ProductId               = @ProductId
                                       ,IsActive                = @IsActive
                                       ,ActivateIfPossible      = @ActivateIfPossible
                                       ,Name                    = @Name
                                       ,PackagedHeight          = @PackagedHeight
                                       ,PackagedLength          = @PackagedLength
                                       ,PackagedWidth           = @PackagedWidth
                                       ,PackagedWeightKg        = @PackagedWeightKg
                                       ,Height                  = @Height
                                       ,Length                  = @Length
                                       ,Width                   = @Width
                                       ,WeightKg                = @WeightKg
                                       ,CubicWeight             = @CubicWeight
                                       ,IsKit                   = @IsKit
                                       ,CreationDate            = @CreationDate
                                       ,RewardValue             = @RewardValue
                                       ,EstimatedDateArrival    = @EstimatedDateArrival
                                       ,ManufacturerCode        = @ManufacturerCode
                                       ,CommercialConditionId   = @CommercialConditionId
                                       ,MeasurementUnit         = @MeasurementUnit
                                       ,UnitMultiplier          = @UnitMultiplier
                                       ,ModalType               = @ModalType
                                       ,KitItensSellApart       = @KitItensSellApart
                                       ,Integrado               = NULL
                                       ,DataAtualizacaoRegistro = GETDATE()
                                  WHERE RefId = @RefId; ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("ProductId", produto.ProductId, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("IsActive", produto.IsActive, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("ActivateIfPossible", (bool)produto.ActivateIfPossible ? 1 : 0, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("Name", produto.Name, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("PackagedHeight", produto.PackagedHeight, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("PackagedLength", produto.PackagedLength, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("PackagedWidth", produto.PackagedWidth, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("PackagedWeightKg", produto.PackagedWeightKg, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("Height", produto.Height, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("Length", produto.Length, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("Width", produto.Width, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("WeightKg", produto.WeightKg, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("CubicWeight", produto.CubicWeight, System.Data.DbType.Double, System.Data.ParameterDirection.Input);
                parametros.Add("IsKit", produto.IsKit, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                parametros.Add("CreationDate", produto.CreationDate, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("RewardValue", produto.RewardValue, System.Data.DbType.Decimal, System.Data.ParameterDirection.Input);
                parametros.Add("EstimatedDateArrival", produto.EstimatedDateArrival, System.Data.DbType.DateTime, System.Data.ParameterDirection.Input);
                parametros.Add("ManufacturerCode", produto.ManufacturerCode, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("CommercialConditionId", produto.CommercialConditionId, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("MeasurementUnit", produto.MeasurementUnit, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("UnitMultiplier", produto.UnitMultiplier, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("ModalType", produto.ModalType, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("KitItensSellApart", produto.KitItensSellApart, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
                parametros.Add("RefId", produto.RefId, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public void GerarLogIntegracao(string codigoIdentificador, string tipoIdentificador, string mensagem, string metodo, int falha)
        {
            const string sql = @"IF (EXISTS(SELECT 1 FROM IntegracoesVtex_.dbo.Logs 
                                                    WHERE CodigoIdentificador = @CodigoIdentificador 
                                                      AND Metodo = @Metodo))
                                 BEGIN
                                     UPDATE IntegracoesVtex_.dbo.Logs
                                        SET TipoIdentificador = @TipoIdentificador
                                           ,Mensagem = @Mensagem
                                           ,FalhaTipoCodigo = @FalhaTipoCodigo
                                           ,DataAtualizacaoRegistro = GETDATE()
                                           ,QuantidadeFalha = QuantidadeFalha + 1
                                      WHERE CodigoIdentificador = @CodigoIdentificador 
                                        AND Metodo = @Metodo;
                                 END
                                 ELSE
                                 BEGIN
                                     INSERT INTO IntegracoesVtex_.dbo.Logs (
                                         CodigoIdentificador
                                        ,TipoIdentificador
                                        ,Mensagem
                                        ,Metodo
                                        ,FalhaTipoCodigo
                                        ,DataCriacaoRegistro
                                        ,DataAtualizacaoRegistro
                                        ,QuantidadeFalha
                                     )
                                     SELECT 
                                            @CodigoIdentificador
                                           ,@TipoIdentificador
                                           ,@Mensagem
                                           ,@Metodo
                                           ,@FalhaTipoCodigo
                                           ,GETDATE()
                                           ,GETDATE()
                                           ,@QuantidadeFalha
                                      WHERE NOT EXISTS(SELECT 1 FROM IntegracoesVtex_.dbo.Logs 
                                                        WHERE CodigoIdentificador = @CodigoIdentificador 
                                                          AND Metodo = @Metodo); 
                                 END ";

            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("CodigoIdentificador", codigoIdentificador, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("TipoIdentificador", tipoIdentificador, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("Mensagem", mensagem, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("Metodo", metodo, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("FalhaTipoCodigo", falha, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);
                parametros.Add("QuantidadeFalha", 1, System.Data.DbType.Int16, System.Data.ParameterDirection.Input);

                connection.Execute(sql, parametros);
            }
        }

        public int InserirProdutoImagem(Vtex_Anuncios_Fotos_Model anuncio)
        {
            const string sql = @"
                INSERT INTO CONNECTPARTS.dbo.vtex_anuncios_fotos(
                    id_vtex
                   ,sku
                   ,foto1 
                   ,foto2 
                   ,foto3 
                   ,foto4 
                   ,foto5 
                   ,foto6 
                   ,foto7 
                   ,foto8 
                   ,foto9 
                   ,foto10
                   ,foto11
                   ,foto12
                   ,foto13
                   ,foto14
                )
                SELECT
                       @id_vtex
                      ,@sku
                      ,@foto1 
                      ,@foto2 
                      ,@foto3 
                      ,@foto4 
                      ,@foto5 
                      ,@foto6 
                      ,@foto7 
                      ,@foto8 
                      ,@foto9 
                      ,@foto10
                      ,@foto11
                      ,@foto12
                      ,@foto13
                      ,@foto14
                 WHERE NOT EXISTS(SELECT 1 
                                    FROM CONNECTPARTS.dbo.vtex_anuncios_fotos 
                                   WHERE sku = @sku);
            ";
            using var connection = new SqlConnection(_connectionString);
            {
                DynamicParameters parametros = new DynamicParameters();
                parametros.Add("id_vtex", anuncio.id_vtex, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
                parametros.Add("sku", anuncio.sku, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto1", anuncio.foto1, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto2", anuncio.foto2, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto3", anuncio.foto3, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto4", anuncio.foto4, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto5", anuncio.foto5, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto6", anuncio.foto6, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto7", anuncio.foto7, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto8", anuncio.foto8, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto9", anuncio.foto9, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto10", anuncio.foto10, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto11", anuncio.foto11, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto12", anuncio.foto12, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto13", anuncio.foto13, System.Data.DbType.String, System.Data.ParameterDirection.Input);
                parametros.Add("foto14", anuncio.foto14, System.Data.DbType.String, System.Data.ParameterDirection.Input);

                return connection.Execute(sql, parametros);
            }
        }

        public List<string> RetornaDks()
        {
            const string sql = @"
                SELECT DISTINCT [sku] FROM [CONNECTPARTS].[dbo].[ml_anuncios_variacoes]
                 WHERE NOT EXISTS(SELECT 1 
                                    FROM CONNECTPARTS.dbo.vtex_anuncios_fotos 
                                   WHERE vtex_anuncios_fotos.sku = ml_anuncios_variacoes.sku)
            ";

            using var connection = new SqlConnection(_connectionString);
            {
                return (List<string>) connection.Query<string>(sql);
            }
        }


        //Acompanhamento Sellers..........................................................................
        public async Task<List<TbListSellersModel>> BuscaSellersSalvos(bool isActive)
        {
            string sql = @"SELECT * FROM TbSellers WHERE isActive = @isActive";

            var connection = new SqlConnection(_followSellers);
            connection.Open();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("isActive", isActive, System.Data.DbType.Boolean, System.Data.ParameterDirection.Input);
            var query = await connection.QueryAsync<TbListSellersModel>(sql, parameters);
            connection.Close();
            return query.ToList();
        }

        public async Task<dynamic> UpdateSellers(List<TbListSellersModel> listSellers)
        {
            List<dynamic> linhasAfetadas = new List<dynamic>();

            using (SqlConnection connection = new SqlConnection(_followSellers))
            {
                connection.Open();
                string sql = @"MERGE TbSellers AS destino
                         USING(VALUES (@idSeller, @nameSeller, @isActive))AS origem(idSeller, nameSeller, isActive)
                    ON destino.idSeller = origem.idSeller

                    WHEN MATCHED THEN
                        UPDATE SET
                            destino.nameSeller = origem.nameSeller,
                            destino.isActive = origem.isActive
                    WHEN NOT MATCHED THEN
                        INSERT (idSeller, nameSeller, isActive)
                        VALUES(origem.idSeller, origem.nameSeller, origem.isActive);";

                foreach (var item in listSellers)
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@idSeller", item.idSeller);
                        command.Parameters.AddWithValue("@nameSeller", item.nameSeller);
                        command.Parameters.AddWithValue("@isActive", item.isActive ? 1 : 0);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        linhasAfetadas.Add(new { Seller = item, Success = rowsAffected});
                    };
                }
                connection.Close();
                
            }

            return linhasAfetadas;
            
        }

        public TbPedidosModel buscaUltimoPedido()
        {
            string sql = @"SELECT * FROM TbPedidos WHERE CreationDate = (SELECT MAX(CreationDate) FROM TbPedidos)";
            SqlConnection connection = new SqlConnection(_followSellers);
            connection.Open();
            return connection.QueryAsync<TbPedidosModel>(sql).Result.FirstOrDefault();
        }

        public List<TbPedidosModel> buscaTodos()
        {
            string sql = @"SELECT * FROM TbPedidos";
            SqlConnection connection = new SqlConnection(_followSellers);
            connection.Open();
            return connection.QueryAsync<TbPedidosModel>(sql).Result.ToList();
        }

        public async Task<dynamic> NovosPedidos(List<TbPedidosModel> listPedidos)
        {
            using (var connection = new SqlConnection(_followSellers))
            {
                connection.Open();
                List<dynamic> ListaReturn = new List<dynamic>();
                foreach (var item in listPedidos)
                {
                    using ( var Transaction =  connection.BeginTransaction() )
                    {
                        try
                        {
                            string sql = @"INSERT INTO TbPedidos
                                            (OrderID, CreationDate, Status, StatusDescription, LastChange, SellerID)
                                        SELECT @OrderID, @CreationDate, @Status, @StatusDescription, @LastChange, @SellerID
                                        WHERE NOT EXISTS(
                                            SELECT 1
                                            FROM TbPedidos
                                            WHERE OrderID = @OrderID
                                        )";

                            string sqlLog = @"INSERT INTO TbLogPedidos
                                            (OrderID, Status, StatusDescription, LastChange)
                                        SELECT @OrderID, @Status, @StatusDescription, @LastChange
                                        WHERE NOT EXISTS(
                                            SELECT 1
                                            FROM TbLogPedidos
                                            WHERE OrderID = @OrderID
                                        )";
                            DynamicParameters parameter = new DynamicParameters();
                            parameter.Add("OrderID", item.OrderID, DbType.String, ParameterDirection.Input);
                            parameter.Add("CreationDate", item.CreationDate, DbType.DateTime, ParameterDirection.Input);
                            parameter.Add("Status", item.Status, DbType.String, ParameterDirection.Input);
                            parameter.Add("StatusDescription", item.StatusDescription, DbType.String, ParameterDirection.Input);
                            parameter.Add("LastChange", item.LastChange, DbType.DateTime, ParameterDirection.Input);
                            parameter.Add("SellerID", item.SellerID, DbType.String, ParameterDirection.Input);
                            int rowsAffected = await connection.ExecuteAsync(sql, parameter, Transaction);

                            DynamicParameters parameterLog = new DynamicParameters();
                            parameterLog.Add("OrderID", item.OrderID, DbType.String, ParameterDirection.Input);
                            parameterLog.Add("Status", item.Status, DbType.String, ParameterDirection.Input);
                            parameterLog.Add("StatusDescription", item.StatusDescription, DbType.String, ParameterDirection.Input);
                            parameterLog.Add("LastChange", item.LastChange, DbType.DateTime, ParameterDirection.Input);
                            int rowsLogAffected = await connection.ExecuteAsync(sqlLog, parameter, Transaction);

                            if(rowsAffected > 0 && rowsLogAffected > 0)
                            {
                                Transaction.Commit();
                                ListaReturn.Add(new { item.OrderID, OK = true });
                            }

                        }
                        catch (Exception ex)
                        {
                            ListaReturn.Add(new { item.OrderID, OK = false });
                            return new { Error = ex.Message, ListaReturn};
                        }
                    }
                }
                return ListaReturn;
            }
            
        }


        public async Task<dynamic> UpdatePedidos(List<TbPedidosModel> listPedidos)
        {
            using (var connection = new SqlConnection(_followSellers))
            {
                connection.Open();
                List<dynamic> ListaReturn = new List<dynamic>();
                foreach (var item in listPedidos)
                {
                    using (var Transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string sql = @"UPDATE TbPedidos
                                           SET 
                                            Status = @Status, 
                                            StatusDescription = @StatusDescription, 
                                            LastChange = @LastChange
                                         WHERE
                                            OrderID = @OrderID";

                            string sqlLog = @"INSERT INTO TbLogPedidos
                                                (OrderID, Status, StatusDescription, LastChange)
                                            VALUES
                                                (@OrderID, @Status, @StatusDescription, @LastChange)";
                            DynamicParameters parameter = new DynamicParameters();
                            parameter.Add("OrderID", item.OrderID, DbType.String, ParameterDirection.Input);
                            parameter.Add("CreationDate", item.CreationDate, DbType.DateTime, ParameterDirection.Input);
                            parameter.Add("Status", item.Status, DbType.String, ParameterDirection.Input);
                            parameter.Add("StatusDescription", item.StatusDescription, DbType.String, ParameterDirection.Input);
                            parameter.Add("LastChange", item.LastChange, DbType.DateTime, ParameterDirection.Input);
                            parameter.Add("SellerID", item.SellerID, DbType.String, ParameterDirection.Input);
                            int rowsAffected = await connection.ExecuteAsync(sql, parameter, Transaction);

                            DynamicParameters parameterLog = new DynamicParameters();
                            parameterLog.Add("OrderID", item.OrderID, DbType.String, ParameterDirection.Input);
                            parameterLog.Add("Status", item.Status, DbType.String, ParameterDirection.Input);
                            parameterLog.Add("StatusDescription", item.StatusDescription, DbType.String, ParameterDirection.Input);
                            parameterLog.Add("LastChange", item.LastChange, DbType.DateTime, ParameterDirection.Input);
                            int rowsLogAffected = await connection.ExecuteAsync(sqlLog, parameter, Transaction);

                            if (rowsAffected > 0 && rowsLogAffected > 0)
                            {
                                Transaction.Commit();
                                ListaReturn.Add(new { item.OrderID, OK = true });
                            }

                        }
                        catch (Exception ex)
                        {
                            ListaReturn.Add(new { item.OrderID, OK = false });
                            return new { Error = ex.Message, ListaReturn };
                        }
                    }
                }
                return ListaReturn;
            }

        }
    }
}
