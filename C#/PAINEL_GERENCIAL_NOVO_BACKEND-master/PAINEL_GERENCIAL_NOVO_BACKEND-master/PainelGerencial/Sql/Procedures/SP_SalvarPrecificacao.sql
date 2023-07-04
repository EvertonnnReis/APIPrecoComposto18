USE [CONNECTPARTS]
GO

IF OBJECT_ID('SP_SalvarPrecificacao') IS NOT NULL
BEGIN
    DROP PROCEDURE [dbo].[SP_SalvarPrecificacao]
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_SalvarPrecificacao] (
    @ProdutoCodigoExterno VARCHAR(30)
   ,@listaPreco INT
   ,@Email VARCHAR(255)
   ,@ValorDefinido DECIMAL(12,2)
   ,@ValorMinimoCalculado DECIMAL(12,2)
   ,@Codigo INT
) AS
BEGIN
    DECLARE @dkPai VARCHAR(40) = '';

    -- Produtos Filhos
    DECLARE @ProdutosFilhos TABLE(ProdutoCodigoExterno VARCHAR(30), ProdutoCodigoExternoPai VARCHAR(30));
    INSERT INTO @ProdutosFilhos(ProdutoCodigoExterno, ProdutoCodigoExternoPai)
    SELECT Produtos.PROS_EXT_COD, ProdutosPai.PROS_EXT_COD
      FROM ABACOS.dbo.TCOM_PROSER Produtos (NOLOCK)
     INNER JOIN ABACOS.dbo.TCOM_PROSER ProdutosPai (NOLOCK)
        ON ProdutosPai.PROS_COD = Produtos.PROS_COD_PAI
     WHERE ProdutosPai.PROS_EXT_COD = @ProdutoCodigoExterno;

    UPDATE ConnectParts.dbo.Precificacoes_Temporarias
       SET ValorDefinido           = @ValorDefinido
          ,TabelaPreco             = @listaPreco
          ,ValorMinimoCalculado    = @ValorMinimoCalculado
     WHERE Id = @Codigo; 

    -- Atualiza os filhos
    UPDATE CONNECTPARTS.dbo.Precificacoes_Temporarias
       SET valorDefinido = @ValorDefinido
          ,valorMinimoCalculado = @ValorMinimoCalculado
      FROM CONNECTPARTS.dbo.Precificacoes_Temporarias
     WHERE ProdutoCodigoExterno IN (SELECT Pai.PROS_EXT_COD
                                      FROM ABACOS.dbo.TCOM_PROSER (NOLOCK) AS Pai
                                     WHERE Pai.PROS_COD_PAI IN (SELECT PROS_COD 
                                                                  FROM ABACOS.dbo.TCOM_PROSER 
                                                                 WHERE PROS_EXT_COD = @ProdutoCodigoExterno)
                                       AND Pai.PROS_COD <> Pai.PROS_COD_PAI)
       AND Email = @Email;

    -- Atualiza o percentual
    UPDATE CONNECTPARTS.dbo.Precificacoes_Temporarias
       SET PercentualAjuste = CASE WHEN valorDefinido = 0 AND valorPor = 0 THEN 0
                                  ELSE (valorDefinido - produtoCusto) / (CASE WHEN valorDefinido > 0 THEN valorDefinido ELSE valorPor END) * 100
                              END
      FROM CONNECTPARTS.dbo.Precificacoes_Temporarias
     WHERE ProdutoCodigoExterno IN (SELECT Pai.PROS_EXT_COD
                                      FROM ABACOS.dbo.TCOM_PROSER (NOLOCK) AS Pai
                                     WHERE Pai.PROS_COD_PAI IN (SELECT PROS_COD 
                                                                  FROM ABACOS.dbo.TCOM_PROSER 
                                                                 WHERE PROS_EXT_COD = @ProdutoCodigoExterno)
                                       AND Pai.PROS_COD <> Pai.PROS_COD_PAI)
       AND Email = @Email;

    -- Se estiver editando o pai
	IF EXISTS(SELECT 1 FROM CONNECTPARTS.dbo.Precificacoes_Temporarias 
               WHERE Precificacoes_Temporarias.ProdutoCodigoExternoPai = @ProdutoCodigoExterno
                 AND Email = @Email)
	BEGIN
        -- Atualiza os produtos que contém o kit
        -- inicio
        DECLARE @pros_cod_pai INT = 0, @pros_cod int, @dk VARCHAR(30), @valorCalculado DECIMAL(12,2);
        DECLARE @ComponentesCalc TABLE (PROS_COD INT, ProdutoCodigoExterno VARCHAR(40) COLLATE Latin1_General_CI_AS, ValorCalculado DECIMAL(12,2));

        DECLARE crs_Pais CURSOR FOR
        SELECT PROS_COD, PROS_EXT_COD, Precificacoes_Temporarias.ProdutoCodigoExternoPai
          FROM CONNECTPARTS.dbo.Precificacoes_Temporarias
         INNER JOIN ABACOS.dbo.TCOM_PROSER
            ON TCOM_PROSER.PROS_EXT_COD = Precificacoes_Temporarias.ProdutoCodigoExterno
         WHERE Email = @Email
           AND Kit = 1
           AND EXISTS(SELECT 1 
                        FROM ABACOS.dbo.TCOM_COMPRO Componentes (NOLOCK) 
                       INNER JOIN ABACOS.dbo.TCOM_PROSER ProdutoComponente (NOLOCK) 
                          ON (Componentes.COMP_COD_PRO = ProdutoComponente.PROS_COD) 
                       WHERE Componentes.PROS_COD = TCOM_PROSER.PROS_COD
                         AND ProdutoComponente.PROS_EXT_COD IN (SELECT ProdutoCodigoExterno COLLATE SQL_Latin1_General_CP1_CI_AS
                                                                  FROM @ProdutosFilhos) );

        OPEN crs_Pais;

        FETCH NEXT FROM crs_Pais
         INTO @pros_cod, @dk, @dkPai;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DELETE FROM @ComponentesCalc;

            INSERT INTO @ComponentesCalc(PROS_COD, ProdutoCodigoExterno, ValorCalculado)
	        SELECT ProdutoComponente.PROS_COD, ProdutoComponente.PROS_EXT_COD
                  ,CASE WHEN Precificacoes_Temporarias.ProdutoCodigoExterno IS NULL THEN 
                       ABACOS.dbo.TCOM_PROLIS.PROL_VAL_PREPRO * TCOM_COMPRO.COMP_QTF
                   ELSE @ValorDefinido * TCOM_COMPRO.COMP_QTF
                   END
              FROM ABACOS.dbo.TCOM_COMPRO
             INNER JOIN ABACOS.dbo.TCOM_PROSER
                ON ABACOS.dbo.TCOM_PROSER.PROS_COD = ABACOS.dbo.TCOM_COMPRO.PROS_COD
             INNER JOIN ABACOS.dbo.TCOM_PROSER AS ProdutoComponente
                ON ProdutoComponente.PROS_COD = ABACOS.dbo.TCOM_COMPRO.COMP_COD_PRO
             INNER JOIN ABACOS.dbo.TCOM_PROLIS
                ON ABACOS.dbo.TCOM_PROLIS.PROS_COD = ABACOS.dbo.TCOM_COMPRO.COMP_COD_PRO
               AND ABACOS.dbo.TCOM_PROLIS.LISP_COD = @listaPreco
              LEFT JOIN CONNECTPARTS.dbo.Precificacoes_Temporarias
                ON Precificacoes_Temporarias.ProdutoCodigoExterno = ProdutoComponente.PROS_EXT_COD
               AND Precificacoes_Temporarias.Email = @Email
             WHERE ABACOS.dbo.TCOM_PROSER.PROS_EXT_COD = @dk;

            SELECT @valorCalculado = ISNULL(SUM(ISNULL(ValorCalculado, 0)), 0)
	          FROM @ComponentesCalc;

            -- Atualiza valor 
            UPDATE CONNECTPARTS.dbo.Precificacoes_Temporarias
               SET ValorMinimoCalculado = @valorCalculado 
                  ,valorDefinido = @valorCalculado 
              FROM CONNECTPARTS.dbo.Precificacoes_Temporarias
             WHERE ProdutoCodigoExterno IN (@dk, @dkPai)
               AND ProdutoCodigoExterno <> @ProdutoCodigoExterno
               AND Email = @Email;


            -- Preço Vendável
            UPDATE CONNECTPARTS.dbo.Precificacoes_Temporarias
               SET valorDefinido = CASE WHEN (valorDefinido % 5) <= 2.5 THEN 
                                       valorDefinido - (valorDefinido % 5) - 0.10
                                   ELSE
                                       valorDefinido - (valorDefinido % 5) - 0.10 + 5
                                   END
              FROM CONNECTPARTS.dbo.Precificacoes_Temporarias
             WHERE Precificacoes_Temporarias.ProdutoCodigoExterno IN (@dk, @dkPai)
               AND ProdutoCodigoExterno <> @ProdutoCodigoExterno
               AND Precificacoes_Temporarias.Email = @Email;

            -- Atualiza percentual
            UPDATE CONNECTPARTS.dbo.Precificacoes_Temporarias
               SET PercentualAjuste = CASE WHEN valorDefinido = 0 AND valorPor = 0 THEN 0
                                      ELSE (valorDefinido - produtoCusto) / (CASE WHEN valorDefinido > 0 THEN valorDefinido ELSE valorPor END) * 100
                                      END
              FROM CONNECTPARTS.dbo.Precificacoes_Temporarias
             WHERE Precificacoes_Temporarias.ProdutoCodigoExterno IN (@dk, @dkPai)
               AND ProdutoCodigoExterno <> @ProdutoCodigoExterno
               AND Precificacoes_Temporarias.Email = @Email;

            FETCH NEXT FROM crs_Pais
             INTO @pros_cod, @dk, @dkPai;
        END

        CLOSE crs_Pais;
        DEALLOCATE crs_Pais;
        -- fim
    END

    -- Define se o preço foi alterado ou não
    UPDATE ConnectParts.dbo.Precificacoes_Temporarias
       SET Alterado = CASE 
                        WHEN dbo.FN_PrecoVenda(ValorMinimoCalculado) != ValorDefinido THEN 1
                        ELSE 0
                      END
     WHERE Email = @Email;
END
GO