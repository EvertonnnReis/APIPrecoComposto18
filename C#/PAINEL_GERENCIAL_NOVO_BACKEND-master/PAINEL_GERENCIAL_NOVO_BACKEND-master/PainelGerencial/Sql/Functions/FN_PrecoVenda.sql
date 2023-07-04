USE [CONNECTPARTS]
GO

IF OBJECT_ID('FN_PrecoVenda') IS NOT NULL
BEGIN
  DROP FUNCTION [dbo].[FN_PrecoVenda]
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.FN_PrecoVenda(@valor DECIMAL(12,2)) 
RETURNS DECIMAL(12,2)
AS
BEGIN
  DECLARE @resto DECIMAL(12,2) = @valor % 5
         ,@resultado DECIMAL(12,2);
  IF (@resto  < 2.5)
  BEGIN
    SET @resultado = @valor - @resto - 0.10;
  END
  ELSE
  BEGIN
    SET @resultado = @valor - @resto - 0.10 + 5;
  END

  RETURN @resultado;
END